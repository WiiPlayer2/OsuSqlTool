using GitHubUpdate;
using IrcDotNet;
using Newtonsoft.Json;
using OsuSqlTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuSqlTool
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public MainWindow()
        {
            SQL = new SQLConnector();
            SQL.Disconnected += Sql_Disconnected;

            InitializeComponent();
        }

        public SQLConnector SQL { get; private set; }

        public IEnumerable<SQLCategoryMaps> CurrentLadderMaps
        {
            get
            {
                return SQL.Maps.GetLadderMaps(Settings.Ladder)
                    .GroupBy(o => o.Category)
                    .Select(o => new SQLCategoryMaps(o, o.Key));
            }
        }

        private void CallPropertChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void Sql_Disconnected(object sender, EventArgs e)
        {
            ShowLoginDialog();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            ReloadMaps();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ladderCombo.SelectedValue = Settings.Ladder;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                CheckUpdate();
            }

            ReloadMaps();
            SQL.Connect();
        }

        private void CheckUpdate()
        {
            Update.Init("WiiPlayer2", "OsuSqlTool");
            if (Update.HasUpdate())
            {
                var res = MessageBox.Show(
                    string.Format("There is a newer version available at {0} \n(Current version: {1}; Newer version: {2})\n\nDo you want to download it now?",
                    Update.ReleaseUrl, Update.CurrentVersion, Update.NewestVersion), "Update available!",
                    MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes)
                {
                    Process.Start(Update.ReleaseUrl);
                }
            }
        }

        private void ShowLoginDialog()
        {
            Dispatcher.Invoke(() =>
            {
                var loginDialog = new LoginDialog();
                loginDialog.Username = Settings.Username;
                loginDialog.Password = Settings.Password;
                var res = loginDialog.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    Settings.Username = loginDialog.Username;
                    Settings.Password = loginDialog.Password;
                    Settings.Save();
                    SQL.Connect();
                }
                else
                {
                    Close();
                }
            });
        }

        private void ReloadMaps()
        {
            SQL.Maps.ReloadMaps();
            CallPropertChanged("CurrentLadderMaps");
        }

        private void ladderCombo_Selected(object sender, RoutedEventArgs e)
        {
            if (ladderCombo.SelectedValue != null
                && SQL.IsReady)
            {
                Settings.Ladder = (SQLLadder)ladderCombo.SelectedValue;
                Settings.Save();
                CallPropertChanged("CurrentLadderMaps");
            }
        }

        private void ForgetLogin_Click(object sender, RoutedEventArgs e)
        {
            Settings.Username = "";
            Settings.Password = "";
            Settings.Save();

            SQL.Disconnect();
        }
    }
}
