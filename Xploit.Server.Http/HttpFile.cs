using System.IO;

namespace Xploit.Server.Http
{
    public class HttpFile
    {
        long _lgt = 0;
        string _fileName, _fileTemp, _contentType, _title;

        public string Name { get { return _title; } }
        public string FileName { get { return _fileName; } }
        public string FileTemp { get { return _fileTemp; } }
        public string ContentType { get { return _contentType; } }
        public long Length { get { return _lgt; } }

        public HttpFile(string name, string file_org, string contentType, string file_tmp)
        {
            _contentType = contentType;
            _fileName = file_org;
            _title = name;

            _fileTemp = file_tmp;
        }
        public override string ToString() { return _title + " -> " + _fileTemp; }
        public byte[] GetFileData() { return File.ReadAllBytes(_fileTemp); }
        internal void UpdateLength(long lg) { _lgt = lg; }
    }
}