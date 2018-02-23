using System.IO;
using System.Text;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;
using XPloit.Core.Sockets.Messages;

namespace XPloit.Core.Sockets.Protocols
{
    public class XPloitTelnetProtocol : IXPloitSocketProtocol
    {
        Encoding _Codec;

        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding Codec { get { return _Codec; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="codec">Encoding</param>
        public XPloitTelnetProtocol(Encoding codec) { _Codec = codec; }

        //https://support.microsoft.com/en-us/kb/231866
        //public static byte[] GetColorMessage()
        //{
        //    return new byte[] 
        //        { 
        //           255, 0x1B, (byte)'[', 
        //            38, 2, 
        //            0, 187, 0,
        //            (byte)'m' 
        //        };
        //}

        #region Allowed send
        public static void Send(XPloitSocketClient client, string data) { client.Send(new XPloitMsgString() { Data = data }); }
        public static void Send(XPloitSocketClient client, byte[] data) { client.Send(new XPloitMsgByteArray() { Data = data }); }
        #endregion

        public virtual int Send(IXPloitSocketMsg msg, Stream stream)
        {
            if (stream == null || msg == null) return 0;

            byte[] bff;
            switch (msg.Type)
            {
                case EXPloitSocketMsg.String:
                    {
                        XPloitMsgString send = (XPloitMsgString)msg;
                        bff = _Codec.GetBytes(send.Data);
                        break;
                    }
                case EXPloitSocketMsg.ByteArray:
                    {
                        XPloitMsgByteArray send = (XPloitMsgByteArray)msg;
                        bff = send.Data;
                        break;
                    }
                default: { bff = msg.Serialize(_Codec, null); break; }
            }

            int length = bff.Length;
            if (length == 0) return 0;

            stream.Write(bff, 0, length);
            return length;
        }
        public virtual IXPloitSocketMsg Read(Stream stream)
        {
            // Comprobar que la cabera es menor que el paquere
            if (stream == null) return null;

            // String hasta \r o \n

            StringBuilder sb = new StringBuilder();

            int lee;
            while ((lee = stream.ReadByte()) != -1)
            {
                if (lee == 0 || lee == 10 || lee == 13) break;
                sb.Append((char)lee);
            }

            if (sb.Length == 0) return null;

            return new XPloitMsgString() { Data = sb.ToString() };
        }
        public bool Connect(XPloitSocketClient client)
        {
            return true;
        }
    }
}