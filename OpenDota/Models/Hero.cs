using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class Hero
    {
        public int id { get; set; }
        public string name { get; set; }
        public string localized_name { get; set; }
        public string primary_attr { get; set; }
        public string attack_type { get; set; }
        public string[] roles { get; set; }
        public int legs { get; set; }
    }

}