namespace XPloit.Server.Http
{
    public class HttpPath
    {
        string _Path = "";
        string _Name = "";

        public string Path { get { return _Path; } }
        public string Name { get { return _Name; } }
        public HttpPath(string path)
        {
            int ix = path.LastIndexOf('/');
            if (ix == -1)
            {
                _Path = "";
                _Name = path;
            }
            else
            {
                _Path = path.Substring(0, ix);
                _Name = path.Substring(ix + 1);
            }
        }
        public override string ToString() { return "Path: '" + _Path + "' - Name: '" + _Name + "'"; }
    }
}