using XPloit.Helpers.Geolocate;

namespace XPloit.Sniffer.Interfaces
{
    public interface ICountryRecaller
    {
        bool RecallCounty(ILocationProvider current);
    }
}