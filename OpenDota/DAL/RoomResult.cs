using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OpenDota.DAL
{
    public class RoomResult
    {
        [Key]
        public int RoomId { get; set; }
        public string Comment { get; set; }
        public int Result { get; set; }
        public virtual ICollection<HeroResult> CreatorHeroes { get; set; }
        public virtual ICollection<HeroResult> OppHeroes { get; set; }
    }
}