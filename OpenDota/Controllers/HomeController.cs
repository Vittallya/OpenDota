using OpenDota.Models;
using OpenDota.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OpenDota.DAL;
using System.Data.Entity;

namespace OpenDota.Controllers
{
    public class HomeController : Controller
    {
        static RoomService _roomService;


        public async Task<ActionResult> Index()
        {
            if (_roomService == null)
                _roomService = new RoomService();

            if (!_roomService.IsHeroesLoaded)
            {

                HttpClient client = new HttpClient();

                HttpResponseMessage message = await client.GetAsync("https://api.opendota.com/api/heroes");
                string content = await message.Content.ReadAsStringAsync();

                var heroes = JsonConvert.DeserializeObject<Hero[]>(content);
                _roomService.LoadHeroes(heroes);
            }

            return View();
        }

        [HttpGet]
        public ActionResult GenerateLink()
        {
            var room = _roomService.CreateRoom();
            string link = "https://" + HttpContext.Request.Url.Authority + Url.Action(nameof(JoinRoom), new { RoomId = room.Id });// "/Room/Index?roomId=" + roomId;
            room.SetupLink(link);


            return PartialView("LinkView", new RoomModel(link, room.Id));
        }

        public ActionResult JoinRoom(RoomModel model)
        {
            var room = _roomService[model.RoomId];

            if(room != null)
            {
                model.Link = room.Link;

                if(room.RoomStatus == RoomStatus.Pause)
                {
                    room.JoinUser(model.IsCreator);

                    //model.Heroes = room.GetHeroesFor(model.IsCreator);
                    //model.IsBlocking = room.IsBanMode();
                    //model.IsReturn = true;


                    return View("RoomView", model);
                }
                else if(room.RoomStatus == RoomStatus.Completed)
                {
                    return RedirectToAction("ShowResult", new { roomId = model.RoomId });
                }

            }

            var res = _roomService.ConnectToRoom(model.RoomId, model.IsCreator);


            if (!res.IsOk)
            {
                Response.StatusCode = 404;
                return Content(res.Message);
            }
            //поиск комнаты, если нашел, проверить, находится ли она в режиме ожидания, если да - присоединение
            return View("RoomView", model);
        }

        [HttpPost]
        public void OnUserUnload(RoomModel model)
        {
            var room = _roomService[model.RoomId];
            if(room != null)
            {
                room.UserUnloaded(model.IsCreator);
            }

        }


        public async Task<ActionResult> StartMatch(RoomModel model)
        {
            var room = _roomService[model.RoomId];
            
            while (room.RoomStatus == RoomStatus.AwaitOnCreate || room.RoomStatus == RoomStatus.Pause)
            {
                await Task.Delay(200);                
            }

            model.IsBlocking = room.IsBanMode();

            if (room.IsAwaitOpponent(model.IsCreator))
            {
                return PartialView("ConnectionAwaitView", model);
            }
            model.Heroes = room.GetHeroesFor(model.IsCreator);
            return PartialView("ChooseHeroView", model);
        }
        public ActionResult SelectHero(RoomModel model)
        {
            var room = _roomService[model.RoomId];

            if(room == null)
            {
                Response.StatusCode = 404;
                return Content("Комната не найдена");
            }


            if (!model.IsBlocking)
                room.SelectHero(model.IsCreator, model.HeroSelected);
            else
                room.BanHero(model.IsCreator, model.HeroSelected);

            model.IsBlocking = room.IsBanMode();

            return PartialView("ConnectionAwaitView", model);

        }


        public async Task<ActionResult> AwaitOpponent(RoomModel model)
        {
            var room = _roomService[model.RoomId];

            while (room.IsAwaitOpponent(model.IsCreator) || room.IsSavingProccesNow())
            {
                if (!Prognozer.IsLoaded)
                {
                    await Prognozer.LoadMatches();
                }
                else
                {
                    await Task.Delay(200);
                }
            }

            if (!room.CanChoose())
            {
                if (!room.IsResultSaved)
                {
                    await SaveResults(room);
                }

                return RedirectToAction("ShowResult", new { roomId = room.Id, isShowLink = true });
            }

            model.Heroes = room.GetHeroesFor(model.IsCreator);

            model.IsBlocking = room.IsBanMode();
            //проверить, можно ли пользователю выбрать еще кого-то
            //-да: переход в режим ожидания, пердставление ожидания
            //-нет: переход в финальную страницу


            return PartialView("ChooseHeroView", model);
        }
        public ActionResult GetActualSelection(int roomId)
        {
            var room = _roomService[roomId];

            if(room.RoomStatus == RoomStatus.AwaitOnCreate || room.RoomStatus == RoomStatus.Pause)
            {
                return null;
            }

            return PartialView("ActualHeroes", new ActualHeroesModel
            {
                User1Heroes = room.GetAllHeroesSelected(true),
                User2Heroes = room.GetAllHeroesSelected(false),
                User1Blocked = room.GetAllHeroesBlocked(true),
                User2Blocked = room.GetAllHeroesBlocked(false)
            });

        }

        private Prognozer prognozer = new Prognozer();

        [HttpGet]
        public async Task<ActionResult> ShowResult(int roomId, bool? isShowLink)
        {
            using(var context = new RoomContext())
            {
                await context.RoomResult.LoadAsync();

                var room = context.RoomResult.Find(roomId);
                if(room == null)
                {
                    Response.StatusCode = 404;
                    return Content("Данная комната не найдена");
                }

                string link = null;

                if(isShowLink.HasValue && isShowLink.Value)
                {
                    link = "https://" + HttpContext.Request.Url.Authority + Url.Action(nameof(ShowResult), new { roomId = roomId });
                }


                return View("ResultView", new ResultModel
                {
                    RoomId = room.RoomId,
                    Link = link,
                    CreatorsHeroes = room.CreatorHeroes.Select(x => x.Name).ToArray(),
                    OpponentHeroes = room.OppHeroes.Select(x => x.Name).ToArray(),
                    Result = room.Result,
                    Comment = room.Comment,
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ShowResult(ResultModel model)
        {
            using (var db = new RoomContext())
            {
                var room = await db.RoomResult.FindAsync(model.RoomId);
                room.Comment = model.Comment;
                await db.SaveChangesAsync();
            }

            return Content("<h3>"+ model.Comment+"</h3>");
        }


        string error;

        private async Task<RoomResult> SaveResults(RoomSession room)
        {
            room.ResultCalled = true;
            int res = await prognozer.GetResult(room[true].SelectedHeroes.ToArray(), room[false].SelectedHeroes.ToArray());
            room.SetResult(res);
            var roomRes = GetRoomRes(room);
            await SaveRoomRes(roomRes);
            room.CompleteCompetition();

            return roomRes;
        }
        private RoomResult GetRoomRes(RoomSession room)
        {
            var creator = room[true];
            var opp = room[false];

            var creatorHeroes = creator.SelectedHeroes.
                Select(x => _roomService.AllHeroes.FirstOrDefault(y => y.id == x)).
                Select(x => new HeroResult { HeroId = x.id, Name = x.localized_name, ChoosenByCreator = true, RoomResultId = room.Id }).
                ToArray();

            var oppHeroes = opp.SelectedHeroes.
                Select(x => _roomService.AllHeroes.FirstOrDefault(y => y.id == x)).
                Select(x => new HeroResult { HeroId = x.id, Name = x.localized_name, RoomResultId = room.Id }).
                ToArray();

            var roomRes = new RoomResult
            {
                CreatorHeroes = creatorHeroes,
                OppHeroes = oppHeroes,
                Result = room.Result,
                RoomId = room.Id,
            };
            return roomRes;
        }
        private async Task<bool> SaveRoomRes(RoomResult roomResult)
        {
            try
            {
                using (var db = new RoomContext())
                {
                    db.RoomResult.Add(roomResult);
                    await db.SaveChangesAsync();
                }
                return true;
            }
            catch(Exception e)
            {
                error = e.Message;
                return false;
            }

        }
        //public async Task<ActionResult> NextStep(RoomModel model)
        //{
        //    TestClass.Steps++;

        //    if ((model.IsCreator && TestClass.Steps % 2 == 0) || (!model.IsCreator && TestClass.Steps != 0))
        //    {
        //        return PartialView("ChooseHeroView", model);

        //    }


        //    return Content("Сейчас ходит соперник...");

        //}
    }
}
    //класс содержит переменные и методы

//    public static class TestClass
//    {

//        //public static bool CheckMyStep(int roomId, bool isCreator)
//        //{

//        //}

//        public static int Steps { get; set; }

//        public static Dictionary<int, byte> Rooms { get; } = new Dictionary<int, byte>();

//        private static readonly Random rand = new Random();


//        public static Random rand2 = new Random();



//        public static bool ConnectToRoom(int roomId)
//        {
//            if (Rooms.ContainsKey(roomId))
//            {
//                IncreaceRoomConnectionsCount(roomId);
//                return true;
//            }
//            return false;
//        }

//        public static bool HasOneUser(int roomId) => Rooms.TryGetValue(roomId, out byte res) && res == 1;

//        public static bool IsFullRoom(int roomId) => Rooms.TryGetValue(roomId, out byte res) && res == 2;

//        public static bool IncreaceRoomConnectionsCount(int roomId)
//        {
//            if (Rooms.ContainsKey(roomId))
//            {
//                Rooms[roomId]++;
//                return true;
//            }
//            return false;
//        }
//        public static bool DecreaceRoomConnectionsCount(int roomId)
//        {
//            if (Rooms.ContainsKey(roomId))
//            {
//                Rooms[roomId]--;
//                return true;
//            }
//            return false;
//        }

//        public static int CreateRoom()
//        {
//            int roomId;
//            do
//            {
//                roomId = rand.Next();
//            }
//            while (Rooms.ContainsKey(roomId));

//            Rooms.Add(roomId, 0);
//            return roomId;
//        }

//        public static bool IsJoin { get; set; }
//    }

//}