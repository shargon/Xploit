using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using XPloit.Helpers.Interfaces;

namespace XPloit.Helpers
{
    public class HttpHelper
    {
        public class Response
        {
            Dictionary<string, string> _Headers = new Dictionary<string, string>();
            string _ContentType;
            Encoding _Encoding = Encoding.UTF8;

            /// <summary>
            /// All is Ok
            /// </summary>
            public bool IsOk { get; set; }
            /// <summary>
            /// Http ContentType
            /// </summary>
            public string ContentType
            {
                get { return _ContentType; }
                set
                {
                    _ContentType = value;

                    if (!string.IsNullOrEmpty(value))
                        foreach (string sep in value.Split(';'))
                        {
                            if (!sep.StartsWith("charset=", StringComparison.InvariantCultureIgnoreCase)) continue;

                            try
                            {
                                string ch = sep.Substring(8);
                                Encoding codec = Encoding.GetEncoding(ch);
                                if (codec != null) _Encoding = codec;
                            }
                            catch { }
                        }
                }
            }
            /// <summary>
            /// Size of response
            /// </summary>
            public long ContentLength { get; set; }
            /// <summary>
            /// Response from cache
            /// </summary>
            public bool IsFromCache { get; set; }
            /// <summary>
            /// Response Url
            /// </summary>
            public Uri ResponseUri { get; set; }
            /// <summary>
            /// Encoding
            /// </summary>
            public Encoding Encoding { get { return _Encoding; } set { _Encoding = value; } }
            /// <summary>
            /// Headers
            /// </summary>
            public Dictionary<string, string> Headers { get { return _Headers; } }
            /// <summary>
            /// Copy class from WebResponse
            /// </summary>
            /// <param name="response">Response</param>
            internal void CopyFrom(WebResponse response)
            {
                ContentLength = response.ContentLength;
                ContentType = response.ContentType;
                IsFromCache = response.IsFromCache;
                ResponseUri = response.ResponseUri;

                foreach (string k in response.Headers.Keys)
                {
                    if (_Headers.ContainsKey(k)) continue;

                    _Headers.Add(k, response.Headers[k]);
                }
            }
        }
        /// <summary>
        /// Download url to stream
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="stream">Stream</param>
        /// <param name="progress">Progreso</param>
        public static Response Download(string url, Stream stream, IProgress progress)
        {
            Response res = new Response();

            try
            {
                HttpWebRequest http = (HttpWebRequest)WebRequest.Create(url);
                using (WebResponse response = http.GetResponse())
                {
                    Stream st = response.GetResponseStream();
                    res.CopyFrom(response);

                    if (progress != null)
                    {
                        if (res.ContentLength > 0)
                        {
                            progress.StartProgress(res.ContentLength);
                        }
                        else progress = null;
                    }

                    // TODO: Gzip compression

                    if (progress == null)
                        st.CopyTo(stream);
                    else
                    {
                        for (long x = 0, m = res.ContentLength; x < m;)
                        {
                            byte[] buffer = new byte[1024];
                            int lee = st.Read(buffer, 0, buffer.Length);
                            if (lee == 0) break;

                            stream.Write(buffer, 0, lee);

                            if (progress != null)
                                progress.WriteProgress(x + lee);

                            x += lee;
                        }
                    }
                }
                res.IsOk = true;
            }
            catch
            {
                res.IsOk = false;
            }
            finally
            {
                if (progress != null)
                    progress.EndProgress();
            }
            return res;
        }
        /// <summary>
        /// Get length of
        /// </summary>
        /// <param name="url">Url</param>
        public static Response GetHeadResponse(string url)
        {
            Response res = new Response();
            try
            {
                HttpWebRequest http = (HttpWebRequest)WebRequest.Create(url);
                http.Method = "HEAD";
                using (WebResponse response = http.GetResponse())
                {
                    res.CopyFrom(response);
                }
                res.IsOk = true;
            }
            catch
            {
                res.IsOk = false;
            }
            return res;
        }
        /// <summary>
        /// Download url to string
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="progress">Progreso</param>
        public static string DownloadString(string url, IProgress progress)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Response ret = Download(url, ms, progress);
                if (!ret.IsOk) return null;

                byte[] data = ms.ToArray();
                return ret.Encoding.GetString(data);
            }
        }
    }
}