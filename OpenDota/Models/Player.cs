using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class PlayerByRank
    {
        public int account_id { get; set; }
        public int rating { get; set; }
        public bool fh_unavailable { get; set; }
    }


    public class Player
    {
        public object tracked_until { get; set; }
        public int solo_competitive_rank { get; set; }
        public int rank_tier { get; set; }
        public Profile profile { get; set; }
        public int leaderboard_rank { get; set; }
        public Mmr_Estimate mmr_estimate { get; set; }
        public int competitive_rank { get; set; }
    }

    public class Profile
    {
        public int account_id { get; set; }
        public string personaname { get; set; }
        public object name { get; set; }
        public bool plus { get; set; }
        public int cheese { get; set; }
        public string steamid { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public string profileurl { get; set; }
        public DateTime last_login { get; set; }
        public object loccountrycode { get; set; }
        public bool is_contributor { get; set; }
    }

    public class Mmr_Estimate
    {
        public int estimate { get; set; }
    }


}