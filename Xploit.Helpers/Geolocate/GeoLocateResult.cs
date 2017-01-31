namespace XPloit.Helpers.Geolocate
{
    public class GeoLocateResult
    {
        /// <summary>
        /// Código ISO
        /// </summary>
        public string ISOCode { get; private set; }
        /// <summary>
        /// Pais
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isoCode">Código ISO</param>
        /// <param name="country">Pais</param>
        public GeoLocateResult(string isoCode, string country)
        {
            ISOCode = isoCode;
            Country = country;
        }

        public static GeoLocateResult Private = new GeoLocateResult("Private", "Private");
        public static GeoLocateResult Empty = new GeoLocateResult("Empty", "Empty");

        public override string ToString()
        {
            return ISOCode + " [" + Country + "]";
        }
    }
}