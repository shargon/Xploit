using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;

namespace XPloit.Helpers
{
    public class JsonHelper
    {
        //static JavaScriptSerializer JSON = new JavaScriptSerializer();
        /// <summary>
        /// Serializa el objeto devuelto
        /// </summary>
        /// <param name="obj">Objeto a devolver</param>
        /// <returns>Devuelve el Json Serializado</returns>
        public static string Serialize(object obj) { return Serialize(obj, false, true); }
        /// <summary>
        /// Serializa el objeto devuelto
        /// </summary>
        /// <param name="obj">Objeto a devolver</param>
        /// <param name="indent">True si se desea indentado</param>
        /// <param name="serializeNull">True si los null hay que serializarlos</param>
        /// <returns>Devuelve el Json Serializado</returns>
        public static string Serialize(object obj, bool indent, bool serializeNull)
        {
            return JsonConvert.SerializeObject(obj, indent ? Formatting.Indented : Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = serializeNull ? NullValueHandling.Include : NullValueHandling.Ignore
                //PreserveReferencesHandling = PreserveReferencesHandling.Objects
            }
                );
        }
        /// <summary>
        /// Obtiene el objeto
        /// </summary>
        /// <param name="json">Json</param>
        /// <returns>Obtiene el objeto</returns>
        public static object Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
                try { return JsonConvert.DeserializeObject(json); }
                catch { }

            return null;
        }
        /// <summary>
        /// Obtiene el objeto
        /// </summary>
        /// <param name="json">Json</param>
        /// <param name="tp">Tipo del objeto a deserializar</param>
        /// <returns>Obtiene el objeto</returns>
        public static object Deserialize(string json, Type tp)
        {
            if (!string.IsNullOrEmpty(json))
                try { return JsonConvert.DeserializeObject(json, tp); }
                catch { }

            return null;
        }
        /// <summary>
        /// Obtiene el objeto
        /// </summary>
        /// <typeparam name="T">Objeto a devolver</typeparam>
        /// <param name="json">Json</param>
        /// <returns>Obtiene el objeto</returns>
        public static T Deserialize<T>(string json)
        {
            if (!string.IsNullOrEmpty(json))
                try { return JsonConvert.DeserializeObject<T>(json); }
                catch { }

            return default(T);
        }
        /// <summary>
        /// Serializa un objeto a BSON
        /// </summary>
        /// <param name="obj">Objeto a devolver</param>
        /// <returns>Devuelve el Bson Serializado</returns>
        public static byte[] SerializeBSON(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                JsonSerializer serializer = new JsonSerializer();

                using (BsonWriter writer = new BsonWriter(ms))
                    serializer.Serialize(writer, obj);

                return ms.ToArray();
            }
        }
        /// <summary>
        /// Obtiene el objeto
        /// </summary>
        /// <typeparam name="T">Objeto a devolver</typeparam>
        /// <param name="bson">Bson</param>
        /// <returns>Obtiene el objeto</returns>
        public static T DeserializeBSON<T>(byte[] bson)
        {
            if (bson == null || bson.Length == 0) return default(T);

            try
            {
                JsonSerializer serializer = new JsonSerializer();
                using (MemoryStream ms = new MemoryStream(bson))
                {
                    using (BsonReader reader = new BsonReader(ms))
                        return serializer.Deserialize<T>(reader);
                }
            }
            catch { }
            return default(T);
        }
    }
}
