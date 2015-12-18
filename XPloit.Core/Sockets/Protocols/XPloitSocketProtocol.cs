using XPloit.Core.Sockets.Enums;
using System;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Helpers.Crypt;
using XPloit.Core.Sockets.Interfaces;
using XPloit.Core.Sockets.Exceptions;
using System.Collections.Generic;

namespace XPloit.Core.Sockets.Protocols
{
    //4 bytes length msg ,msg -> read first 4 bytes length
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
            _MaxLength = (int)Math.Pow(255, _HeaderLength) - _HeaderLength;
        }
        public XPloitSocketProtocol(Encoding codec, EProtocolMode mode) : this(codec, null, mode) { }
        public XPloitSocketProtocol(AESHelper crypt, EProtocolMode mode) : this(Encoding.UTF8, crypt, mode) { }

        public virtual int Send(XPloitSocketClient cl, IXPloitSocketMsg msg)
        {
            if (cl == null || msg == null) return 0;

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
            cl.Write(bff, 0, length, true);

            return length;
        }
        public virtual IEnumerable<IXPloitSocketMsg> ProcessBuffer(XPloitSocketClient cl, ref byte[] bxf)
        {
            // Comprobar que la cabera es menor que el paquere
            int bjl = bxf == null ? 0 : bxf.Length;
            int bl = bjl - _HeaderLength;
            if (bl <= 0) return new IXPloitSocketMsg[] { };

            // Parsear
            List<IXPloitSocketMsg> lret = new List<IXPloitSocketMsg>();
            int ml, x = 0;
            try
            {
                while (x < bl)
                {
                    // Choose header
                    switch (_HeaderLength)
                    {
                        case 0: { ml = 1; break; }
                        case 1: { ml = bxf[0]; break; }
                        case 2: { ml = (int)BitConverterHelper.ToUInt16(bxf, 0); break; }
                        case 3: { ml = (int)BitConverterHelper.ToUInt24(bxf, 0); break; }
                        case 4: { ml = (int)BitConverterHelper.ToInt32(bxf, 0); break; }

                        default: throw (new ProtocolException());
                    }

                    // Control de tamaño máximo
                    if (ml > _MaxLength) cl.Disconnect(EDissconnectReason.Error);

                    if (bl >= ml)
                    {
                        x += _HeaderLength;

                        IXPloitSocketMsg msg = XPloitSocketProtocol.ProcessMessage(_Crypt, _Codec, bxf, x, ml);
                        x += ml;
                        if (msg != null) lret.Add(msg);

                        continue;
                    }

                    break;
                }

                int nl = bjl - x;

                // se consume todo
                if (nl == 0) bxf = new byte[] { };
                else
                {
                    // no llega a consumir ningún paquete
                    if (bjl != nl && nl > 0)
                    {
                        // recoger lo sobrante y modificarlo
                        byte[] bff = new byte[nl];
                        Array.Copy(bxf, x, bff, 0, nl);

                        bxf = bff;
                    }
                    else
                    {
                        if (nl < 0) throw new ProtocolException();
                    }
                }
                // desconexion (pierde el buffer)

                return lret;
            }
            catch
            {
                cl.Disconnect(EDissconnectReason.Error);

                return lret;
            }
        }

        internal static IXPloitSocketMsg ProcessMessage(AESHelper crypt, Encoding codec, byte[] msg, int index, int length)
        {
            if (msg == null || index < 0 || length <= 0) return null;

            //Log(msg, index - 4, length + 4, false, cl.Parent.IsServer);
            if (crypt != null)
            {
                //desencriptamos el mensaje
                msg = crypt.Decrypt(msg, index, ref length);
                if (msg == null) return null;

                index = 0;
            }

            return IXPloitSocketMsg.Deserialize(codec, msg, index, length);
        }
    }
}