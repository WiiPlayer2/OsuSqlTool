using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    class OsuIrcClient : StandardIrcClient
    {
        protected override void WriteMessage(string line, object token)
        {
            var tmp = line;
            if (tmp.StartsWith("PASS :")
                || tmp.StartsWith("NICK :"))
            {
                var colonIndex = tmp.IndexOf(':');
                tmp = tmp.Remove(colonIndex, 1);
            }
            base.WriteMessage(tmp, token);
        }
    }
}
