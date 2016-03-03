using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Text;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Attributes
{
    public class SerializableBSONReference : SerializableJSONReference
    {
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="type">Tipo de clase de referencia</param>
        public SerializableBSONReference(Type type) : base(type) { }
        public override byte[] Serialize(Encoding codec, byte type, object msg, byte[] header)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BsonWriter writer = new BsonWriter(ms))
            {
                //grabar la cabecera del mensaje
                if (header != null) ms.Write(header, 0, header.Length);
                //grabar el tipo
                ms.WriteByte((byte)type);
                //grabar el mensaje serializado
                Serializer.Serialize(writer, msg);
                return ms.ToArray();
            }
        }
        public override T Deserialize<T>(Type type, Encoding codec, byte[] data, int index, int length)
        {
            using (MemoryStream ms = new MemoryStream(data, index, length))
            using (BsonReader read = new BsonReader(ms))
                return (T)Serializer.Deserialize(read, type);
        }
    }
}