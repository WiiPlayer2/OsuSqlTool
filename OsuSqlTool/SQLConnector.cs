using IrcDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    public class SQLConnector : INotifyPropertyChanged
    {
        private OsuIrcClient client;
        private SQLState currentState = SQLState.Unqueued;
        private IrcUser osuSqlUser;
        private Dictionary<Regex, Action<Match>> regexActions;

        public SQLConnector()
        {
            client = new OsuIrcClient();
            client.Connected += Client_Connected;
            client.ConnectFailed += Client_ConnectFailed;
            client.Disconnected += Client_Disconnected;
            client.MotdReceived += Client_MotdReceived;

            Maps = new SQLMapDownloader();
            Matches = new SQLMatchDownloader();
            Matches.MatchFound += Matches_MatchFound;

            regexActions = new Dictionary<Regex, Action<Match>>();
            RegisterAction("^A match was found!.*$", ChatMatchFound);
            RegisterAction("^You're queued up!.*$", ChatQueuedUp);
            RegisterAction("^You failed to ready up in time!$", ChatReadyFailed);
            RegisterAction("^You're no longer searching for a match!$", ChatUnqueued);
        }

        private void Matches_MatchFound(object sender, SQLMatch e)
        {
            if (Maps.AreMapsLoaded)
            {

            }
        }

        public event EventHandler Disconnected = (s, e) => { };

        public event EventHandler MatchFound = (s, e) => { };

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public event EventHandler Queued = (s, e) => { };

        public event EventHandler Unqueued = (s, e) => { };
        public SQLState CurrentState
        {
            get
            {
                return currentState;
            }
            private set
            {
                if (currentState != value)
                {
                    currentState = value;
                    CallPropertyChanged("CurrentState");
                }
            }
        }

        public bool IsReady
        {
            get
            {
                return osuSqlUser != null;
            }
        }

        public SQLMapDownloader Maps { get; private set; }

        public SQLMatchDownloader Matches { get; private set; }

        public void Ban(SQLMap map)
        {
            Chat("!ban {0}", map.MapIndex);
        }

        public void Connect()
        {
            if (string.IsNullOrWhiteSpace(Settings.Instance.Username)
                || string.IsNullOrWhiteSpace(Settings.Instance.Password))
            {
                Disconnected(this, EventArgs.Empty);
            }
            else
            {
                client.Connect("irc.ppy.sh", 6667, false, new IrcUserRegistrationInfo()
                {
                    NickName = Settings.Instance.Username,
                    Password = Settings.Instance.Password,
                    UserName = Settings.Instance.Username,
                    RealName = Settings.Instance.Username,
                });
            }
        }

        public void Disconnect()
        {
            Matches.Stop();
            client.Disconnect();
        }

        public void Pick(SQLMap map)
        {
            Chat("!pick {0}", map.MapIndex);
        }

        public void Queue(SQLLadder ladder)
        {
            Chat("!queue {0}", ladder);
        }

        public void Ready()
        {
            Chat("!ready");
        }

        public void Unqueue()
        {
            Chat("!unqueue");
        }

        protected void CallPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void Chat(string format, params object[] args)
        {
            client.LocalUser.SendMessage(osuSqlUser, string.Format(format, args));
        }

        #region ChatActions
        private void ChatMatchFound(Match obj)
        {
            CurrentState = SQLState.MatchFound;
            MatchFound(this, EventArgs.Empty);
        }

        private void ChatQueuedUp(Match obj)
        {
            CurrentState = SQLState.Queued;
            Queued(this, EventArgs.Empty);
        }

        private void ChatReadyFailed(Match obj)
        {
            CurrentState = SQLState.Unqueued;
            Unqueued(this, EventArgs.Empty);
        }

        private void ChatUnqueued(Match obj)
        {
            CurrentState = SQLState.Unqueued;
            Unqueued(this, EventArgs.Empty);
        }
        #endregion

        private void Client_Connected(object sender, EventArgs e)
        {
            client.LocalUser.MessageReceived += LocalUser_MessageReceived;
        }

        private void Client_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            Disconnected(this, e);
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Disconnected(this, e);
        }

        private void Client_MotdReceived(object sender, EventArgs e)
        {
            client.LocalUser.SendMessage("osu_SQL", "!help");
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (e.Source.Name == "osu_SQL")
            {
                if (osuSqlUser == null)
                {
                    osuSqlUser = e.Source as IrcUser;
                    Matches.Start();
                    CallPropertyChanged("IsReady");
                }

                var match = regexActions
                    .Select(o => new Tuple<Match, Action<Match>>(o.Key.Match(e.Text), o.Value))
                    .SingleOrDefault(o => o.Item1.Success);
                if (match != null)
                {
                    match.Item2(match.Item1);
                }
            }
        }

        private void RegisterAction(string regex, Action<Match> action)
        {
            regexActions[new Regex(regex)] = action;
        }
    }
}
