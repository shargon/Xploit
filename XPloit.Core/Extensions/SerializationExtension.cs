using XPloit.Helpers;

namespace XPloit.Core.Extensions
{
    public static class SerializationExtension
    {
        /// <summary>
        /// Serializa el objeto devuelto
        /// </summary>
        /// <param name="obj">Objeto a devolver</param>
        /// <returns>Devuelve el Json Serializado</returns>
        public static string Serialize2Json(this object obj)
        {
            return JsonHelper.Serialize(obj);
        }
        /// <summary>
        /// Obtiene el objeto del GameAction
        /// </summary>
        /// <typeparam name="T">Objeto a devolver</typeparam>
        /// <param name="json">Json</param>
        /// <returns>Obtiene el objeto del GameAction</returns>
        public static T DeserializeFromJson<T>(this string json)
        {
            return JsonHelper.Deserialize<T>(json);
        }
    }
}