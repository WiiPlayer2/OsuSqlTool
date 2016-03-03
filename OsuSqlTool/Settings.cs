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
    public class Settings : DependencyObject, INotifyPropertyChanged
    {
        private IniFile ini;

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
        // Using a DependencyProperty as the backing store for Ladder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LadderProperty =
            DependencyProperty.Register("Ladder", typeof(SQLLadder), typeof(Settings), new PropertyMetadata(SQLLadder.Beginner));

        // Using a DependencyProperty as the backing store for NotificationSoundUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationSoundUriProperty =
            DependencyProperty.Register("NotificationSoundUri", typeof(Uri), typeof(Settings), new PropertyMetadata(new Uri("file://")));

        // Using a DependencyProperty as the backing store for NotificationVolume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationVolumeProperty =
            DependencyProperty.Register("NotificationVolume", typeof(double), typeof(Settings), new PropertyMetadata(1.0d));

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(Settings), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for UseNotificationSound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseNotificationSoundProperty =
            DependencyProperty.Register("UseNotificationSound", typeof(bool), typeof(Settings), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for Username.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(Settings), new PropertyMetadata(""));

        public SQLLadder Ladder
        {
            get { return (SQLLadder)GetValue(LadderProperty); }
            set { SetValue(LadderProperty, value); }
        }

        public Uri NotificationSoundUri
        {
            get { return (Uri)GetValue(NotificationSoundUriProperty); }
            set { SetValue(NotificationSoundUriProperty, value); }
        }

        public double NotificationVolume
        {
            get { return (double)GetValue(NotificationVolumeProperty); }
            set { SetValue(NotificationVolumeProperty, value); }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public bool UseNotificationSound
        {
            get { return (bool)GetValue(UseNotificationSoundProperty); }
            set { SetValue(UseNotificationSoundProperty, value); }
        }

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }
        #endregion Values

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropertyChanged(this, new PropertyChangedEventArgs(e.Property.Name));
        }

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
