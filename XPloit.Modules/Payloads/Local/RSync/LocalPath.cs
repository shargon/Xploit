using System.Collections.Generic;
using System.IO;
using XPloit.Core;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.RSync
{
    public class LocalPath : Payload, Auxiliary.Local.RSync.ISync
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Sync local path"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Local path as remote")]
        public DirectoryInfo RemotePath { get; set; }
        #endregion

        public bool AllowMd5Hash { get { return true; } }

        public string PurgePath(string path)
        {
            // Ftp from linux replacement
            if (SystemHelper.IsWindows)
            {
                // /test/test:file.txt ->  fail at windows
                if (path.Length > 3)
                {
                    string d = path.Substring(0, 2);
                    path = d + path.Substring(2).Replace(":", "_");
                }
            }
            return path;
        }

        public void DeleteFolder(Auxiliary.Local.RSync.SyncFolder p)
        {
            string path = PurgePath(p.GetFullPath(Path.DirectorySeparatorChar));

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        public void CreateFolder(Auxiliary.Local.RSync.SyncFolder p)
        {
            string path = PurgePath(p.GetFullPath(Path.DirectorySeparatorChar));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public Stream GetFile(Auxiliary.Local.RSync.SyncFile p)
        {
            string path = PurgePath(p.GetFullPath(Path.DirectorySeparatorChar));
            return File.OpenRead(path);
        }
        public string GetMd5(Auxiliary.Local.RSync.SyncFile p)
        {
            string path = PurgePath(p.GetFullPath(Path.DirectorySeparatorChar));
            return HashHelper.HashHexFile(HashHelper.EHashType.Md5, path);
        }
        public void DeleteFile(Auxiliary.Local.RSync.SyncFile p)
        {
            string path = PurgePath(p.GetFullPath(Path.DirectorySeparatorChar));
            if (File.Exists(path))
                File.Delete(path);
        }
        public void WriteFile(Auxiliary.Local.RSync.SyncFile p, Stream stream)
        {
            string path = PurgePath(p.GetFullPath(Path.DirectorySeparatorChar));
            using (FileStream fs = File.OpenWrite(path))
            {
                stream.CopyTo(fs);
            }
        }
        void FillDir(Auxiliary.Local.RSync.SyncFolder ret)
        {
            string path = PurgePath(ret.GetFullPath(Path.DirectorySeparatorChar));

            List<Auxiliary.Local.RSync> ls = new List<Auxiliary.Local.RSync>();
            foreach (string file in Directory.GetFiles(path))
            {
                ret.Files.Add(new Auxiliary.Local.RSync.SyncFile(ret)
                {
                    Name = Path.GetFileName(file),
                    Length = new FileInfo(file).Length,
                });
            }
            foreach (string dir in Directory.GetDirectories(path))
            {
                Auxiliary.Local.RSync.SyncFolder ret2 = new Auxiliary.Local.RSync.SyncFolder(ret)
                {
                    Name = Path.GetFileName(dir),
                };
                FillDir(ret2);
                ret.Folders.Add(ret2);
            }
        }

        public Auxiliary.Local.RSync.SyncFolder GetTree()
        {
            Auxiliary.Local.RSync.SyncFolder ret = new Auxiliary.Local.RSync.SyncFolder(null) { Name = RemotePath.FullName };
            FillDir(ret);

            return ret;
        }
    }
}