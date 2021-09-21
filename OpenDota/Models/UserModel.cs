using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenDota.Models;

namespace OpenDota.Models
{
    public class UserModel
    {        
        public bool IsCreator { get;}

        public UserModel(bool isCreator)
        {
            IsCreator = isCreator;
        }

        public ICollection<int> BannedHeroes { get; set; } = new List<int>();
        public ICollection<int> SelectedHeroes { get; set; } = new List<int>();


    }
}