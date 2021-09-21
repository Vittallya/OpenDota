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
            room.SelectHero(model.IsCreator, model.HeroSelected);

            if (!room.CanChoose())
            {
                int res = await GetResultForSession(room);
                return PartialView("ResultView", new ResultModel { Result = res });
            }

            model.IsBlocking = room.IsBanMode();

            return PartialView("ConnectionAwaitView", model);

        }

        public async Task<ActionResult> BanHero(RoomModel model)
        {
            var room = _roomService[model.RoomId];
            room.BanHero(model.IsCreator, model.HeroSelected);


            if (!room.CanChoose())
            {
                int res = await GetResultForSession(room);
                return PartialView("ResultView", new ResultModel() { Result = res });
            }
            return PartialView("ConnectionAwaitView", model);

        }

        private async Task<int> GetResultForSession(RoomSession room)
        {
            int res = 0;
            if(!room.IsResultSetted && !room.ResultCalled)
            {
                room.ResultCalled = true;
                res = await prognozer.GetResult(room[true].SelectedHeroes.ToArray(), room[false].SelectedHeroes.ToArray());
                room.SetResult(res);
            }
            else if (room.ResultCalled && !room.IsResultSetted)
            {
                while(!room.IsResultSetted)
                {
                    await Task.Delay(200);
                }
                res = room.Result;
            }
            else
            {
                res = room.Result;
            }
            return res;
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

        public ActionResult GetStep(RoomModel model)
        {
            return PartialView("ChooseHeroView", model);
        }

        private Prognozer prognozer = new Prognozer();

        public async Task<ActionResult> AwaitOpponent(RoomModel model)
        {
            var room = _roomService[model.RoomId];

            while (room.IsAwaitOpponent(model.IsCreator))
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
                int res = await GetResultForSession(room);

                return PartialView("ResultView", new ResultModel() { Result = res });
            }

            model.Heroes = room.GetHeroesFor(model.IsCreator);

            model.IsBlocking = room.IsBanMode();
            //проверить, можно ли пользователю выбрать еще кого-то
            //-да: переход в режим ожидания, пердставление ожидания
            //-нет: переход в финальную страницу


            return PartialView("ChooseHeroView", model);
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