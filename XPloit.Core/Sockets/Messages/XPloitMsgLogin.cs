using XPloit.Core.Sockets.Enums;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Messages
{
    public class XPloitMsgLogin : IXPloitSocketMsg
    {
        [DataMember(Name = "u")]
        public string User { get; set; }
        [DataMember(Name = "p")]
        public string Password { get; set; }

        public XPloitMsgLogin()
        {
        }
        public override EXPloitSocketMsg Type { get { return EXPloitSocketMsg.Login; } }
    }
}