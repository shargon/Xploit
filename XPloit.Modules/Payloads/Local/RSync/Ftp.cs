using System.Collections.Generic;
using System.IO;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.RSync
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Ftp rsync")]
    public class Ftp : Payload, Auxiliary.Local.RSync.ISync
    {
        char SeparatorChar = '/';

        #region Properties
        [ConfigurableProperty(Description = "FTP Server")]
        public string Server { get; set; }
        [ConfigurableProperty(Description = "FTP Path")]
        public string Path { get; set; }
        [ConfigurableProperty(Description = "FTP User")]
        public string User { get; set; }
        [ConfigurableProperty(Description = "FTP Password")]
        public string Password { get; set; }
        [ConfigurableProperty(Description = "FTP Port")]
        public ushort Port { get; set; }
        [ConfigurableProperty(Description = "Use ssl")]
        public bool SSL { get; set; }
        #endregion

        public bool AllowMd5Hash { get { return false; } }

        public Ftp()
        {
            Port = 21;
            Path = "/";
            SSL = false;
        }

        public FtpHelper GetFtp(string path = "")
        {
            return new FtpHelper(new FtpHelper.Cfg(Server, path, User, Password, Port, SSL));
        }

        public void DeleteFolder(Auxiliary.Local.RSync.SyncFolder p)
        {
            string path = p.GetFullPath(SeparatorChar);
            GetFtp().DeleteFile(path);
        }
        public void CreateFolder(Auxiliary.Local.RSync.SyncFolder p)
        {
            string path = p.GetFullPath(SeparatorChar);
            GetFtp().CreateDirectory(path);
        }
        public Stream GetFile(Auxiliary.Local.RSync.SyncFile p)
        {
            string path = p.GetFullPath(SeparatorChar);

            MemoryStream stream = new MemoryStream();
            GetFtp().Download(path, stream, null, null);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        public string GetMd5(Auxiliary.Local.RSync.SyncFile p)
        {
            using (Stream ms = GetFile(p))
                return HashHelper.HashHex(HashHelper.EHashType.Md5, ms, true);
        }
        public void DeleteFile(Auxiliary.Local.RSync.SyncFile p)
        {
            string path = p.GetFullPath(SeparatorChar);
            GetFtp().DeleteFile(path);
        }
        public void WriteFile(Auxiliary.Local.RSync.SyncFile p, Stream stream)
        {
            string path = p.GetFullPath(SeparatorChar);
            GetFtp().Upload(stream, path, null, null);
        }
        void FillDir(Auxiliary.Local.RSync.SyncFolder ret)
        {
            string path = ret.GetFullPath(SeparatorChar);

            FtpHelper ftp = GetFtp(path);

            List<Auxiliary.Local.RSync> ls = new List<Auxiliary.Local.RSync>();
            foreach (FtpHelper.Item item in ftp.ListDirectoryExtended())
            {
                if (item.IsDirectory)
                {
                    Auxiliary.Local.RSync.SyncFolder ret2 = new Auxiliary.Local.RSync.SyncFolder(ret)
                    {
                        Name = item.Name,
                    };
                    FillDir(ret2);
                    ret.Folders.Add(ret2);
                }
                else
                {
                    ret.Files.Add(new Auxiliary.Local.RSync.SyncFile(ret)
                    {
                        Name = item.Name,
                        Length = item.Length,
                    });
                }
            }
        }

        public Auxiliary.Local.RSync.SyncFolder GetTree()
        {
            Auxiliary.Local.RSync.SyncFolder ret = new Auxiliary.Local.RSync.SyncFolder(null)
            {
                Name = Path,
            };

            FillDir(ret);

            return ret;
        }
    }
}