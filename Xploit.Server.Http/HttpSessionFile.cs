using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xploit.Server.Http.Interfaces;

namespace Xploit.Server.Http
{
    public class HttpSessionFile : IHttpSession
    {
        string _file = null;
        string _id;
        static Encoding codec = Encoding.UTF8;
        Dictionary<string, string> vars = new Dictionary<string, string>();

        public Dictionary<string, string> Variables { get { return vars; } }
        public string SessionID { get { return _id; } }

        public string this[string name]
        {
            get
            {
                string dv = null;
                if (vars.TryGetValue(name, out dv)) return dv;
                return null;
            }
            set
            {
                if (value == null) { vars.Remove(name); return; }
                if (vars.ContainsKey(name)) vars[name] = value;
                else vars.Add(name, value);
            }
        }

        public HttpSessionFile(string directory, string session_id)
        {
            _id = session_id;
            _file = directory + "\\" + session_id;
            FileStream fs = null;
            try { fs = new FileStream(_file, FileMode.Open, FileAccess.Read); }
            catch (FileNotFoundException) { return; }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    fs = new FileStream(_file, FileMode.Open, FileAccess.Read);
                }
                catch { }
            }
            if (fs != null)
            {
                try
                {
                    byte[] bsi = new byte[4], bs = null;
                    int le = 0;
                    while ((le = fs.ReadByte()) != -1)
                    {
                        bs = new byte[le];
                        if (fs.Read(bs, 0, le) != le) break;
                        string name = codec.GetString(bs);

                        if (fs.Read(bsi, 0, 4) != 4) break;
                        le = BitConverter.ToInt32(bsi, 0);
                        bs = new byte[le];

                        if (fs.Read(bs, 0, le) != le) break;
                        string val = codec.GetString(bs);

                        vars.Add(name, val);
                    }
                }
                catch { }
                finally { fs.Close(); fs.Dispose(); File.SetLastAccessTime(_file, DateTime.Now); }
            }
        }

        public void Save()
        {
            FileStream fs = null;
            try
            {
                byte[] bsi = null, dv = null;
                fs = new FileStream(_file, FileMode.Create, FileAccess.Write);

                foreach (string k in vars.Keys)
                {
                    int l = k.Length;
                    fs.WriteByte((byte)l);
                    dv = codec.GetBytes(k);
                    fs.Write(dv, 0, dv.Length);

                    dv = codec.GetBytes(vars[k]);
                    bsi = BitConverter.GetBytes(dv.Length);
                    fs.Write(bsi, 0, bsi.Length);
                    fs.Write(dv, 0, dv.Length);
                }
            }
            catch { }
            finally { if (fs != null) { fs.Close(); fs.Dispose(); File.SetLastAccessTime(_file, DateTime.Now); } }
        }
        public void Clear() { vars.Clear(); }
    }
}