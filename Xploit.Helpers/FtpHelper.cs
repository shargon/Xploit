using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using XPloit.Helpers.Interfaces;

namespace XPloit.Helpers
{
    public class FtpHelper
    {
        public class Item
        {
            public enum ListStyle
            {
                Unix,
                Windows,
                Unknown
            }

            public ListStyle Style { get; set; }
            public bool IsDirectory { get; set; }
            public string Name { get; set; }
            public long Length { get; set; }

            public Item(string line)
            {
                Style = ListStyle.Unknown;

                Regex fileZilla = new Regex(
                   @"(?<dir>[-dl])(?<ownerSec>)[-r][-w]-x[-r][-w]-x[-r][-w][-x]s+(?:d)s+(?<owner>w+)s+(?<group>w+)s+(?<size>d+)s+(?<month>w+)s+(?<day>d{1,2})s+(?<year>w+)s+(?<name>.*)$");

                Match match = fileZilla.Match(line);
                if (match.Success) ParseMatch(match.Groups, ListStyle.Unix);
                else
                {
                    Regex unixStyle = new Regex(@"^(?<dir>[\-ld])(?<permission>([\-r][\-w][\-xs]){3})\s+(?<filecode>\d+)\s+(?<owner>\w+)\s+(?<group>\w+)\s+(?<size>\d+)\s+(?<timestamp>((?<month>\w{3})\s+(?<day>\d{1,2})\s+(?<hour>\d{1,2}):(?<minute>\d{2}))|((?<month>\w{3})\s+(?<day>\d{1,2})\s+(?<year>\d{4})))\s+(?<name>.+)$");
                    //Regex unixStyle = new Regex(@"^(?<dir>[-dl])(?<ownerSec>[-r][-w][-x])(?<groupSec>[-r][-w][-x])(?<everyoneSec>[-r][-w][-x])s+(?:d)s+(?<owner>w+)s+(?<group>w+)s+(?<size>d+)s+(?<month>w+)s+(?<day>d{1,2})s+(?<hour>d{1,2}):(?<minutes>d{1,2})s+(?<name>.*)$");
                    match = unixStyle.Match(line);
                    if (match.Success) ParseMatch(match.Groups, ListStyle.Unix);
                }
            }
            void ParseMatch(GroupCollection matchGroups, ListStyle style)
            {
                string dirMatch = (style == ListStyle.Unix ? "d" : "dir");

                Style = style;
                IsDirectory = matchGroups["dir"].Value.Equals(dirMatch, StringComparison.InvariantCultureIgnoreCase);
                Name = matchGroups["name"].Value;

                if (!IsDirectory)
                    Length = long.Parse(matchGroups["size"].Value);
            }
        }
        public class Cfg
        {
            /// <summary>
            /// FTP-Server
            /// </summary>
            public string Server { get; set; }
            /// <summary>
            /// FTP-Path
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// FTP-User
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// FTP-Password
            /// </summary>
            public string Password { get; set; }
            /// <summary>
            /// FTP-Port
            /// </summary>
            public ushort Port { get; set; }
            /// <summary>
            /// FTP-Retries
            /// </summary>
            public ushort Retries { get; set; }
            /// <summary>
            /// FTP in SSL ?
            /// </summary>
            public bool SSL { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public Cfg()
            {
                Server = ""; Path = ""; User = ""; Password = "";
                Port = 21; Retries = 1;
                SSL = false;
            }
            /// <summary>
            /// Copy from other config
            /// </summary>
            /// <param name="cfg">Config</param>
            public Cfg(FtpHelper.Cfg cfg) : this(cfg, cfg != null ? cfg.Path : null) { }
            /// <summary>
            /// Copy from other config but change the remotePath
            /// </summary>
            /// <param name="cfg">Config</param>
            /// <param name="remotePath">RemotePath</param>
            public Cfg(FtpHelper.Cfg cfg, string remotePath)
                : this()
            {
                if (cfg != null)
                {
                    Server = cfg.Server; // Path = cfg.Path;
                    User = cfg.User; Password = cfg.Password;
                    Port = cfg.Port; SSL = cfg.SSL;
                }
                Path = remotePath;
            }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="server">Server</param>
            /// <param name="remotePath">RemotePath</param>
            /// <param name="user">User</param>
            /// <param name="passw">Password</param>
            /// <param name="port">Port</param>
            /// <param name="ssl">True if use SSL</param>
            public Cfg(string server, string remotePath, string user, string passw, ushort port, bool ssl)
            {
                if (server == null) server = "";
                if (remotePath == null) remotePath = "";
                if (user == null) user = "";
                if (passw == null) passw = "";

                Server = server; Path = remotePath;
                User = user; Password = passw;
                Port = port; SSL = ssl;
            }
        }

        #region Variables
        int _BufferLength = 2048;
        Cfg _Config;
        bool _BinaryMode = true;
        TimeSpan _DownloadTimeOut = TimeSpan.Zero;
        TimeSpan _UploadTimeOut = TimeSpan.Zero;
        #endregion

        #region Properties
        /// <summary>
        /// Buffer Length
        /// </summary>
        public int BufferLength { get { return _BufferLength; } set { _BufferLength = value; } }
        /// <summary>
        /// Set Timeout for Download (Zero == No TimeOut)
        /// </summary>
        public TimeSpan DownloadTimeOut { get { return _DownloadTimeOut; } set { _DownloadTimeOut = value; } }
        /// <summary>
        /// Set Timeout for Upload (Zero == No TimeOut)
        /// </summary>
        public TimeSpan UploadTimeOut { get { return _UploadTimeOut; } set { _UploadTimeOut = value; } }
        /// <summary>
        /// Configuration
        /// </summary>
        public Cfg Config { get { return _Config; } set { _Config = value; } }
        /// <summary>
        /// Use BinaryMode
        /// </summary>
        public bool BinaryMode { get { return _BinaryMode; } set { _BinaryMode = value; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cfg">Configuration</param>
        public FtpHelper(Cfg cfg) { _Config = cfg; }

        #region GetRequest
        FtpWebRequest GetRequest(string command) { return GetRequest(_Config.Path, "", command); }
        FtpWebRequest GetRequest(string file, string command) { return GetRequest(_Config.Path, file, command); }
        FtpWebRequest GetRequest(string path, string file, string command)
        {
            string dir = "ftp://" + (_Config.Server + ":" + _Config.Port.ToString() +
                "/" + path + (!string.IsNullOrEmpty(file) ? "/" + file : "")).Replace("//", "/");

            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(dir);

            ftp.KeepAlive = false;
            ftp.Timeout = 30000;
            //ftp.Timeout=;

            ftp.Credentials = new System.Net.NetworkCredential(_Config.User, _Config.Password);
            //ftp.UsePassive = false;

            if (_Config.SSL) ftp.EnableSsl = true;
            ftp.Method = command;
            return ftp;
        }
        #endregion

        /// <summary>
        /// Login Method (Throw exception if fail)
        /// </summary>
        public void Login()
        {
            Exception ex = null;
            Login(out ex);
            if (ex != null) throw (ex);
        }
        /// <summary>
        /// Login Method (return false if fail)
        /// </summary>
        /// <param name="ex">Exception if fail</param>
        public bool Login(out Exception ex)
        {
            ex = null;

            FtpWebRequest ftp = GetRequest("", "", WebRequestMethods.Ftp.PrintWorkingDirectory);
            FtpWebResponse response = null;

            try { response = (FtpWebResponse)ftp.GetResponse(); }
            catch (Exception e) { ex = e; }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }
            ftp.Abort();

            return ex == null;
        }
        /// <summary>
        /// Get the file Length, return -1 if fail
        /// </summary>
        /// <param name="file">Remote file</param>
        public long GetFileSize(string file)
        {
            long l = -1;
            // Try Get filesize
            try
            {
                FtpWebRequest ftp = GetRequest(file, WebRequestMethods.Ftp.GetFileSize);
                ftp.UseBinary = true;

                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                {
                    l = response.ContentLength;
                    response.Close();
                }
                ftp.Abort();
                return l;
            }
            catch { }

            // Try download and cancel file
            try
            {
                FtpWebRequest ftp = GetRequest(file, WebRequestMethods.Ftp.DownloadFile);
                ftp.UseBinary = true;

                string s;
                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                {
                    //BinaryReader sr = new BinaryReader(response.GetResponseStream());
                    s = response.StatusDescription;
                    try { response.Close(); }
                    catch { }
                }
                try { ftp.Abort(); }
                catch { }

                // Search in the description
                int i = s.IndexOf(" bytes");
                if (i == -1) return l;
                s = s.Remove(i, s.Length - i);
                i = s.LastIndexOfAny(new char[] { ' ', '(' });
                if (i == -1) return l;
                s = s.Remove(0, i + 1);
                if (long.TryParse(s, out l)) return l;

                return 0;
            }
            catch { }

            return l;
        }
        /// <summary>
        /// Get the list of entries in current Directory
        /// </summary>
        public List<string> ListDirectory()
        {
            string s;

            FtpWebRequest ftp = GetRequest(WebRequestMethods.Ftp.ListDirectory);
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                s = sr.ReadToEnd();
                sr.Close();
                response.Close();
            }

            ftp.Abort();

            List<string> files = new List<string>();
            foreach (string fl in s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (fl == "." || fl == "..") continue;

                files.Add(Path.GetFileName(fl));
            }

            return files;
        }
        /// <summary>
        /// Get the list of entries in current Directory
        /// </summary>
        public List<Item> ListDirectoryExtended()
        {
            string s;

            FtpWebRequest ftp = GetRequest(WebRequestMethods.Ftp.ListDirectoryDetails);
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                s = sr.ReadToEnd();
                sr.Close();
                response.Close();
            }

            ftp.Abort();

            List<Item> files = new List<Item>();
            foreach (string fl in s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Item it = new Item(fl);
                if (it.Style == Item.ListStyle.Unknown)
                    continue;
                if (it.IsDirectory && (it.Name == "." || it.Name == ".."))
                    continue;

                files.Add(it);
            }

            return files;
        }
        /// <summary>
        /// Download
        /// </summary>
        /// <param name="remoteFile">Remote file</param>
        /// <param name="localFile">Local file</param>
        /// <param name="prog">Progress</param>
        /// <param name="cancel">Cancel</param>
        public void Download(string remoteFile, string localFile, IProgress prog, CancelEventArgs cancel)
        {
            Exception ex = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                Download(remoteFile, fs, prog, cancel);
            }
            catch (Exception e)
            {
                ex = e;
            }
            finally
            {
                if (fs != null) { fs.Close(); fs.Dispose(); }
            }
            // Throw exception
            if (ex != null)
            {
                if (File.Exists(localFile)) File.Delete(localFile);
                throw (ex);
            }
        }
        /// <summary>
        /// Download
        /// </summary>
        /// <param name="remoteFile">Remote file</param>
        /// <param name="stream">Stream</param>
        /// <param name="prog">Progress</param>
        /// <param name="cancel">Cancel</param>
        public void Download(string remoteFile, Stream stream, IProgress prog, CancelEventArgs cancel)
        {
            if (prog != null) prog.StartProgress(GetFileSize(remoteFile));

            FtpWebRequest ftp = null;
            try
            {
                ftp = GetRequest(remoteFile, WebRequestMethods.Ftp.DownloadFile);
                ftp.UseBinary = true;
                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                using (BinaryReader sr = new BinaryReader(response.GetResponseStream()))
                {
                    int lee = 0;
                    long lleva = 0;
                    byte[] bff = new byte[BufferLength];
                    DateTime start = DateTime.Now;
                    bool bts = _DownloadTimeOut != TimeSpan.Zero;

                    do
                    {
                        lee = sr.Read(bff, 0, bff.Length);
                        if (lee == 0) break;
                        lleva += lee;
                        stream.Write(bff, 0, lee);

                        if (prog != null)
                            prog.WriteProgress(lleva);

                        if (cancel != null && cancel.Cancel)
                        {
                            sr.Close();
                            response.Close();
                            ftp.Abort();

                            return;
                        }

                        if (bts && DateTime.Now - start > _DownloadTimeOut)
                        {
                            sr.Close();
                            response.Close();
                            ftp.Abort();

                            TimeSpan s = DateTime.Now - start;
                            throw (new Exception("TIMEOUT EXCEDED ( AVG " + StringHelper.Convert2Kb(lleva / s.TotalSeconds) + "/sec )"));
                        }
                    } while (lee > 0);

                    sr.Close();
                    response.Close();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                if (prog != null) prog.EndProgress();
                if (ftp != null) ftp.Abort();
            }
        }
        /// <summary>
        /// Upload
        /// </summary>
        /// <param name="localFile">Local file</param>
        /// <param name="remoteFile">Remote file</param>
        /// <param name="prog">Progress</param>
        /// <param name="cancel">Cancel</param>
        public void Upload(string localFile, string remoteFile, IProgress prog, CancelEventArgs cancel)
        {
            Exception ex = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(localFile, FileMode.Open, FileAccess.Read);
                Upload(fs, remoteFile, prog, cancel);
            }
            catch (Exception e) { ex = e; }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            if (ex != null) throw (ex);
        }
        /// <summary>
        /// Upload
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="remoteFile">Remote file</param>
        /// <param name="prog">Progress</param>
        /// <param name="cancel">Cancel</param>
        public void Upload(Stream stream, string remoteFile, IProgress prog, CancelEventArgs cancel)
        {
            long mx = stream.Length;
            if (prog != null) prog.StartProgress(mx);

            FtpWebRequest ftp = null;
            try
            {
                ftp = GetRequest(remoteFile, WebRequestMethods.Ftp.UploadFile);

                ftp.UseBinary = true;
                ftp.ContentLength = mx;

                using (Stream sw = ftp.GetRequestStream())
                {
                    int lee = 0;
                    long lleva = 0;
                    byte[] bff = new byte[BufferLength];
                    DateTime start = DateTime.Now;
                    bool bts = _UploadTimeOut != TimeSpan.Zero;

                    do
                    {
                        lee = stream.Read(bff, 0, bff.Length);
                        if (lee <= 0) break;
                        lleva += lee;
                        sw.Write(bff, 0, lee);

                        if (prog != null)
                            prog.WriteProgress(lleva);

                        if (cancel != null && cancel.Cancel)
                        {
                            sw.Close();
                            ftp.Abort();

                            DeleteFile(remoteFile);
                        }

                        if (bts && DateTime.Now - start > _UploadTimeOut)
                        {
                            sw.Close();
                            ftp.Abort();

                            TimeSpan s = DateTime.Now - start;
                            throw (new Exception("TIMEOUT EXCEDED ( AVG " +
                                StringHelper.Convert2Kb(lleva / s.TotalSeconds) + "/sec )"));
                        }
                    } while (lee > 0);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                if (prog != null) prog.EndProgress();
                if (ftp != null) ftp.Abort();
            }

            long mxhay = GetFileSize(remoteFile);
            if (mxhay != 0) { if (mx != mxhay) throw (new Exception("Length error")); }
        }
        /// <summary>
        /// Create a directory
        /// </summary>
        /// <param name="dirName">Directory name</param>
        public bool CreateDirectory(string dirName)
        {
            bool dv = false;
            FtpWebRequest ftp = GetRequest(dirName, WebRequestMethods.Ftp.MakeDirectory);
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)ftp.GetResponse();
                dv = response.StatusCode == FtpStatusCode.PathnameCreated;
            }
            catch { return false; }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }
            ftp.Abort();
            return dv;
        }
        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="file">File Name</param>
        public bool DeleteFile(string file)
        {
            bool dv = false;
            FtpWebRequest ftp = GetRequest(file, WebRequestMethods.Ftp.DeleteFile);
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)ftp.GetResponse();
                dv = response.StatusCode == FtpStatusCode.FileActionOK;
            }
            catch { return false; }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }
            ftp.Abort();
            return dv;
        }
        /// <summary>
        /// Extract File Name from details
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="extension">Extension</param>
        /// <param name="useCaseSensitive">Use can sensitive</param>
        /// <param name="returnExtension">return extension</param>
        public string ExtractFileNameFromDetails(string line, string extension, bool useCaseSensitive, bool returnExtension)
        {
            int ix = -1;
            if (useCaseSensitive) ix = line.IndexOf(extension);
            else ix = line.IndexOf(extension, StringComparison.InvariantCultureIgnoreCase);

            if (ix == -1) return null;

            string name = line.Substring(0, ix + extension.Length);
            ix = name.LastIndexOf(' ');
            if (ix != -1)
            {
                name = name.Remove(0, ix + 1);
            }

            if (!returnExtension) name = Path.GetFileNameWithoutExtension(name);
            return name;
        }
    }
}