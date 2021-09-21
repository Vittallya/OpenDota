using Newtonsoft.Json;
using OpenDota.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace OpenDota.Services
{
    public class Prognozer
    {

        static Match2[] matches2;
        public async Task<int> GetResult(int[] heroesFirst, int[] heroesSecond)
        {
            int totalWins1 = await GetTotalWins(heroesFirst);
            int totalWins2 = await GetTotalWins(heroesSecond);

            return totalWins1 - totalWins2;

            //5
            //-34
            //0 - 50/50
        }

        private async Task<double> GetTotalSkill(int[] heroes)
        {
            double totalSum = 0;
            for (int i = 0; i < heroes.Length; i++)
            {
                var heroId = heroes[i];
                var skill = await GetSkillFor(heroId);
                totalSum += skill;

            }
            return totalSum;
        }

        private double GetSumOfSkillForHero(Match[] matches)
        {
            return matches.Sum(x => x.kills / x.deaths + x.assists * 0.5);
        }

        

        private async Task<double> GetSkillFor(int heroId)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage message = await client.GetAsync($"https://api.opendota.com/api/heroes/{heroId}/matches");
            string content = await message.Content.ReadAsStringAsync();

            var matches = JsonConvert.DeserializeObject<Match[]>(content);
            var skill = GetSumOfSkillForHero(matches);

            return skill;
        }


        private async Task<int> GetTotalWins(int[] heroes)
        {

            if(matches2 == null)
            {
                await LoadMatches();
            }

            //List<Match2> matches = new List<Match2>();
            int count = 0;

            for (int i = 0; i < matches2.Length; i++)
            {
                var match = matches2[i];
                int[] heroesRadiant = match.radiant_team.Split(',').Select(x => int.Parse(x)).ToArray();
                int[] heroesDire = match.dire_team.Split(',').Select(x => int.Parse(x)).ToArray();


                if (match.radiant_win && 
                    heroesRadiant.Any(x => heroes.Contains(x)) ||
                    !match.radiant_win &&
                    heroesDire.Any(x => heroes.Contains(x)))

                {
                    //matches.Add(match);
                    count++;
                }
            }

            var skill = count;

            return skill;
        }

        public static bool IsLoaded => matches2 != null;

        public static async Task LoadMatches()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message = await client.GetAsync($"https://api.opendota.com/api/publicMatches");
            string content = await message.Content.ReadAsStringAsync();
            try
            {
                matches2 = JsonConvert.DeserializeObject<Match2[]>(content);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}