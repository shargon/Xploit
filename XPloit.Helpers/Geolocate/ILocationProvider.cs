using System.Net;

namespace XPloit.Helpers.Geolocate
{
    public interface ILocationProvider
    {
        /// <summary>
        /// Cantidad de registros contenidos
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Posiciona la ip
        /// </summary>
        /// <param name="address">Dirección</param>
        GeoLocateResult LocateIp(IPAddress address);
    }
}