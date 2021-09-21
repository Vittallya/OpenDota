using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{

    public class Match2
    {
        public long match_id { get; set; }
        public long match_seq_num { get; set; }
        public bool radiant_win { get; set; }
        public int start_time { get; set; }
        public int duration { get; set; }
        public int? avg_mmr { get; set; }
        public int? num_mmr { get; set; }
        public int lobby_type { get; set; }
        public int game_mode { get; set; }
        public int avg_rank_tier { get; set; }
        public int num_rank_tier { get; set; }
        public int cluster { get; set; }
        public string radiant_team { get; set; }
        public string dire_team { get; set; }
    }


    public class Rootobject
    {
        public long match_id { get; set; }
        public long match_seq_num { get; set; }
        public bool radiant_win { get; set; }
        public int start_time { get; set; }
        public int duration { get; set; }
        public int avg_mmr { get; set; }
        public int num_mmr { get; set; }
        public int lobby_type { get; set; }
        public int game_mode { get; set; }
        public int avg_rank_tier { get; set; }
        public int num_rank_tier { get; set; }
        public int cluster { get; set; }
        public string radiant_team { get; set; }
        public string dire_team { get; set; }
    }

}