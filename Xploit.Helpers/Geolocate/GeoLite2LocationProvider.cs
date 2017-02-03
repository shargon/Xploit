using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace XPloit.Helpers.Geolocate
{
    public class GeoLite2LocationProvider : ILocationProvider
    {
        class geoIp
        {
            public IPNetwork Network;
            public GeoLocateResult Result;

            public geoIp(IPNetwork network, GeoLocateResult data)
            {
                Network = network;
                Result = data;
            }

            public override string ToString()
            {
                return Network.ToString() + " - " + Result.ToString();
            }
        }

        static GeoLite2LocationProvider _Current;
        /// <summary>
        /// Geolocalization
        /// </summary>
        public static GeoLite2LocationProvider Current { get { return _Current; } }
        /// <summary>
        /// Load the current GeoIp
        /// </summary>
        /// <param name="blockFile">Block file</param>
        /// <param name="locationFile">Location file</param>
        public static bool LoadCurrent(string blockFile, string locationFile)
        {
            if (_Current == null || _Current.Count <= 0)
                _Current = new GeoLite2LocationProvider(blockFile, locationFile);

            return _Current != null && _Current.Count > 0;
        }
        geoIp[] _Locates;

        public int Count { get { return _Locates == null ? 0 : _Locates.Length; } }

        public GeoLocateResult[] Countries { get { return _Locates.Select(u => u.Result).Distinct().ToArray(); } }

        public static IEnumerable<string> LineGenerator(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null) yield return line;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pathBlocksIp">Ruta de bloques ip</param>
        /// <param name="pathLocations">Ruta de posiciones</param>
        public GeoLite2LocationProvider(string pathBlocksIp, string pathLocations)
        {
            if (File.Exists(pathBlocksIp) && File.Exists(pathLocations))
            {
                Dictionary<long, GeoLocateResult> data = new Dictionary<long, GeoLocateResult>();

                using (Stream fs = CompressHelper.UnGzFile(pathLocations))
                using (StreamReader sr = new StreamReader(fs))
                {
                    Parallel.ForEach<string>(LineGenerator(sr), line =>
                    {
                        string[] sp = line.Split(',');
                        if (sp.Length <= 0) return;

                        long c;
                        if (long.TryParse(sp[0], out c))
                        {
                            switch (sp.Length)
                            {
                                case 6:
                                    {
                                        // Archivo de paises
                                        GeoLocateResult add = new GeoLocateResult(sp[2] + "-" + sp[4], (string.IsNullOrEmpty(sp[3]) ? "" : sp[3] + " - ") + sp[5]);
                                        lock (data) data.Add(c, add);
                                        break;
                                    }
                                case 13:
                                    {
                                        // Archivo de paises y poblaciones
                                        GeoLocateResult add = new GeoLocateResult(sp[2] + "-" + sp[4], (string.IsNullOrEmpty(sp[12]) ? "" : sp[12] + " - ") + sp[5]);
                                        lock (data) data.Add(c, add);
                                        break;
                                    }
                            }

                        }
                    }
                    );
                }

                List<geoIp> l = new List<geoIp>();
                using (Stream fs = CompressHelper.UnGzFile(pathBlocksIp))
                using (StreamReader sr = new StreamReader(fs))
                {
                    Parallel.ForEach<string>(LineGenerator(sr), line =>
                    {
                        string[] sp = line.Split(',');
                        // El archivo grande y pequeño están organizados igual
                        if (sp.Length != 10 && sp.Length != 6) return;

                        long c;
                        GeoLocateResult r;
                        if (long.TryParse(sp[2], out c) && data.TryGetValue(c, out r))
                        {
                            IPNetwork net;
                            if (IPNetwork.TryParse(sp[0], out net))
                            {
                                geoIp add = new geoIp(net, r);
                                lock (l) l.Add(add);
                            }
                        }
                    }
                    );
                }
                _Locates = l.ToArray();
            }
        }
        /// <summary>
        /// Localiza la ip
        /// </summary>
        /// <param name="address">Direccón</param>
        public GeoLocateResult LocateIp(IPAddress address)
        {
            foreach (geoIp g in _Locates)
                if (IPNetwork.Contains(g.Network, address))
                    return g.Result;

            return GeoLocateResult.Empty;
        }
    }
}