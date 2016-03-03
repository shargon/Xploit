using System;
using System.Collections.Generic;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets.Headers
{
    public class XPloitMsgHeadersCollection : List<IXploitMsgHeader>
    {
        public const byte MaxHeaders = 255;

        /// <summary>
        /// Default constructor
        /// </summary>
        public XPloitMsgHeadersCollection()
        {

        }

        /// <summary>
        /// Check if headers are ok for send
        /// </summary>
        public bool HaveValidHeaders
        {
            get
            {
                int c = Count;
                return c > 0 && c <= MaxHeaders;
            }
        }

        /// <summary>
        /// Get header
        /// </summary>
        /// <typeparam name="T">Type of header</typeparam>
        public T GetHeader<T>() where T : IXploitMsgHeader
        {
            Type tp = typeof(T);

            foreach (IXploitMsgHeader i in this)
            {
                if (i.GetType() == tp) return (T)i;
            }

            return default(T);
        }
    }
}