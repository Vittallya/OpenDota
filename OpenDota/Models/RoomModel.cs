using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class RoomModel
    {
        public RoomModel(string link, int roomId)
        {
            Link = link;
            RoomId = roomId;
        }
        public RoomModel()
        {

        }
        public string Link { get; set; }
        public int RoomId { get; set; }
        public bool IsCreator { get; set; }

        public int HeroSelected { get; set; }

        public bool IsReturn { get; set; }

        public bool IsBlocking { get; set; }
        public Hero[] Heroes { get; set; }
    }
}