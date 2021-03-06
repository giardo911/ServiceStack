﻿using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.IO;

namespace ServiceStack.VirtualPath
{
    public class FileSystemVirtualPathProvider : AbstractVirtualPathProviderBase, IVirtualFiles, IWriteableVirtualPathProvider
    {
        protected DirectoryInfo RootDirInfo;
        protected FileSystemVirtualDirectory RootDir;

        public override IVirtualDirectory RootDirectory { get { return RootDir; } }
        public override string VirtualPathSeparator { get { return "/"; } }
        public override string RealPathSeparator { get { return Convert.ToString(Path.DirectorySeparatorChar); } }

        public FileSystemVirtualPathProvider(IAppHost appHost, string rootDirectoryPath)
            : this(appHost, new DirectoryInfo(rootDirectoryPath))
        { }

        public FileSystemVirtualPathProvider(IAppHost appHost, DirectoryInfo rootDirInfo)
            : base(appHost)
        {
            if (rootDirInfo == null)
                throw new ArgumentNullException("rootDirInfo");

            this.RootDirInfo = rootDirInfo;
            Initialize();
        }

        public FileSystemVirtualPathProvider(IAppHost appHost)
            : base(appHost)
        {
            Initialize();
        }

        protected override sealed void Initialize()
        {
            if (RootDirInfo == null)
                RootDirInfo = new DirectoryInfo(AppHost.Config.WebHostPhysicalPath);

            if (RootDirInfo == null || !RootDirInfo.Exists)
                throw new ApplicationException(
                    "RootDir '{0}' for virtual path does not exist".Fmt(RootDirInfo.FullName));

            RootDir = new FileSystemVirtualDirectory(this, null, RootDirInfo);
        }


        public string EnsureDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            return dirPath;
        }

        public void WriteFile(string filePath, string textContents)
        {
            var realFilePath = RootDir.RealPath.CombineWith(filePath);
            EnsureDirectory(Path.GetDirectoryName(realFilePath));
            File.WriteAllText(realFilePath, textContents);
        }

        public void WriteFile(string filePath, Stream stream)
        {
            var realFilePath = RootDir.RealPath.CombineWith(filePath);
            EnsureDirectory(Path.GetDirectoryName(realFilePath));
            File.WriteAllBytes(realFilePath, stream.ReadFully());
        }

        public void WriteFiles(IEnumerable<IVirtualFile> files, Func<IVirtualFile, string> toPath = null)
        {
            this.CopyFrom(files, toPath);
        }

        public void DeleteFile(string filePath)
        {
            var realFilePath = RootDir.RealPath.CombineWith(filePath);
            try
            {
                File.Delete(realFilePath);
            }
            catch (Exception /*ignore*/) {}
        }

        public void DeleteFiles(IEnumerable<string> filePaths)
        {
            filePaths.Each(DeleteFile);
        }

        public void DeleteFolder(string dirPath)
        {
            var realPath = RootDir.RealPath.CombineWith(dirPath);
            if (Directory.Exists(realPath))
                Directory.Delete(realPath, recursive: true);
        }
    }
}
