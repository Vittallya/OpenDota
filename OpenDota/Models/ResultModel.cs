using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class ResultModel
    {
        public int Result { get; set; }

        public int RoomId { get; set; }

        public string[] CreatorsHeroes { get; set; }
        public string[] OpponentHeroes { get; set; }
        public string[] BlockedByCreator { get; set; }
        public string[] BlockedByOpponent { get; set; }

        public string Comment { get; set; }
        public string Link { get; internal set; }
    }
}