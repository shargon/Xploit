using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Messages
{
    public class XPloitMsgPing : IXPloitSocketMsg
    {
        [DataMember(Name = "d")]
        [JsonProperty(PropertyName = "d")]
        public DateTime Date { get; set; }
        [DataMember(Name = "i")]
        [JsonProperty(PropertyName = "i")]
        public string Info { get; set; }

        public XPloitMsgPing() { Date = DateTime.Now; }

        public override EXPloitSocketMsg Type { get { return EXPloitSocketMsg.Ping; } }
    }
}