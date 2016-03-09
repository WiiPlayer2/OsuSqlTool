using MadMilkman.Ini;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OsuSqlTool
{
    public class Settings : INotifyPropertyChanged
    {
        private IniFile ini;

        private Dictionary<string, object> settingValues = new Dictionary<string, object>();

        static Settings()
        {
            try
            {
                Instance = new Settings();
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message, e.ToString());
            }
        }

        private Settings()
        {
            SettingsFile = Path.Combine(Path.GetDirectoryName(
                new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "config.ini");

            ini = new IniFile();

            if (File.Exists(SettingsFile))
            {
                ini.Load(SettingsFile);
            }

            SetDefault("General", "Username", "");
            SetDefault("General", "Password", "");
            SetDefault("General", "Ladder", SQLLadder.Beginner);
            SetDefault("General", "UseNotificationSound", true);
            SetDefault("General", "NotificationSoundUri", "file://");
            SetDefault("General", "NotificationVolume", 1.0d);

            ini.Save(SettingsFile);

            Load();
        }

        private void CallPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public static Settings Instance { get; private set; }

        public string SettingsFile { get; private set; }

        public void Reload()
        {
            ini = new IniFile();
            ini.Load(SettingsFile);
            Load();
        }

        public void Save()
        {
            var keys = ini.Sections["General"].Keys;

            keys["Username"].Value = Username;
            keys["Password"].Value = Password;
            keys["Ladder"].Value = Ladder.ToString();
            keys["UseNotificationSound"].Value = UseNotificationSound.ToString();
            keys["NotificationSoundUri"].Value = NotificationSoundUri.ToString();
            keys["NotificationVolume"].Value = NotificationVolume.ToString(CultureInfo.InvariantCulture);

            ini.Save(SettingsFile);
        }

        #region Values
        public SQLLadder Ladder
        {
            get { return GetValue<SQLLadder>("Ladder"); }
            set { SetValue("Ladder", value); }
        }

        public Uri NotificationSoundUri
        {
            get { return GetValue<Uri>("NotificationSoundUri"); }
            set { SetValue("NotificationSoundUri", value); }
        }

        public double NotificationVolume
        {
            get { return GetValue<double>("NotificationVolume"); }
            set { SetValue("NotificationVolume", value); }
        }

        public string Password
        {
            get { return GetValue<string>("Password"); }
            set { SetValue("Password", value); }
        }

        public bool UseNotificationSound
        {
            get { return GetValue<bool>("UseNotificationSound"); }
            set { SetValue("UseNotificationSound", value); }
        }

        public string Username
        {
            get { return GetValue<string>("Username"); }
            set { SetValue("Username", value); }
        }
        #endregion Values

        #region Parser
        private static double ParseDouble(string value)
        {
            return ParseDouble(value, 0.0d);
        }

        private static double ParseDouble(string value, double defaultValue)
        {
            var val = defaultValue;
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
            {
                val = defaultValue;
            }
            return val;
        }

        private static TEnum ParseEnum<TEnum>(string value)
                                    where TEnum : struct
        {
            return ParseEnum<TEnum>(value, default(TEnum));
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum defaultValue)
            where TEnum : struct
        {
            TEnum val = defaultValue;
            if (!Enum.TryParse<TEnum>(value, out val))
            {
                val = defaultValue;
            }
            return val;
        }
        #endregion

        #region Setter + Getter
        private T GetValue<T>(string name)
        {
            if (!settingValues.ContainsKey(name))
            {
                return default(T);
            }
            return (T)settingValues[name];
        }

        private void SetValue(string name, object value)
        {
            if (!settingValues.ContainsKey(name)
                || !settingValues[name].Equals(value))
            {
                settingValues[name] = value;
                CallPropertyChanged(name);
            }
        }
        #endregion

        private void Load()
        {
            var keys = ini.Sections["General"].Keys;

            Username = keys["Username"].Value;
            Password = keys["Password"].Value;
            Ladder = ParseEnum<SQLLadder>(keys["Ladder"].Value);
            UseNotificationSound = keys["UseNotificationSound"].Value.ToLower() == "true";
            NotificationSoundUri = new Uri(keys["NotificationSoundUri"].Value);
            NotificationVolume = ParseDouble(keys["NotificationVolume"].Value, 1);
        }

        private void SetDefault(string section, string name)
        {
            SetDefault(section, name, "");
        }

        private void SetDefault(string section, string name, string value)
        {
            var sect = ini.Sections[section];
            if (sect == null)
            {
                sect = ini.Sections.Add(section);
            }
            var key = sect.Keys[name];
            if (key == null)
            {
                sect.Keys.Add(name, value);
            }
        }

        private void SetDefault(string section, string name, IFormattable value)
        {
            SetDefault(section, name,
                value == null ? "" : value.ToString(null, CultureInfo.InvariantCulture));
        }

        private void SetDefault(string section, string name, object value)
        {
            SetDefault(section, name, value == null ? "" : value.ToString());
        }
    }
}
