using IrcDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    public class SQLConnector : INotifyPropertyChanged
    {
        private OsuIrcClient client;
        private IrcUser osuSqlUser;

        public SQLConnector()
        {
            client = new OsuIrcClient();
            client.Connected += Client_Connected;
            client.ConnectFailed += Client_ConnectFailed;
            client.Disconnected += Client_Disconnected;
            client.MotdReceived += Client_MotdReceived;
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Disconnected(this, e);
        }

        private void Client_MotdReceived(object sender, EventArgs e)
        {
            client.LocalUser.SendMessage("osu_SQL", "!rank");
        }

        private void Client_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            Disconnected(this, e);
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            client.LocalUser.MessageReceived += LocalUser_MessageReceived;
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (e.Source.Name == "osu_SQL")
            {
                if (osuSqlUser == null)
                {
                    osuSqlUser = e.Source as IrcUser;
                    CallPropertyChanged("IsReady");
                }
            }
        }

        public void Connect()
        {
            if (string.IsNullOrWhiteSpace(Settings.Username)
                || string.IsNullOrWhiteSpace(Settings.Password))
            {
                Disconnected(this, EventArgs.Empty);
            }
            else
            {
                client.Connect("irc.ppy.sh", 6667, false, new IrcUserRegistrationInfo()
                {
                    NickName = Settings.Username,
                    Password = Settings.Password,
                    UserName = Settings.Username,
                    RealName = Settings.Username,
                });
            }
        }

        public event EventHandler Disconnected = (s, e) => { };

        protected void CallPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public bool IsReady
        {
            get
            {
                return osuSqlUser != null;
            }
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public void Ban(SQLMap map)
        {
            Chat("!ban {0}", map.MapIndex);
        }

        public void Pick(SQLMap map)
        {
            Chat("!pick {0}", map.MapIndex);
        }

        private void Chat(string format, params object[] args)
        {
            client.LocalUser.SendMessage(osuSqlUser, string.Format(format, args));
        }
    }
}
