using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xploit.Server.Http.Enums;

namespace Xploit.Server.Http
{
    public class Html5SocketMsg
    {

        string msg = null;
        Encoding _codec = null;
        byte[] _data = null;
        ESocketDataType _tip = ESocketDataType.text;

        public ESocketDataType Type { get { return _tip; } }
        public byte[] Data { get { return _data; } }
        public string DataString
        {
            get
            {
                if (msg != null) return msg;
                msg = _codec.GetString(_data);
                return msg;
            }
        }
        public Html5SocketMsg(Encoding codec, ESocketDataType tp, byte[] msg)
        {
            _codec = codec;
            _tip = tp;
            _data = msg;
            _tip = tp;
        }
        public static Html5SocketMsg GetMsg(Encoding codec, Stream stream)
        {
            //https://github.com/lemmingzshadow/php-websocket/blob/master/server/lib/WebSocket/Connection.php
            byte[] r2 = new byte[2];
            if (stream.Read(r2, 0, 2) != 2) throw (new Exception("Read Error"));

            string firstByteBinary = Convert.ToString(r2[0], 2);//.PadLeft(8, '0')
            string secondByteBinary = Convert.ToString(r2[1], 2);//.PadLeft(8, '0')
            int opcode = Convert.ToInt32(firstByteBinary.Substring(4, 4), 2);

            ESocketDataType type = ESocketDataType.close;
            switch (opcode)
            {
                case 1: type = ESocketDataType.text; break;
                case 2: type = ESocketDataType.binary; break;
                case 8: type = ESocketDataType.close; break;
                case 9: type = ESocketDataType.ping; break;
                case 10: type = ESocketDataType.pong; break;
                default: throw (new Exception("unknown data (opcode)"));
            }

            int payloadLength = r2[1] & 127;
            bool isMasked = secondByteBinary[0] == '1';

            //if (!isMasked) { close(1002); }

            int dataLength = 0;
            byte[] mask = null;
            if (payloadLength == 126)
            {
                mask = new byte[6];
                if (stream.Read(mask, 0, 6) != 6) throw (new Exception("Error in length"));
                r2[0] = mask[1];
                r2[1] = mask[0];
                dataLength = BitConverter.ToInt16(r2, 0);//invert
            }
            else if (payloadLength == 127)
            {
                mask = new byte[12];
                if (stream.Read(mask, 0, 12) != 12) throw (new Exception("Error in length"));
                byte[] bl = new byte[8];
                for (int x = 0; x < 8; x++) bl[7 - x] = mask[x];//invert
                dataLength = (int)BitConverter.ToInt64(bl, 0);
            }
            else
            {
                mask = new byte[4];
                if (stream.Read(mask, 0, 4) != 4) throw (new Exception("Error in length"));
                dataLength = payloadLength;
            }

            byte[] data = new byte[dataLength];
            int ix = 0, rest = dataLength, lee = 1;

            while (rest > 0 && lee > 0) { lee = stream.Read(data, ix, rest); ix += lee; rest -= lee; }
            if (lee == 0 && rest != 0) throw (new Exception("Disconnected!"));

            if (isMasked)
            {
                int index_mask = mask.Length - 4;
                for (int i = 0; i < dataLength; i++)
                {
                    data[i] = (byte)(data[i] ^ mask[index_mask + (i % 4)]);
                }
            }

            return new Html5SocketMsg(codec, type, data);
        }
        static string SOCKET_13_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        static SHA1 sha1 = SHA1CryptoServiceProvider.Create();
        public static String ComputeWebSocketHandshakeSecurityHash09(string secWebSocketKey)
        {
            // 1. Combine the request Sec-WebSocket-Key with magic key.
            secWebSocketKey = secWebSocketKey + SOCKET_13_GUID;
            // 2. Compute the SHA1 hash
            byte[] sha1Hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey));
            // 3. Base64 encode the hash
            string secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            return secWebSocketAccept;
        }
        public static byte[] hybi10Encode(byte[] payload, ESocketDataType type/*, bool with_mask*/)
        {
            //https://github.com/lemmingzshadow/php-websocket/blob/master/server/lib/WebSocket/Connection.php
            int payloadLength = payload.Length;
            byte btype = 0;
            switch (type)
            {
                case ESocketDataType.text: btype = 129; break;
                case ESocketDataType.close: btype = 136; break;
                case ESocketDataType.ping: btype = 137; break;
                case ESocketDataType.pong: btype = 138; break;
            }

            int ix = 2;
            byte[] send = null;
            if (payloadLength > 65535)
            {
                byte[] bx = BitConverter.GetBytes((long)payloadLength);
                send = new byte[payloadLength + 10];
                send[0] = btype;
                send[1] = 127;
                send[2] = bx[7];
                send[3] = bx[6];
                send[4] = bx[5];
                send[5] = bx[4];
                send[6] = bx[3];
                send[7] = bx[2];
                send[8] = bx[1];
                send[9] = bx[0];
                ix = 10;

                if (send[2] > 127) { throw (new Exception("TO BIG PACKET")); }
            }
            else if (payloadLength > 125)
            {
                byte[] bx = BitConverter.GetBytes((short)payloadLength);
                send = new byte[payloadLength + 4];
                send[0] = btype;
                send[1] = 126;
                send[2] = bx[1];
                send[3] = bx[0];
                ix = 4;
            }
            else
            {
                send = new byte[payloadLength + 2];
                send[0] = btype;
                send[1] = (byte)payloadLength;
            }
            Array.Copy(payload, 0, send, ix, payloadLength);
            return send;
        }
    }
}