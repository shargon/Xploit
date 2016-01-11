using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace XPloit.Core.Helpers
{
    class TorExitNoteHelper
    {
        // https://www.torproject.org/projects/tordnsel.html.en
        // https://check.torproject.org/exit-addresses

        public class ExitNode
        {
            string _ExitAddressString, _ExitNodeId;
            IPAddress _ExitAddress;
            DateTime _Published, _LastStatus, _ExitAddressDate;

            /// <summary>
            /// Id del nodo
            /// </summary>
            public string ExitNodeId { get { return _ExitNodeId; } }
            /// <summary>
            /// Fecha de publicación
            /// </summary>
            public DateTime Published { get { return _Published; } }
            /// <summary>
            /// Último estado
            /// </summary>
            public DateTime LastStatus { get { return _LastStatus; } }
            /// <summary>
            /// Dirección de salida
            /// </summary>
            public IPAddress ExitAddress { get { return _ExitAddress; } }
            /// <summary>
            /// Dirección de salida en String
            /// </summary>
            public string ExitAddressString { get { return _ExitAddressString; } }
            /// <summary>
            /// Fecha de salida
            /// </summary>
            public DateTime ExitAddressDate { get { return _ExitAddressDate; } }

            /// <summary>
            /// Constructor
            /// </summary>
            public ExitNode(string exitNodeId, DateTime published, DateTime lastStatus, DateTime exitAddressDate, IPAddress exitAddress)
            {
                _ExitNodeId = exitNodeId;
                _ExitAddress = exitAddress;
                _Published = published;
                _LastStatus = lastStatus;

                _ExitAddressDate = exitAddressDate;
                _ExitAddressString = exitAddress == null ? null : exitAddress.ToString();
            }

            public override string ToString() { return _ExitNodeId + " - " + _ExitAddressString; }
        }

        static ExitNode[] _ExitNodes = null;
        /// <summary>
        /// Nodos de salida
        /// </summary>
        public static ExitNode[] ExitNodes
        {
            get
            {
                if (_ExitNodes == null) UpdateTorExitNodeList();
                return _ExitNodes;
            }
        }

        /// <summary>
        /// Devuelve si es un nodo de salida Tor
        /// </summary>
        /// <param name="ip">Ip</param>
        /// <param name="port">Puerto</param>
        /// <param name="remote">Ip de nuetro servicio</param>
        /// <returns>Devuelve si es un nodo de salida Tor</returns>
        //public static bool IsTorExitNode(IPAddress ip, ushort port, IPAddress remote)
        //{
        //    //            function IsTorExitPoint(){
        //    //    if (gethostbyname(ReverseIPOctets($_SERVER['REMOTE_ADDR']).".".$_SERVER['SERVER_PORT'].".".ReverseIPOctets($_SERVER['SERVER_ADDR']).".ip-port.exitlist.torproject.org")=="127.0.0.2") {
        //    //        return true;
        //    //    } else {
        //    //       return false;
        //    //    } 
        //    //}

        //    //function ReverseIPOctets($inputip){
        //    //    $ipoc = explode(".",$inputip);
        //    //    return $ipoc[3].".".$ipoc[2].".".$ipoc[1].".".$ipoc[0];
        //    //}

        //    string sIP = ip.ToString();
        //    string[] reverse = remote.ToString().Split('.');
        //    Array.Reverse(reverse);
        //    // Reverse Ip
        //    string sremoteIP = string.Join(".", reverse);

        //    string strTor = sIP + "." + port.ToString() + "." + sremoteIP + ".ip-port.exitlist.torproject.org";
        //    IPHostEntry host = Dns.GetHostByName(strTor);

        //    try
        //    {
        //        if (host.AddressList.Length == 0) return false;
        //        if (host.AddressList[0].ToString() == "127.0.0.2") return true;
        //    }
        //    catch { }
        //    return false;
        //}
        /// <summary>
        /// Actualiza la lista de Nodos de Tor
        /// </summary>
        /// <returns>Devuelve el número de nodos de salida</returns>
        public static int UpdateTorExitNodeList()
        {
            List<ExitNode> ls = new List<ExitNode>();
            try
            {
                using (WebClient wb = new WebClient())
                {
                    string[] dv = wb.DownloadString("https://check.torproject.org/exit-addresses").Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    string exitNodeId = null;
                    DateTime published = DateTime.MinValue;
                    DateTime lastStatus = DateTime.MinValue;
                    IPAddress exitAddress = IPAddress.None;
                    DateTime exitAddressDate = DateTime.MinValue;

                    foreach (string s in dv)
                    {
                        // ExitNode 00654C9E1A9E821976A0F4DB52ACF93DB8523470
                        // Published 2014-12-16 07:19:19
                        // LastStatus 2014-12-16 08:02:39
                        // ExitAddress 217.8.57.105 2014-12-16 08:03:09
                        string iz, dr;
                        int ix = s.IndexOf(' ');
                        if (ix == -1) continue;
                        iz = s.Substring(0, ix);
                        dr = s.Remove(0, ix + 1);

                        switch (iz)
                        {
                            case "ExitNode": exitNodeId = dr; break;
                            case "Published": published = ToDate(dr); break;
                            case "LastStatus": lastStatus = ToDate(dr); break;
                            case "ExitAddress":
                                {
                                    ix = dr.IndexOf(' ');
                                    if (ix == -1) continue;

                                    iz = dr.Substring(0, ix);
                                    dr = dr.Remove(0, ix + 1);

                                    if (!IPAddress.TryParse(iz, out exitAddress)) continue;
                                    exitAddressDate = ToDate(dr);

                                    ls.Add(new ExitNode(exitNodeId, published, lastStatus, exitAddressDate, exitAddress));
                                    break;
                                }
                        }
                    }
                }
            }
            catch { }

            _ExitNodes = ls.ToArray();
            return _ExitNodes.Length;
        }
        /// <summary>
        /// Comprueba si es un nodo de Tor por la lista cacheada
        /// </summary>
        /// <param name="ip">Ip</param>
        /// <returns>Devuelve si es un nodo de salida Tor</returns>
        public static bool IsTorExitNode(IPAddress ip)
        {
            if (ip == null) return false;

            string sip = ip.ToString();
            foreach (ExitNode ex in ExitNodes)
                if (ex.ExitAddressString == sip) return true;

            return false;
        }
        static DateTime ToDate(string dr) { return DateTime.ParseExact(dr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
    }
}