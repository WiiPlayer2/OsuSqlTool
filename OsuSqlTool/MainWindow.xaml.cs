﻿using GitHubUpdate;
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
            SQL.MatchFound += SQL_MatchFound;
            SQL.PropertyChanged += SQL_PropertyChanged;

            Settings.Instance.PropertyChanged += Instance_PropertyChanged;

            InitializeComponent();
        }

        private void SQL_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentState")
            {
                Dispatcher.Invoke(() =>
                {
                    queueButton.IsEnabled = true;
                    queueButton.Opacity = 0.7;
                    switch (SQL.CurrentState)
                    {
                        case SQLState.Unqueueable:
                            queueButton.Content = "Unqueueable";
                            queueButton.Background = FindResource("unqueueableColor") as Brush;
                            break;
                        case SQLState.Unqueued:
                            queueButton.Content = "Queue";
                            queueButton.Background = FindResource("queueColor") as Brush;
                            break;
                        case SQLState.Queued:
                            queueButton.Content = "Unqueue";
                            queueButton.Background = FindResource("unqueueColor") as Brush;
                            break;
                        case SQLState.MatchFound:
                            queueButton.Content = "Ready Up";
                            queueButton.Background = FindResource("readyColor") as Brush;
                            break;
                    }
                });
            }
        }

        private void SQL_MatchFound(object sender, EventArgs e)
        {
            if (Settings.Instance.UseNotificationSound)
            {
                Dispatcher.Invoke(() =>
                {
                    notifyMedia.Stop();
                    notifyMedia.Play();
                });
            }
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Ladder")
            {
                CallPropertyChanged("CurrentLadderMaps");
            }
        }

        public SQLConnector SQL { get; private set; }

        public IEnumerable<SQLCategoryMaps> CurrentLadderMaps
        {
            get
            {
                return SQL.Maps.GetLadderMaps(Settings.Instance.Ladder)
                    .GroupBy(o => o.Category)
                    .Select(o => new SQLCategoryMaps(o, o.Key));
            }
        }

        private void CallPropertyChanged(string name)
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
                    string.Format("There is a newer version available at {0} \n(Current version: {1}; Newer version: {2})\n\n"
                        + "Do you want to download it now?",
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
                loginDialog.Username = Settings.Instance.Username;
                loginDialog.Password = Settings.Instance.Password;
                var res = loginDialog.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    Settings.Instance.Username = loginDialog.Username;
                    Settings.Instance.Password = loginDialog.Password;
                    Settings.Instance.Save();
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
            CallPropertyChanged("CurrentLadderMaps");
        }

        private void ForgetLogin_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Username = "";
            Settings.Instance.Password = "";
            Settings.Instance.Save();

            SQL.Disconnect();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var option = new OptionsDialog();
            option.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutDialog();
            about.ShowDialog();
        }

        private void Queue_Click(object sender, RoutedEventArgs e)
        {
            queueButton.IsEnabled = false;
            queueButton.Opacity = 0;
            switch (SQL.CurrentState)
            {
                case SQLState.Unqueued:
                    SQL.Queue(Settings.Instance.Ladder);
                    break;
                case SQLState.Queued:
                    SQL.Unqueue();
                    break;
                case SQLState.MatchFound:
                    SQL.Ready();
                    break;
                default:
                    queueButton.IsEnabled = true;
                    queueButton.Opacity = 0.7;
                    break;
            }
        }

        private void SendFeedback_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://new.ppy.sh/forum/t/426617");
        }
    }
}
