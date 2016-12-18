using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using XPloit.Core.Attributes;
using XPloit.Core.Extensions;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Headers;
using XPloit.Helpers;

namespace XPloit.Core.Sockets.Interfaces
{
    [DataContract]
    public class IXPloitSocketMsg
    {
        /// <summary>
        /// Especifica el tipo de la clase
        /// </summary>
        public virtual EXPloitSocketMsg Type { get { throw (new Exception("ERROR")); } }

        /// <summary>
        /// Headers
        /// </summary>
        [JsonIgnore]
        public XPloitMsgHeadersCollection Headers { get; set; }

        //Variable para acelerar la reflexión
        static Dictionary<byte, SerializableJSONReference>
            _Cache = new Dictionary<byte, SerializableJSONReference>(),
            _CacheHeaders = new Dictionary<byte, SerializableJSONReference>();

        /// <summary>
        /// Protected constructor
        /// </summary>
        protected IXPloitSocketMsg()
        {
            Headers = new XPloitMsgHeadersCollection();
        }
        /// <summary>
        /// Obtiene el array de bytes de la clase actual
        /// </summary>
        /// <param name="codec">Codec para la obtención</param>
        /// <param name="header">Cabecera del mensaje</param>
        /// <returns>Devuelve el Array de Bytes de la clase actual</returns>
        public byte[] Serialize(Encoding codec, byte[] header)
        {
            SerializableJSONReference tp;
            EXPloitSocketMsg type = this.Type;

            if (header == null) header = new byte[1];
            else Array.Resize(ref header, header.Length + 1);

            if (Headers.HaveValidHeaders)
            {
                // Headers length
                byte hay = (byte)Headers.Count;
                header[header.Length - 1] = hay;

                // Headers
                int l = 0;
                List<byte[]> ret = new List<byte[]>();
                foreach (IXploitMsgHeader h in Headers)
                {
                    SerializableJSONReference tp2;
                    EXPloitSocketMsgHeader type2 = h.Type;

                    // Message
                    if (!_CacheHeaders.TryGetValue((byte)type2, out tp2))
                    {
                        //no está en cache
                        tp2 = type2.GetAttribute<SerializableJSONReference>();
                        lock (_CacheHeaders) { _CacheHeaders.Add((byte)type2, tp2); }
                    }

                    byte[] data = tp2.Serialize(codec, (byte)type2, h, null);
                    ret.Add(data);
                    l += data.Length;
                }

                int x = header.Length;
                Array.Resize(ref header, x + l + (hay * 2));

                foreach (byte[] b in ret)
                {
                    ushort lb = (ushort)b.Length;

                    byte[] bl = BitConverterHelper.GetBytesUInt16(lb);
                    Array.Copy(bl, 0, header, x, 2);
                    x += 2;

                    Array.Copy(b, 0, header, x, lb);
                    x += lb;
                }
                ret.Clear();
            }

            // Message
            if (!_Cache.TryGetValue((byte)type, out tp))
            {
                //no está en cache
                tp = type.GetAttribute<SerializableJSONReference>();
                lock (_Cache) { _Cache.Add((byte)type, tp); }
            }

            return tp.Serialize(codec, (byte)type, this, header);
        }
        /// <summary>
        /// Genera un TCP SocketManager del array de bytes de los datos solicitados
        /// </summary>
        /// <param name="codec">Codec para la obtención</param>
        /// <param name="data">Array de bytes</param>
        /// <param name="index">Inicio en el array</param>
        /// <param name="length">Longitud de lectura</param>
        /// <returns>Devuelve la clase TCPSocketMsgManager</returns>
        public static IXPloitSocketMsg Deserialize(Encoding codec, byte[] data, int index, int length)
        {
            using (MemoryStream ms = new MemoryStream(data, index, length))
                return Deserialize(codec, ms);
        }
        /// <summary>
        /// Genera un TCP SocketManager del array de bytes de los datos solicitados
        /// </summary>
        /// <param name="codec">Codec para la obtención</param>
        /// <param name="stream">Stream</param>
        /// <returns>Devuelve la clase TCPSocketMsgManager</returns>
        public static IXPloitSocketMsg Deserialize(Encoding codec, Stream stream)
        {
            List<IXploitMsgHeader> headers = new List<IXploitMsgHeader>();
            byte nheaders = (byte)stream.ReadByte();
            byte type;
            SerializableJSONReference tp;

            if (nheaders > 0)
            {
                // Read headers

                byte[] bl = new byte[3];
                for (int x = 0; x < nheaders; x++)
                {
                    // Read header Length
                    stream.Read(bl, 0, 3);
                    type = bl[2];

                    byte[] data = new byte[BitConverterHelper.ToUInt16(bl, 0) - 1];
                    stream.Read(data, 0, data.Length);

                    if (!_CacheHeaders.TryGetValue(type, out tp))
                    {
                        //no está en cache
                        EXPloitSocketMsgHeader e = (EXPloitSocketMsgHeader)type;
                        tp = e.GetAttribute<SerializableJSONReference>();
                        lock (_CacheHeaders)
                        {
                            _CacheHeaders.Add(type, tp);
                        }
                    }

                    headers.Add(tp.Deserialize<IXploitMsgHeader>(tp.Type, codec, data, 0, data.Length));
                }
            }

            type = (byte)stream.ReadByte();

            if (!_Cache.TryGetValue(type, out tp))
            {
                //no está en cache
                EXPloitSocketMsg e = (EXPloitSocketMsg)type;
                tp = e.GetAttribute<SerializableJSONReference>();
                lock (_Cache)
                {
                    _Cache.Add(type, tp);
                }
            }

            IXPloitSocketMsg ret = tp.Deserialize<IXPloitSocketMsg>(tp.Type, codec, stream);
            if (nheaders > 0) ret.Headers.AddRange(headers);
            return ret;
        }
    }
}