using XPloit.Core.Sockets.Enums;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Messages
{
    public class XPloitMsgByteArray : IXPloitSocketMsg
    {
        [DataMember(Name = "d")]
        public byte[] Data { get; set; }

        public XPloitMsgByteArray()
        {
        }
        public override EXPloitSocketMsg Type { get { return EXPloitSocketMsg.ByteArray; } }
    }
}