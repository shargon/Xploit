using XPloit.Core.Sockets.Enums;
using System;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Messages
{
    public class XPloitMsgPing : IXPloitSocketMsg
    {
        [DataMember(Name = "d")]
        public DateTime Date { get; set; }
        [DataMember(Name="i")]
        public string Info { get; set; }

        public XPloitMsgPing()
        {
            Date = DateTime.Now;
        }

        public override EXPloitSocketMsg Type { get { return EXPloitSocketMsg.Ping; } }
    }
}