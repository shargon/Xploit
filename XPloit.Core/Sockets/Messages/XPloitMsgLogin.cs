using XPloit.Core.Sockets.Enums;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Interfaces;
using Newtonsoft.Json;

namespace XPloit.Core.Sockets.Messages
{
    public class XPloitMsgLogin : IXPloitSocketMsg
    {
        [DataMember(Name = "d")]
        [JsonProperty(PropertyName = "d")]
        public string Domain { get; set; }
        [DataMember(Name = "u")]
        [JsonProperty(PropertyName = "u")]
        public string User { get; set; }
        [DataMember(Name = "p")]
        [JsonProperty(PropertyName = "p")]
        public string Password { get; set; }

        public override EXPloitSocketMsg Type { get { return EXPloitSocketMsg.Login; } }
    }
}