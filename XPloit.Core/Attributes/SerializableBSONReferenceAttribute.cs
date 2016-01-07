using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Text;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Attributes
{
    public class SerializableBSONReferenceAttribute : SerializableJSONReferenceAttribute
    {
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="type">Tipo de clase de referencia</param>
        public SerializableBSONReferenceAttribute(Type type) : base(type) { }
        public override byte[] Serialize(Encoding codec, EXPloitSocketMsg type, IXPloitSocketMsg msg, byte[] header)
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
        public override IXPloitSocketMsg Deserialize(Type type, Encoding codec, byte[] data, int index, int length)
        {
            using (MemoryStream ms = new MemoryStream(data, index, length))
            using (BsonReader read = new BsonReader(ms))
                return (IXPloitSocketMsg)Serializer.Deserialize(read, type);
        }
    }
}