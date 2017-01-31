using System;

namespace XPloit.Server.Http
{
    public class HttpCache
    {
        bool _InGZip = false;
        string _etag = null;
        string _ctyp = null;
        string[] _headers = null;
        byte[] _data = null;
        DateTime _date = DateTime.Now;

        public string ETag
        {
            get
            {
                if (_etag == null) _etag = HttpUtilityEx.Md5(_data);
                return _etag;
            }
        }
        public string ContentType { get { return _ctyp; } }
        public string[] Headers { get { return _headers; } }
        public byte[] Data { get { return _data; } }
        public DateTime DateTime { get { return _date; } }
        public int Length { get { return _data == null ? 0 : _data.Length; } }
        public bool InGZip { get { return _InGZip; } }

        public HttpCache(string[] headers, string content_type, byte[] data, bool ingzip)
        {
            _headers = headers;
            _ctyp = content_type;
            _data = data;
            _InGZip = ingzip;
        }
        public HttpCache(string[] headers, string content_type, byte[] data, bool ingzip, string etag)
        {
            _headers = headers;
            _ctyp = content_type;
            _data = data;
            _etag = etag;
            _InGZip = ingzip;
        }
        public static int SortByDate(HttpCache a, HttpCache b) { return a._date.CompareTo(b._date); }
    }
}