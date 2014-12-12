using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Prototyped.Base.Repository
{
    public abstract class ARepository
    {
        public abstract bool FileExists(string fileName);
        public abstract bool DeleteFile(string fileName);
        public abstract File LoadFile(string fileName);
        public abstract File CreateFile(string fileName, Stream contents);
        public abstract File UpdateFile(string fileName, Stream contents);

        public abstract class File
        {
            public string Name { get; set; }
            public Folder Parent { get; set; }
            public Stream Stream { get; set; }

            public string FullPath
            {
                get
                {
                    return Parent.FullName + Path.PathSeparator + Name;
                }
            }

            protected File(string name, Stream contents, Folder parent = null)
            {
                Name = name;
                Parent = parent;
                Stream = contents;
            }

            protected abstract void Load(StreamReader reader);
            protected abstract void Save(StreamWriter writer);
        }

        public abstract class Folder
        {
            public string Name { get; protected set; }
            public Folder Parent { get; protected set; }

            public string FullName {
                get
                {
                    var path = Name;
                    var curr = this;
                    while (curr.Parent != null)
                    {
                        curr = curr.Parent;
                        path =  curr.Name + Path.PathSeparator + path;
                    }
                    return path;
                }
            }

            protected Folder(string name, Folder parent = null)
            {
                Name = name;
                Parent = parent;
            }

            public abstract IEnumerable<Folder> ListFolders();
            public abstract IEnumerable<File> ListFiles(string filter = null);
            public abstract void AttachFile(File file);
            public abstract void RemoveFile(File file);
        }
    }
}