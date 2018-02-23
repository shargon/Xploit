using Newtonsoft.Json;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Messages
{
    public class XPloitMsgString : IXPloitSocketMsg
    {
        [DataMember(Name = "d")]
        [JsonProperty(PropertyName = "d")]
        public string Data { get; set; }

        public override EXPloitSocketMsg Type { get { return EXPloitSocketMsg.String; } }
    }
}