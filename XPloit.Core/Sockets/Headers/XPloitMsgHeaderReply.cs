using System;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Headers
{
    public class XPloitMsgHeaderReply : IXploitMsgHeader
    {
        public override EXPloitSocketMsgHeader Type { get { return EXPloitSocketMsgHeader.Reply; } }

        /// <summary>
        /// Response Id
        /// </summary>
        [DataMember(Name = "r")]
        public Guid InResponseTo { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        internal XPloitMsgHeaderReply()
        {
            InResponseTo = Guid.Empty;
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="inResponseTo">In response to</param>
        internal XPloitMsgHeaderReply(Guid inResponseTo)
        {
            InResponseTo = inResponseTo;
        }
        /// <summary>
        /// Create a Reply
        /// </summary>
        internal static XPloitMsgHeaderReply CreateNew(Guid inResponseTo)
        {
            return new XPloitMsgHeaderReply(inResponseTo);
        }

        public override string ToString()
        {
            return "Reply=" + InResponseTo.ToString();
        }
    }
}