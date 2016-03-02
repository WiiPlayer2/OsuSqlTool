using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    public class SQLMapDownloader
    {
        public const string API_URL = "http://osusql.com/mappools.json";

        private WebClient web;
        private Dictionary<SQLLadder, SQLMap[]> maps;

        public SQLMapDownloader()
        {
            web = new WebClient();
            maps = new Dictionary<SQLLadder, SQLMap[]>();

            foreach (var ladder in Enum.GetValues(typeof(SQLLadder)))
            {
                maps[(SQLLadder)ladder] = new SQLMap[0];
            }
        }

        public void ReloadMaps()
        {
            try
            {
                var mapJson = web.DownloadString(API_URL);
                var maps = JsonConvert.DeserializeObject<SQLMap[][]>(mapJson);

                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < maps[i].Length; j++)
                    {
                        maps[i][j].Ladder = (SQLLadder)i;
                        maps[i][j].MapIndex = j + 1;
                    }
                }

                foreach (var ladder in Enum.GetValues(typeof(SQLLadder)))
                {
                    this.maps[(SQLLadder)ladder] = maps[(int)(SQLLadder)ladder];
                }

                AreMapsLoaded = true;
            }
            catch { }
        }

        public IEnumerable<SQLMap> GetLadderMaps(SQLLadder ladder)
        {
            return maps[ladder];
        }

        public bool AreMapsLoaded { get; private set; }
    }
}
