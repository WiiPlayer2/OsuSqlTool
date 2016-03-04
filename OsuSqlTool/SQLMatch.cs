using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OsuSqlTool
{
    public class SQLMatch
    {
        public SQLLadder Ladder { get; set; }
        [JsonProperty("user2")]
        public SQLPlayer LeftPlayer { get; set; }
        [JsonProperty("user1")]
        public SQLPlayer RightPlayer { get; set; }

        public SQLPlayer UserPlayer { get; set; }

        public SQLPlayer VersusPlayer { get; set; }

        [JsonProperty("mpid")]
        public string MultiplayerID { get; set; }
        [JsonProperty("mpname")]
        public string MultiplayerName { get; set; }

        [JsonProperty("bans")]
        public int[] Bans { get; set; }

        [JsonProperty("picks")]
        public int[] Picks { get; set; }

        public bool SetPlayers(string username)
        {
            if (LeftPlayer.Username.ToLower() == username)
            {
                UserPlayer = LeftPlayer;
                VersusPlayer = RightPlayer;
            }
            else if (RightPlayer.Username.ToLower() == username)
            {
                UserPlayer = RightPlayer;
                VersusPlayer = LeftPlayer;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}