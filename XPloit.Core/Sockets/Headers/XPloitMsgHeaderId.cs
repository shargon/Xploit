using System;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Headers
{
    public class XPloitMsgHeaderId : IXploitMsgHeader
    {
        public override EXPloitSocketMsgHeader Type { get { return EXPloitSocketMsgHeader.Id; } }

        /// <summary>
        /// Message Id
        /// </summary>
        [DataMember(Name = "i")]
        public Guid Id { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public XPloitMsgHeaderId()
        {
            Id = Guid.Empty;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        public XPloitMsgHeaderId(Guid id)
        {
            Id = id;
        }
        /// <summary>
        /// Create a new Id
        /// </summary>
        public static XPloitMsgHeaderId CreateNew()
        {
            return new XPloitMsgHeaderId(Guid.NewGuid());
        }

        public override string ToString()
        {
            return "Id=" + Id.ToString();
        }
    }
}