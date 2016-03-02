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
    public partial class MainWindow : Window
    {
        public const string API_URL = "http://osusql.com/mappools.json";

        private WebClient web;
        private SQLMap[][] maps;
        private StandardIrcClient client;
        private IrcUser osuSqlUser;

        public MainWindow()
        {
            web = new WebClient();
            SQL = new SQLConnector();
            SQL.Disconnected += Sql_Disconnected;

            InitializeComponent();
        }

        public SQLConnector SQL { get; private set; }

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
            ReloadMaps();
            SQL.Connect();
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

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (e.Source.Name == "osu_SQL")
            {
                if (osuSqlUser == null)
                {
                    var type = e.Source.GetType();
                    osuSqlUser = e.Source as IrcUser;
                    Dispatcher.Invoke(() =>
                    {
                        IsEnabled = true;
                    });
                }
            }
        }

        private void ShowMaps()
        {
            if (maps != null)
            {
                var ladder = (SQLCategory)ladderCombo.SelectedValue;
                mapPanel.Children.Clear();
                foreach (var e in Enum.GetValues(typeof(SQLCategory))
                    .Cast<SQLCategory>())
                {
                    var text = new TextBlock();
                    text.Text = e.ToString();
                    text.FontSize = 20;

                    mapPanel.Children.Add(text);
                    var wrapPanel = new WrapPanel();
                    mapPanel.Children.Add(wrapPanel);

                    foreach (var m in maps[(int)ladder]
                        .Where(o => o.Category == e))
                    {
                        var mapControl = new MapControl(m);
                        mapControl.Picked += MapControl_Picked;
                        mapControl.Banned += MapControl_Banned;
                        wrapPanel.Children.Add(mapControl);
                    }
                }
            }
        }

        private void MapControl_Banned(object sender, RoutedEventArgs e)
        {
            SQL.Ban((sender as MapControl).Map);
        }

        private void MapControl_Picked(object sender, RoutedEventArgs e)
        {
            SQL.Pick((sender as MapControl).Map);
        }

        private void ReloadMaps()
        {
            try
            {
                var mapJson = web.DownloadString(API_URL);
                maps = JsonConvert.DeserializeObject<SQLMap[][]>(mapJson);

                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < maps[i].Length; j++)
                    {
                        maps[i][j].Ladder = (SQLLadder)i;
                        maps[i][j].MapIndex = j + 1;
                    }
                }

                ShowMaps();
            }
            catch (Exception e)
            {

            }
        }

        private void ladderCombo_Selected(object sender, RoutedEventArgs e)
        {
            if (ladderCombo.SelectedValue != null
                && osuSqlUser != null)
            {
                Settings.Ladder = (SQLLadder)ladderCombo.SelectedValue;
                Settings.Save();
            }
            ShowMaps();
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
