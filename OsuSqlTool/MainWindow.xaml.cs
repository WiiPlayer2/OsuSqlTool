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
            InitializeComponent();

            web = new WebClient();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            ReloadMaps();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ladderCombo.SelectedValue = Settings.Default.Ladder;
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
            InitIRC();
        }

        private void InitIRC()
        {
            osuSqlUser = null;
            Dispatcher.Invoke(() =>
            {
                IsEnabled = false;
            });

            client = new OsuIrcClient();
            client.Connected += Client_Connected;
            client.ConnectFailed += Client_ConnectFailed;
            client.Disconnected += Client_Disconnected;
            var reg = new IrcUserRegistrationInfo()
            {
                NickName = Settings.Default.Username,
                UserName = Settings.Default.Username,
                Password = Settings.Default.Password,
                RealName = Settings.Default.Username,
            };
            try
            {
                if (string.IsNullOrWhiteSpace(reg.NickName)
                    || string.IsNullOrWhiteSpace(reg.Password))
                {
                    throw new ArgumentException();
                }
                client.Connect("irc.ppy.sh", 6667, false, reg);
            }
            catch (Exception e)
            {
                Client_ConnectFailed(client, new IrcErrorEventArgs(e));
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Client_ConnectFailed(sender, new IrcErrorEventArgs(new Exception()));
        }

        private void Client_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var loginDialog = new LoginDialog();
                loginDialog.Username = Settings.Default.Username;
                loginDialog.Password = Settings.Default.Password;
                var res = loginDialog.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    Settings.Default.Username = loginDialog.Username;
                    Settings.Default.Password = loginDialog.Password;
                    Settings.Default.Save();
                    InitIRC();
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

        private void Client_Connected(object sender, EventArgs e)
        {
            client.LocalUser.MessageReceived += LocalUser_MessageReceived;
            client.MotdReceived += Client_MotdReceived;
        }

        private void Client_MotdReceived(object sender, EventArgs e)
        {
            client.LocalUser.SendMessage("osu_SQL", "!rank");
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
            MapCommand(sender, "ban");
        }

        private void MapControl_Picked(object sender, RoutedEventArgs e)
        {
            MapCommand(sender, "pick");
        }

        private void MapCommand(object sender, string command)
        {
            if (osuSqlUser != null)
            {
                try
                {
                    var mapControl = sender as MapControl;
                    var map = mapControl.DataContext as SQLMap;
                    client.LocalUser.SendMessage(osuSqlUser, string.Format("!{0} {1}", command, map.MapIndex));
                }
                catch (Exception e)
                {

                }
            }
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
                Settings.Default.Ladder = (SQLLadder)ladderCombo.SelectedValue;
                Settings.Default.Save();
            }
            ShowMaps();
        }

        private void ForgetLogin_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Username = "";
            Settings.Default.Password = "";
            Settings.Default.Save();
            client.Disconnect();
        }
    }
}
