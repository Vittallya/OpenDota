using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OpenDota.DAL
{
    public class HeroResult
    {
        [Key]
        public int HeroId { get; set; }
        public string Name { get; set; }
        public bool ChoosenByCreator { get; set; }

        public int RoomResultId { get; set; }
    }
}