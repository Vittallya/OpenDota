using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class ActualHeroesModel
    {
        public Hero[] User1Heroes { get; set; }
        public Hero[] User1Blocked { get; set; }

        public Hero[] User2Heroes { get; set; }
        public Hero[] User2Blocked { get; set; }
    }
}