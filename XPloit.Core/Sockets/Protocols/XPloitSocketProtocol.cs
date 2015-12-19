using System;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Helpers.Crypt;
using XPloit.Core.Sockets.Interfaces;
using XPloit.Core.Sockets.Exceptions;
using System.IO;

namespace XPloit.Core.Sockets.Protocols
{
    // 4 bytes length msg ,msg -> read first 4 bytes length
    public class XPloitSocketProtocol : IXPloitSocketProtocol
    {
        AESHelper _Crypt;
        Encoding _Codec;

        public enum EProtocolMode : byte
        {
            /// <summary>
            /// No header is present
            /// </summary>
            None = 0,
            /// <summary>
            /// Max 255 - 1 byte
            /// </summary>
            Byte = 1,
            /// <summary>
            /// Max 65535 - 2 bytes
            /// </summary>
            UInt16 = 2,
            /// <summary>
            /// Max 16581375 - 3 bytes
            /// </summary>
            UInt24 = 3,
            /// <summary>
            /// Max 2147483647 - 4 bytes
            /// </summary>
            Int32 = 4,
        }

        EProtocolMode _Mode;
        byte _HeaderLength;
        int _MaxLength;
        byte[] _HeaderPadding;

        /// <summary>
        /// Max Length message
        /// </summary>
        public int MaxLength { get { return _MaxLength; } }
        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding Codec { get { return _Codec; } }

        public XPloitSocketProtocol(Encoding codec, AESHelper crypt, EProtocolMode mode)
        {
            _Codec = codec;
            _Crypt = crypt;
            _Mode = mode;

            switch (mode)
            {
                case EProtocolMode.None:
                    {
                        // Header is not present
                        _MaxLength = 0;
                        _HeaderLength = 0;
                        break;
                    }
                default:
                    {
                        // Header is present
                        _HeaderLength = (byte)mode;
                        break;
                    }
            }

            _HeaderPadding = new byte[_HeaderLength];
            _MaxLength = (int)Math.Pow(255, _HeaderLength);
        }
        public XPloitSocketProtocol(Encoding codec, EProtocolMode mode) : this(codec, null, mode) { }
        public XPloitSocketProtocol(AESHelper crypt, EProtocolMode mode) : this(Encoding.UTF8, crypt, mode) { }

        void ReadFull(Stream stream, byte[] data, int index, int length)
        {
            int dv = 0;

            do
            {
                dv = stream.Read(data, index, length);
                length -= dv;
                index += dv;
            }
            while (length > 0);
        }

        public virtual int Send(IXPloitSocketMsg msg, Stream stream)
        {
            if (stream == null || msg == null) return 0;

            byte[] bff;
            if (_Crypt != null)
            {
                bff = msg.Serialize(_Codec, null);
                bff = _Crypt.Encrypt(bff, _HeaderPadding);
            }
            else
            {
                bff = msg.Serialize(_Codec, _HeaderPadding);
            }

            int length = bff.Length;
            if (length == 0) return 0;
            if (length > _MaxLength) throw (new MaxLengthPacketException());

            // Append length to header
            switch (_HeaderLength)
            {
                case 1:
                    {
                        bff[0] = (byte)(length - _HeaderLength);
                        break;
                    }
                case 2:
                    {
                        byte[] bft = BitConverterHelper.GetBytesUInt16((ushort)(length - _HeaderLength));
                        Array.Copy(bft, 0, bff, 0, _HeaderLength);
                        break;
                    }
                case 3:
                    {
                        byte[] bft = BitConverterHelper.GetBytesUInt24((uint)(length - _HeaderLength));
                        Array.Copy(bft, 0, bff, 0, _HeaderLength);
                        break;
                    }
                case 4:
                    {
                        byte[] bft = BitConverterHelper.GetBytesInt32(length - _HeaderLength);
                        Array.Copy(bft, 0, bff, 0, _HeaderLength);
                        break;
                    }
            }

            //Log(bff, 0, length, true, cl.Parent.IsServer);
            stream.Write(bff, 0, length);

            return length;
        }
        public virtual IXPloitSocketMsg Read(Stream stream)
        {
            // Comprobar que la cabera es menor que el paquere
            if (stream == null) return null;

            int msgLength = -1;

            // Asignamos el tamaño del mensaje
            switch (_HeaderLength)
            {
                case 0: { msgLength = 1; break; }
                case 1: { msgLength = stream.ReadByte(); break; }
                case 2:
                    {
                        byte[] bxf = new byte[_HeaderLength];
                        ReadFull(stream, bxf, 0, _HeaderLength);
                        msgLength = (int)BitConverterHelper.ToUInt16(bxf, 0);
                        break;
                    }
                case 3:
                    {
                        byte[] bxf = new byte[_HeaderLength];
                        ReadFull(stream, bxf, 0, _HeaderLength);
                        msgLength = (int)BitConverterHelper.ToUInt24(bxf, 0);
                        break;
                    }
                case 4:
                    {
                        byte[] bxf = new byte[_HeaderLength];
                        ReadFull(stream, bxf, 0, _HeaderLength);
                        msgLength = (int)BitConverterHelper.ToInt32(bxf, 0);
                        break;
                    }
                default: throw (new ProtocolException());
            }

            // Control de tamaño máximo
            if (msgLength > _MaxLength) throw (new ProtocolException());

            int index = 0;
            byte[] bxfData = new byte[msgLength];
            ReadFull(stream, bxfData, 0, msgLength);

            //Log(msg, index - 4, length + 4, false, cl.Parent.IsServer);
            if (_Crypt != null)
            {
                //desencriptamos el mensaje
                bxfData = _Crypt.Decrypt(bxfData, index, ref msgLength);
                if (bxfData == null) return null;

                index = 0;
            }

            return IXPloitSocketMsg.Deserialize(_Codec, bxfData, index, msgLength);
        }

        public bool Connect(XPloitSocketClient client) { return true; }
    }
}