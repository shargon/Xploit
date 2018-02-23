using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using XPloit.Core.Sockets.Enums;

namespace XPloit.Core.Sockets.Interfaces
{
    public class IXploitMsgHeader
    {
        /// <summary>
        /// Especifica el tipo de la clase
        /// </summary>
        [DataMember(Name = "t")]
        [JsonProperty(PropertyName = "t")]
        public virtual EXPloitSocketMsgHeader Type { get { throw (new Exception("ERROR")); } }
    }
}