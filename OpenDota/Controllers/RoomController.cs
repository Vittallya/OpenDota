using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OpenDota.Models;


namespace OpenDota.Controllers
{
    public class RoomController : Controller
    {

        private static RoomService _roomService;

        [HttpGet]
        public async Task<ActionResult> Index(int? roomId)
        {
            if (!roomId.HasValue)
                return RedirectToAction("Index", "Home");

            
            //Get и Post
            int roomIdVal = roomId.Value;

            if (!TestClass.IsFullRoom(roomIdVal))
            {
                TestClass.IncreaceRoomConnectionsCount(roomIdVal);                

                while (TestClass.HasOneUser(roomIdVal))
                {
                    await Task.Delay(200);
                }

                return View();
            }
            else
            {
                return HttpNotFound();
            }
        }

        //[HttpGet]
        //public async Task<ActionResult> Index(int? roomId, bool? isCreator)
        //{
        //    if (_roomService == null)
        //        _roomService = new RoomService();

        //    if(isCreator.HasValue && isCreator.Value)
        //    {
        //        _roomService.CreateRoom(roomId.Value);
        //    }
        //    else
        //    {

        //    }
        //}

        //[HttpGet]
        //public async Task<ActionResult> Next(int roomId, bool? isCreator, bool? isOpponent)
        //{
        //    if (_roomService == null)
        //        _roomService = new RoomService();

        //    if(isCreator.HasValue && isCreator.Value)
        //    {
        //        _roomService.CreateRoom(roomId.Value);
        //    }
        //    else
        //    {

        //    }
        //}
        
    }
}