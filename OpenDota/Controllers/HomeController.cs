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

        private RoomContext roomContext;

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
            if (_roomService == null)
                _roomService = new RoomService();

            int roomId = _roomService.CreateRoom();
            string link = "https://" + HttpContext.Request.Url.Authority + Url.Action(nameof(JoinRoom), new { RoomId = roomId });// "/Room/Index?roomId=" + roomId;

            return PartialView("LinkView", new RoomModel(link, roomId));
        }

        public ActionResult LinkView()
        {
            return RedirectToAction("Index");
        }

        public ActionResult JoinRoomCreator(RoomModel model)
        {
            model.IsCreator = true;
            return RedirectToAction("JoinRoom", model);
        }

        public ActionResult JoinRoom(RoomModel model)
        {
            var res = _roomService.ConnectToRoom(model.RoomId, model.IsCreator);

            if (!res.IsOk)
            {
                Response.StatusCode = 404;
                return Content(res.Message);
            }
            //поиск комнаты, если нашел, проверить, находится ли она в режиме ожидания, если да - присоединение
            return View("RoomView", model);
        }

        public async Task<ActionResult> StartMatch(RoomModel model)
        {
            var room = _roomService[model.RoomId];



            while (room.RoomStatus != RoomStatus.Active)
            {
                await Task.Delay(200);                
            }

            model.Heroes = _roomService.AllHeroes;
            model.IsBlocking = true;

            if (model.IsCreator)
            {
                return PartialView("ChooseHeroView", model);
            }

            return PartialView("ConnectionAwaitView", model);
        }
        public async Task<ActionResult> SelectHero(RoomModel model)
        {
            var room = _roomService[model.RoomId];

            if(room == null)
            {
                Response.StatusCode = 404;
                return Content("Комната не найдена");
            }

            room.SelectHero(model.IsCreator, model.HeroSelected);

            if (!room.CanChoose())
            {
                var res = await SaveResults(room);
                //return PartialView("ResultView", new ResultModel { Result = res });
                return RedirectToAction("ShowResult", new { roomId = room.Id, isShowLink = true });
            }

            model.IsBlocking = room.IsBanMode();

            return PartialView("ConnectionAwaitView", model);

        }

        public ActionResult BanHero(RoomModel model)
        {
            var room = _roomService[model.RoomId];
            room.BanHero(model.IsCreator, model.HeroSelected);


            //if (!room.CanChoose())
            //{
            //    await GetResultForSession(room);
            //    //return PartialView("ResultView", new ResultModel { Result = res });
            //    return RedirectToAction("ShowResult", room.Id);
            //}
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

            if (room.IsResultSetted)
            {
                return RedirectToAction("ShowResult", new { roomId = room.Id, isShowLink = true });
            }

            //if (!room.CanChoose())
            //{
            //    int res = await SaveResults(room);

            //    return PartialView("ResultView", new ResultModel() { Result = res });
            //}

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

            if(room.RoomStatus == RoomStatus.AwaitOnCreate)
            {
                return null;
            }

            return PartialView("ActualHeroes", new ActualHeroesModel
            {
                User1Heroes = room.GetAllHeroesFor(true),
                User2Heroes = room.GetAllHeroesFor(false),
                User1Blocked = room.GetAllHeroesBlocked(true),
                User2Blocked = room.GetAllHeroesBlocked(false)
            });

        }

        //public ActionResult GetStep(RoomModel model)
        //{
        //    return PartialView("ChooseHeroView", model);
        //}

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

            return Content("<h2>"+ model.Comment+"</h2>");
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