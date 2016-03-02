using MadMilkman.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    static class Settings
    {
        private static IniFile ini;

        static Settings()
        {
            SettingsFile = Path.Combine(Path.GetDirectoryName(
                new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "config.ini");

            ini = new IniFile();
            if (!File.Exists(SettingsFile))
            {
                var keys = ini.Sections.Add("General").Keys;

                keys.Add("username", "");
                keys.Add("password", "");
                keys.Add("ladder", "Beginner");

                ini.Save(SettingsFile);
            }
            else
            {
                ini.Load(SettingsFile);
            }

            var lKeys = ini.Sections["General"].Keys;

            Username = lKeys["username"].Value;
            Password = lKeys["password"].Value;
            Ladder = ParseEnum<SQLLadder>(lKeys["ladder"].Value);
        }

        public static string SettingsFile { get; private set; }

        #region Values
        public static string Username { get; set; }

        public static string Password { get; set; }

        public static SQLLadder Ladder { get; set; }
        #endregion

        public static void Save()
        {
            var keys = ini.Sections["General"].Keys;

            keys["username"].Value = Username;
            keys["password"].Value = Password;
            keys["ladder"].Value = Ladder.ToString();

            ini.Save(SettingsFile);
        }

        private static TEnum ParseEnum<TEnum>(string value)
            where TEnum : struct
        {
            TEnum val = default(TEnum);
            Enum.TryParse<TEnum>(value, out val);
            return val;
        }
    }
}
