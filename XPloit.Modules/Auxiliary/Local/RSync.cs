using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;

namespace Auxiliary.Local
{
    public class RSync : Module
    {
        public class SyncFolder
        {
            public SyncFolder Parent { get; internal set; }
            public string Name { get; set; }
            public List<SyncFolder> Folders { get; private set; }
            public List<SyncFile> Files { get; private set; }
            internal bool Checked { get; set; }

            public override string ToString() { return Name; }

            public SyncFolder(SyncFolder parent)
            {
                Parent = parent;
                Folders = new List<SyncFolder>();
                Files = new List<SyncFile>();
            }
            public string GetFullPath(char separator)
            {
                string s = Name;

                RSync.SyncFolder p = Parent;
                while (p != null)
                {
                    s = p.Name + separator + s;
                    p = p.Parent;
                }

                return s;
            }
        }
        public class SyncFile
        {
            public SyncFolder Parent { get; internal set; }
            public string Name { get; set; }
            public long Length { get; set; }
            internal bool Checked { get; set; }

            public override string ToString() { return Name; }

            public SyncFile(SyncFolder parent)
            {
                Parent = parent;
            }

            public string GetFullPath(char separator)
            {
                string s = Parent.GetFullPath(separator);
                s += separator;
                s += Name;

                return s;
            }
        }

        public interface ISync
        {
            bool AllowMd5Hash { get; }

            SyncFolder GetTree();

            string GetMd5(SyncFile file);
            Stream GetFile(SyncFile file);

            void DeleteFile(SyncFile file);
            void WriteFile(SyncFile file, Stream stream);

            void DeleteFolder(SyncFolder path);
            void CreateFolder(SyncFolder path);
        }

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Remote sync for folders"; } }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(ISync)); } }
        #endregion

        public enum EMode { Upload, Download }

        #region Properties
        [ConfigurableProperty(Description = "Local folder for sync")]
        public DirectoryInfo LocalPath { get; set; }
        [ConfigurableProperty(Description = "Filter for files")]
        public string FilterFile { get; set; }
        [ConfigurableProperty(Description = "Filter for folders")]
        public string FilterFolder { get; set; }

        [ConfigurableProperty(Description = "Use md5 for compare")]
        public bool UseMd5 { get; set; }
        [ConfigurableProperty(Description = "Delete extra files")]
        public bool DeleteExtra { get; set; }

        [ConfigurableProperty(Optional = true, Description = "Upload (local to payload), Download (payload to local)")]
        public EMode Mode { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public RSync()
        {
            // Default variables
            FilterFile = "*";
            FilterFolder = "*";

            Mode = EMode.Download;
        }

        public override bool Run()
        {
            ISync local;
            ISync remote = (ISync)Payload;

            if (Mode == EMode.Upload)
            {
                local = remote;
                remote = new Payloads.Local.RSync.LocalPath() { RemotePath = LocalPath };
            }
            else
            {
                local = new Payloads.Local.RSync.LocalPath() { RemotePath = LocalPath };
            }

            WriteInfo("Fetching ...");

            Task<SyncFolder> taskA = new Task<SyncFolder>(() => local.GetTree());
            Task<SyncFolder> taskB = new Task<SyncFolder>(() => remote.GetTree());

            taskA.Start(); taskB.Start();
            taskA.Wait(); taskB.Wait();

            SyncFolder lfolder = taskA.Result;
            SyncFolder rfolder = taskB.Result;

            int files = 0, folders = 0;
            Count(rfolder, ref files, ref folders);

            WriteInfo("Total folders", files.ToString(), ConsoleColor.Green);
            WriteInfo("Total files", folders.ToString(), ConsoleColor.Green);

            WriteInfo("Checking write ...");
            Write(local, lfolder, remote, rfolder, FilterFolder, FilterFile, UseMd5);

            if (DeleteExtra)
            {
                WriteInfo("Checking delete ...");
                Delete(local, lfolder, FilterFolder, FilterFile);
            }

            return true;
        }

        void Count(SyncFolder lfolder, ref int files, ref int folders)
        {
            foreach (SyncFolder f in lfolder.Folders)
            {
                folders++;
                Count(f, ref files, ref folders);
            }

            files += lfolder.Files.Count;
        }
        void Delete(ISync local, SyncFolder lfolder, string filterFolder, string filterFile)
        {
            if (lfolder == null) return;

            // Folder
            foreach (SyncFolder f in lfolder.Folders)
            {
                if (!StringHelper.Like(filterFolder, f.Name))
                    continue;

                if (f.Checked)
                {
                    // Esta, hacerlo recursivo
                    Delete(local, f, filterFolder, filterFile);
                }
                else
                {
                    try
                    {
                        local.DeleteFolder(f);
                        WriteInfo("Deleting folder: " + f.GetFullPath('/'));
                    }
                    catch (Exception e)
                    {
                        WriteError(e.Message);
                    }
                }
            }

            // Files
            foreach (SyncFile f in lfolder.Files)
            {
                if (f.Checked || !StringHelper.Like(filterFile, f.Name))
                    continue;

                try
                {
                    local.DeleteFile(f);
                    WriteInfo("Deleting file: " + f.GetFullPath('/'));
                }
                catch (Exception e)
                {
                    WriteError(e.Message);
                }
            }
        }

        void Write(ISync local, SyncFolder lfolder, ISync remote, SyncFolder rfolder, string filterFolder, string filterFile, bool useMd5)
        {
            if (rfolder == null || lfolder == null) return;

            // Folder
            foreach (SyncFolder f in rfolder.Folders)
            {
                SyncFolder esta = null;
                foreach (SyncFolder f2 in lfolder.Folders)
                {
                    if (f2.Name == f.Name)
                    {
                        f2.Checked = true;
                        esta = f2;
                        break;
                    }
                }

                if (!StringHelper.Like(filterFile, f.Name))
                    continue;

                try
                {
                    if (esta == null)
                    {
                        esta = new SyncFolder(lfolder) { Name = f.Name };
                        local.CreateFolder(esta);
                        WriteInfo("Creating folder: " + esta.GetFullPath('/'));
                    }

                    // Esta, hacerlo recursivo
                    Write(local, esta, remote, f, filterFolder, filterFile, useMd5);
                }
                catch (Exception e)
                {
                    WriteError(e.Message);
                }
            }

            // Files
            foreach (SyncFile f in rfolder.Files)
            {
                SyncFile esta = null;
                foreach (SyncFile f2 in lfolder.Files)
                {
                    if (f2.Name == f.Name)
                    {
                        f2.Checked = true;
                        esta = f2;
                        break;
                    }
                }

                if (!StringHelper.Like(filterFile, f.Name))
                    continue;

                try
                {
                    if (esta != null)
                    {
                        // Compare
                        if (f.Length == esta.Length)
                        {
                            // Same file
                            if (useMd5)
                            {
                                if (local.GetMd5(f) == remote.GetMd5(esta))
                                    continue;
                            }
                            else continue;

                            // Delete
                            local.DeleteFile(esta);
                            WriteInfo("Deleting file: " + esta.GetFullPath('/'));
                        }
                    }

                    // Write
                    using (Stream stream = remote.GetFile(f))
                    {
                        SyncFile put = new SyncFile(lfolder) { Name = f.Name };
                        local.WriteFile(put, stream);

                        WriteInfo("Writing file: " + put.GetFullPath('/'));
                    }
                }
                catch (Exception e)
                {
                    WriteError(e.Message);
                }
            }
        }
    }
}