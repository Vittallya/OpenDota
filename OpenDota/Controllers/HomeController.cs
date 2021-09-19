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

namespace OpenDota.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GenerateLink()
        {
            Console.WriteLine(Session.SessionID);


            int roomId = TestClass.CreateRoom();


            string link = "https://" + HttpContext.Request.Url.Authority + Url.Action(nameof(JoinRoom), new { RoomId = roomId });// "/Room/Index?roomId=" + roomId;

            return PartialView("LinkView",new RoomModel(link, roomId));
        }

        public ActionResult LinkView()
        {
            return RedirectToAction("Index");
        }

        public ActionResult JoinRoomCreator(RoomModel model)
        {
            model.IsCreator = true;
            //поиск комнаты, если нашел, проверить, находится ли она в режиме ожидания, если да - присоединение
            return RedirectToAction("JoinRoom", model);
        }

        public ActionResult JoinRoom(RoomModel model)
        {
            if (!TestClass.ConnectToRoom(model.RoomId))
            {
                return HttpNotFound();
            }
            //поиск комнаты, если нашел, проверить, находится ли она в режиме ожидания, если да - присоединение
            return View("RoomView", model);
        }

        public async Task<ActionResult> StartMatch(RoomModel model)
        {
            while (TestClass.HasOneUser(model.RoomId))
            {
                await Task.Delay(200);
            }

            if (model.IsCreator)
            {
                return PartialView("ChooseHeroView", model);
            }

            return PartialView("ConnectionAwaitView", model);
        }

        //"gsdg", 34, 
        bool flag;
        public ActionResult MakeStep(RoomModel model)
        {
            TestClass.Steps++;            
            return PartialView("ConnectionAwaitView", model);

        }

        public ActionResult GetStep(RoomModel model)
        {
            return PartialView("ChooseHeroView", model);

        }


        public async Task<ActionResult> AwaitOpponent(RoomModel model)
        {
            while ((model.IsCreator && TestClass.Steps % 2 != 0) || (!model.IsCreator && TestClass.Steps % 2 == 0))
            {
                await Task.Delay(200);

            }
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

    //класс содержит переменные и методы

    public static class TestClass
    {

        //public static bool CheckMyStep(int roomId, bool isCreator)
        //{

        //}

        public static int Steps { get; set; }

        public static Dictionary<int, byte> Rooms { get; } = new Dictionary<int, byte>();

        private static readonly Random rand = new Random();


        public static Random rand2 = new Random();



        public static bool ConnectToRoom(int roomId)
        {
            if (Rooms.ContainsKey(roomId))
            {
                IncreaceRoomConnectionsCount(roomId);
                return true;
            }
            return false;
        }

        public static bool HasOneUser(int roomId) => Rooms.TryGetValue(roomId, out byte res) && res == 1;

        public static bool IsFullRoom(int roomId) => Rooms.TryGetValue(roomId, out byte res) && res == 2;

        public static bool IncreaceRoomConnectionsCount(int roomId)
        {
            if (Rooms.ContainsKey(roomId))
            {
                Rooms[roomId]++;
                return true;
            }
            return false;
        }
        public static bool DecreaceRoomConnectionsCount(int roomId)
        {
            if (Rooms.ContainsKey(roomId))
            {
                Rooms[roomId]--;
                return true;
            }
            return false;
        }

        public static int CreateRoom()
        {
            int roomId;
            do
            {
                roomId = rand.Next();
            }
            while (Rooms.ContainsKey(roomId));

            Rooms.Add(roomId, 0);
            return roomId;
        }

        public static bool IsJoin { get; set; }
    }            
    //HttpClient client = new HttpClient();
                 
    //HttpResponseMessage message = await client.GetAsync("https://api.opendota.com/api/playersByRank");

    //Redirect("https://api.opendota.com/api/playersByRank").ExecuteResult(ControllerContext);
}