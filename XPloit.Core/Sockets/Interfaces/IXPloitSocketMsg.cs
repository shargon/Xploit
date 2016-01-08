using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using XPloit.Core.Attributes;
using XPloit.Core.Extensions;
using XPloit.Core.Sockets.Enums;

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
        /// Message Id
        /// </summary>
        [DataMember(Name = "i")]
        public Guid Id { get; internal set; }
        /// <summary>
        /// Response Id
        /// </summary>
        [DataMember(Name = "r")]
        public Guid InResponseTo { get; set; }
        //Variable para acelerar la reflexión
        static Dictionary<byte, SerializableJSONReferenceAttribute> _Cache = new Dictionary<byte, SerializableJSONReferenceAttribute>();
        /// <summary>
        /// Protected constructor
        /// </summary>
        protected IXPloitSocketMsg()
        {
            Id = Guid.NewGuid();
            InResponseTo = Guid.Empty;
        }
        /// <summary>
        /// Obtiene el array de bytes de la clase actual
        /// </summary>
        /// <param name="codec">Codec para la obtención</param>
        /// <param name="header">Cabecera del mensaje</param>
        /// <returns>Devuelve el Array de Bytes de la clase actual</returns>
        public byte[] Serialize(Encoding codec, byte[] header)
        {
            SerializableJSONReferenceAttribute tp;
            EXPloitSocketMsg type = this.Type;

            if (!_Cache.TryGetValue((byte)type, out tp))
            {
                //no está en cache
                tp = type.GetAttribute<SerializableJSONReferenceAttribute>();
                lock (_Cache) { _Cache.Add((byte)type, tp); }
            }

            return tp.Serialize(codec, type, this, header);
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
            byte type = data[index];

            SerializableJSONReferenceAttribute tp;
            if (!_Cache.TryGetValue(type, out tp))
            {
                //no está en cache
                EXPloitSocketMsg e = (EXPloitSocketMsg)type;
                tp = e.GetAttribute<SerializableJSONReferenceAttribute>();
                lock (_Cache)
                {
                    _Cache.Add(type, tp);
                }
            }

            return tp.Deserialize(tp.Type, codec, data, index + 1, length - 1);
        }
        /// <summary>
        /// Genera un TCP SocketManager del array de bytes de los datos solicitados
        /// </summary>
        /// <param name="codec">Codec para la obtención</param>
        /// <param name="stream">Stream</param>
        /// <returns>Devuelve la clase TCPSocketMsgManager</returns>
        public static IXPloitSocketMsg Deserialize(Encoding codec, Stream stream)
        {
            byte type = (byte)stream.ReadByte();

            SerializableJSONReferenceAttribute tp;
            if (!_Cache.TryGetValue(type, out tp))
            {
                //no está en cache
                EXPloitSocketMsg e = (EXPloitSocketMsg)type;
                tp = e.GetAttribute<SerializableJSONReferenceAttribute>();
                lock (_Cache)
                {
                    _Cache.Add(type, tp);
                }
            }

            return tp.Deserialize(tp.Type, codec, stream);
        }
    }
}