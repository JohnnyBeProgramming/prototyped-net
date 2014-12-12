using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Prototyped.Base.Repository
{
    public class CachedFile : ARepository.File
    {
        protected byte[] Buffer { get; set; }

        public CachedFile(string name, Stream contents, CachedFolder parent = null) : base(name, contents, parent) { }

        #region ARepository.File

        protected override void Load(StreamReader reader)
        {
            var input = reader.ReadToEnd();
            Buffer = input.ToCharArray().Select(c => (byte)c).ToArray();
        }

        protected override void Save(StreamWriter writer)
        {
            var stream = new MemoryStream(Buffer);
            writer.Write(stream);
        }

        #endregion
    }

    public class CachedFolder : ARepository.Folder
    {
        public readonly List<CachedFile> Files = new List<CachedFile>();
        public readonly List<CachedFolder> Folders = new List<CachedFolder>();

        public CachedFolder(string name, CachedFolder parent = null)
            : base(name, parent)
        {
            Files = new List<CachedFile>();
            Folders = new List<CachedFolder>();
        }

        #region ARepository

        public override IEnumerable<ARepository.Folder> ListFolders()
        {
            return Folders;
        }

        public override IEnumerable<ARepository.File> ListFiles(string filter = null)
        {
            var regex = new Regex((filter ?? "*").Replace("*", "(.*)"));
            return Files.Where(f => regex.Match(f.Name).Success);
        }

        public override void AttachFile(ARepository.File file)
        {
            // Remove any exiting file
            var existing = Files.FirstOrDefault(f => f.Name == file.Name);
            if (existing != null)
            {
                RemoveFile(existing);
            }

            // Add new file object
            Files.Add((CachedFile)file);
        }

        public override void RemoveFile(ARepository.File file)
        {
            var existing = Files.FirstOrDefault(f => f.Name == file.Name);
            if (existing != null)
            {
                Files.Remove(existing);
            }
        }

        #endregion

        public CachedFolder CreateFolder(string name)
        {
            var folder = Folders.FirstOrDefault(f=>f.Name == name);
            if (folder != null)
            {
                folder = new CachedFolder(name, this);
                Folders.Add(folder);
            } 
            return folder;
        }
    }

    public class CachedStorage : ARepository
    {
        public CachedFolder Current { get; set; }
        public readonly CachedFolder Root = new CachedFolder("~");

        public CachedStorage()
        {
            Current = Root;
        }

        public CachedFolder GetFolder(string filePath, bool createIfNotExists = false)
        {
            return GetFolder(filePath, Current ?? Root, createIfNotExists);
        }
        public CachedFolder GetFolder(string filePath, CachedFolder folder, bool createIfNotExists = false)
        {
            var fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrEmpty(fileName))
            {
                filePath = filePath.Substring(0, filePath.Length - fileName.Length);
            }
            else
            {
                filePath = Path.GetDirectoryName(filePath) ?? string.Empty;
            }
            var fullPath = filePath.TrimEnd(Path.DirectorySeparatorChar);
            var directories = fullPath.Split(Path.DirectorySeparatorChar);
            if (!directories.Any())
            {
                // This is the file name (probably)...
                return folder;
            }

            var rootPrefix = Root.FullName;
            var firstPath = directories.First();
            if (firstPath.StartsWith(rootPrefix))
            {
                // Select the root folder and navigate from there
                folder = GetFolder(filePath.Substring(rootPrefix.Length), Root);
            }
            if (firstPath == "..")
            {
                if (folder != Root)
                {
                    folder = (CachedFolder)folder.Parent;
                }                
                else
                {
                    throw new Exception("Cannot navigate past the root folder. Path: " + filePath);
                }
            }
            else if (!string.IsNullOrEmpty(firstPath))
            {
                var found = folder.Folders.FirstOrDefault(f => f.Name == firstPath);
                if (found != null)
                {
                    folder = found;
                }
                else if (createIfNotExists)
                {
                    folder = folder.CreateFolder(firstPath);
                }
                else
                {
                    throw new Exception("Could not find file or part of folder. Path: " + filePath);
                }
            }

            // Get the next sub path section
            var index = 1;
            var subPath = string.Empty;
            while (index < directories.Length - 1)
            {
                subPath += directories[index] + Path.PathSeparator;
                index++;
            }

            // Check if we should find the sub path
            if (!string.IsNullOrEmpty(subPath))
            {
                folder = GetFolder(subPath, folder);
            }

            // Return the matched folder
            return folder;
        }

        public CachedFile GetFile(string fileName)
        {
            return GetFile(fileName, Current);
        }
        public CachedFile GetFile(string fileName, CachedFolder folder)
        {
            var target = GetFolder(fileName, folder);
            if (target != null)
            {
                var ident = Path.GetFileName(fileName);
                var file = target.Files.FirstOrDefault(f => f.Name == ident);
                if (file != null)
                {
                    return file;
                }
            }
            return null;
        }

        #region ARepository

        public override bool FileExists(string fileName)
        {
            try
            {
                var file = GetFile(fileName);
                return file != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool DeleteFile(string fileName)
        {
            try
            {
                var file = GetFile(fileName);
                if (file != null && file.Parent != null)
                {
                    file.Parent.RemoveFile(file);
                    file = null;
                }
                return file == null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override File LoadFile(string fileName)
        {
            var file = GetFile(fileName);
            return file;
        }

        public override File CreateFile(string fileName, Stream contents)
        {
            if (FileExists(fileName))
            {
                // Remove old file
                throw new Exception(string.Format("File already exists: {0}", fileName));
            }

            // Get the folder to create file in
            var path = GetFolder(fileName, true) ?? Root;
            var file = (path != null) ? new CachedFile(Path.GetFileName(fileName), contents, path) : null;
            if (path != null)
            {
                // Attach the file link
                path.AttachFile(file);
            }
            return file;
        }

        public override File UpdateFile(string fileName, Stream contents)
        {
            if (FileExists(fileName))
            {
                // Remove old file
                DeleteFile(fileName);
            }

            // (Re)create new file contents
            return CreateFile(fileName, contents);
        }

        #endregion
    }
}