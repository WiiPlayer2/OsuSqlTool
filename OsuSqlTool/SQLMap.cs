using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    public class SQLMap
    {
        public SQLMap()
        {
            Artist = "- No Artist -";
            Creator = "- No Creator -";
            Difficulty = "- No Difficulty -";
            Title = "- No Title -";
            MapSetID = 92235;
        }

        [JsonProperty("drain")]
        private string drain
        {
            get
            {
                return DrainTime.TotalSeconds.ToString("0");
            }
            set
            {
                var val = 0;
                int.TryParse(value, out val);
                DrainTime = new TimeSpan(0, 0, val);
            }
        }

        [JsonProperty("star")]
        private string star
        {
            get
            {
                return Stars.ToString();
            }
            set
            {
                var val = 0.0;
                double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
                Stars = val;
            }
        }

        [JsonProperty("bpm")]
        private string bpm
        {
            get
            {
                return BPM.ToString();
            }
            set
            {
                var val = 0;
                int.TryParse(value, out val);
                BPM = val;
            }
        }

        [JsonProperty("type")]
        private string type
        {
            get
            {
                switch (Category)
                {
                    case SQLCategory.Doubletime:
                        return "doubletime";
                    case SQLCategory.FreeMod:
                        return "freemod";
                    case SQLCategory.Hardrock:
                        return "hardrock";
                    case SQLCategory.Hidden:
                        return "hidden";
                    case SQLCategory.NoMod:
                        return "nomod";
                    case SQLCategory.Tiebreaker:
                        return "tiebreaker";
                    default:
                        return Category.ToString();
                }
            }
            set
            {
                switch (value)
                {
                    case "doubletime":
                        Category = SQLCategory.Doubletime;
                        break;
                    case "freemod":
                        Category = SQLCategory.FreeMod;
                        break;
                    case "hardrock":
                        Category = SQLCategory.Hardrock;
                        break;
                    case "hidden":
                        Category = SQLCategory.Hidden;
                        break;
                    case "nomod":
                        Category = SQLCategory.NoMod;
                        break;
                    case "tiebreaker":
                        Category = SQLCategory.Tiebreaker;
                        break;
                    default:
                        Category = default(SQLCategory);
                        break;
                }
            }
        }

        public TimeSpan DrainTime { get; private set; }

        public double Stars { get; private set; }

        [JsonProperty("artist")]
        public string Artist { get; private set; }

        [JsonProperty("creator")]
        public string Creator { get; private set; }

        [JsonProperty("version")]
        public string Difficulty { get; private set; }

        [JsonProperty("title")]
        public string Title { get; private set; }

        public int BPM { get; private set; }

        [JsonProperty("mapid")]
        public int MapID { get; private set; }

        [JsonProperty("setid")]
        public int MapSetID { get; private set; }

        public int MapIndex { get; set; }

        public SQLCategory Category { get; private set; }

        public SQLLadder Ladder { get; set; }

        public string MapSetThumbnailURL
        {
            get
            {
                return string.Format("http://b.ppy.sh/thumb/{0}l.jpg", MapSetID);
            }
        }
    }
}
