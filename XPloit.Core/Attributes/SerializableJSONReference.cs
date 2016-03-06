using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Attributes
{
    public class SerializableJSONReference : Attribute
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
        public SerializableJSONReference(Type type) { _Type = type; }

        public virtual byte[] Serialize(Encoding codec, byte type, object msg, byte[] header)
        {
            string smsg = JsonHelper.Serialize(msg, false, false);
            byte[] data = codec.GetBytes(smsg);
            int l = data.Length;

            int header_length = header == null ? 0 : header.Length;
            byte[] all = new byte[l + header_length + 1];

            if (header_length > 0)
                Array.Copy(header, 0, all, 0, header_length);

            Array.Copy(data, 0, all, header_length + 1, l);
            all[header_length] = type;
            return all;
        }
        public virtual T Deserialize<T>(Type type, Encoding codec, byte[] data, int index, int length)
        {
            using (MemoryStream ms = new MemoryStream(data, index, length))
                return Deserialize<T>(type, codec, ms);
        }
        public virtual T Deserialize<T>(Type type, Encoding codec, Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream, codec))
            using (JsonTextReader read = new JsonTextReader(sr))
                return (T)Serializer.Deserialize(read, type);
        }
    }
}