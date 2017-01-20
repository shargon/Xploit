using Xploit.Helpers.Geolocate;

namespace Xploit.Sniffer.Interfaces
{
    public interface ICountryRecaller
    {
        bool RecallCounty(ILocationProvider current);
    }
}