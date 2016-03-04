using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OsuSqlTool
{
    public class SQLMatchDownloader
    {
        private const string API_URL = "http://osusql.com/api/api.php?matches";

        private Timer refreshTimer;
        private WebClient web;

        public SQLMatchDownloader()
        {
            refreshTimer = new Timer(10 * 1000);
            refreshTimer.Elapsed += RefreshTimer_Elapsed;

            web = new WebClient();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
#if DEBUG
            refreshTimer.Stop();
#else
            try
            {
#endif
            var matches = GetData();
            var username = "Neko-sama".ToLower();//Settings.Instance.Username.ToLower();
            var match = matches
                .SingleOrDefault(o => o.SetPlayers(username));
            if (match != null)
            {
                MatchFound(this, match);
            }
#if !DEBUG
            }
            catch { }
#else
            refreshTimer.Stop();
#endif
        }

        public event EventHandler<SQLMatch> MatchFound = (s, e) => { };

        public void Start()
        {
            refreshTimer.Start();
        }

        public void Stop()
        {
            refreshTimer.Stop();
        }

        private SQLMatch[] GetData()
        {
            try
            {
                var text = web.DownloadString(API_URL);
                var jobj = JObject.Parse(text);
                return ((bool)jobj["success"])
                    ? JsonConvert.DeserializeObject<SQLMatch[]>(jobj["data"].ToString())
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
