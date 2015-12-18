using XPloit.Core.Helpers;

namespace XPloit.Core.Extensions
{
    public static class SerializationExtension
    {
        /// <summary>
        /// Serializa el objeto devuelto
        /// </summary>
        /// <param name="obj">Objeto a devolver</param>
        /// <returns>Devuelve el Json Serializado</returns>
        public static string Serialize(this object obj)
        {
            return SerializationHelper.Serialize(obj);
        }
        /// <summary>
        /// Obtiene el objeto del GameAction
        /// </summary>
        /// <typeparam name="T">Objeto a devolver</typeparam>
        /// <param name="json">Json</param>
        /// <returns>Obtiene el objeto del GameAction</returns>
        public static T Deserialize<T>(this string json)
        {
            return SerializationHelper.Deserialize<T>(json);
        }
    }
}