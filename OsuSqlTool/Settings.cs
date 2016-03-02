using MadMilkman.Ini;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #region Public Constructors

        static Settings()
        {
            Instance = new Settings();
        }

        #endregion Public Constructors

        #region Public Properties

        public static Settings Instance { get; private set; }
        public string SettingsFile { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Reload()
        {
            ini = new IniFile();
            ini.Load(SettingsFile);
            Load();
        }

        public void Save()
        {
            var keys = ini.Sections["General"].Keys;

            keys["username"].Value = Username;
            keys["password"].Value = Password;
            keys["ladder"].Value = Ladder.ToString();

            ini.Save(SettingsFile);
        }

        #endregion Public Methods

        #region Private Fields

        private IniFile ini;

        #endregion Private Fields

        #region Private Constructors

        private Settings()
        {
            SettingsFile = Path.Combine(Path.GetDirectoryName(
                new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "config.ini");

            ini = new IniFile();

            if (File.Exists(SettingsFile))
            {
                ini.Load(SettingsFile);
            }

            SetDefault("General", "username", "");
            SetDefault("General", "password", "");
            SetDefault("General", "ladder", SQLLadder.Beginner);

            ini.Save(SettingsFile);

            Load();
        }

        #endregion Private Constructors

        #region Values
        // Using a DependencyProperty as the backing store for Ladder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LadderProperty =
            DependencyProperty.Register("Ladder", typeof(SQLLadder), typeof(Settings), new PropertyMetadata(SQLLadder.Beginner));

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(Settings), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for Username.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(Settings), new PropertyMetadata(""));

        public SQLLadder Ladder
        {
            get { return (SQLLadder)GetValue(LadderProperty); }
            set { SetValue(LadderProperty, value); }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }
        #endregion

        #region Private Methods

        private static TEnum ParseEnum<TEnum>(string value)
            where TEnum : struct
        {
            TEnum val = default(TEnum);
            Enum.TryParse<TEnum>(value, out val);
            return val;
        }

        private void Load()
        {
            var keys = ini.Sections["General"].Keys;

            Username = keys["username"].Value;
            Password = keys["password"].Value;
            Ladder = ParseEnum<SQLLadder>(keys["ladder"].Value);
        }

        #endregion Private Methods

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropertyChanged(this, new PropertyChangedEventArgs(e.Property.Name));
        }

        private void SetDefault(string section, string name)
        {
            SetDefault(section, name, "");
        }

        private void SetDefault(string section, string name, object value)
        {
            SetDefault(section, name, value == null ? "" : value.ToString());
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
    }
}
