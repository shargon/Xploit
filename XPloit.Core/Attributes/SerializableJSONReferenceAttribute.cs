using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Attributes
{
    public class SerializableJSONReferenceAttribute : Attribute
    {
        protected static JsonSerializer Serializer = new JsonSerializer();
        Type _Type;

        /// <summary>
        /// Tipo
        /// </summary>
        public Type Type { get { return _Type; } }
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="type">Tipo de clase de referencia</param>
        public SerializableJSONReferenceAttribute(Type type) { _Type = type; }

        public virtual byte[] Serialize(Encoding codec, EXPloitSocketMsg type, IXPloitSocketMsg msg, byte[] header)
        {
            string smsg = SerializationJsonHelper.Serialize(msg);
            byte[] data = codec.GetBytes(smsg);
            int l = data.Length;

            int header_length = header == null ? 0 : header.Length;
            byte[] all = new byte[l + header_length + 1];
            Array.Copy(data, 0, all, header_length + 1, l);
            all[header_length] = (byte)type;
            return all;
        }
        public virtual IXPloitSocketMsg Deserialize(Type type, byte[] data, int index, int length)
        {
            using (MemoryStream ms = new MemoryStream(data, index, length))
            using (StreamReader sr = new StreamReader(ms))
            using (JsonTextReader read = new JsonTextReader(sr))
                return (IXPloitSocketMsg)Serializer.Deserialize(read, type);
        }
    }
}