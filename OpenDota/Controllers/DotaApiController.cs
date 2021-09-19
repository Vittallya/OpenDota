using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenDota.Controllers
{
    public class DotaApiController: Controller
    {
        [HttpGet]
        public string GetPlayersByRank()
        {
            return Redirect("https://api.opendota.com/api/playersByRank").Url;
        }

    }
}