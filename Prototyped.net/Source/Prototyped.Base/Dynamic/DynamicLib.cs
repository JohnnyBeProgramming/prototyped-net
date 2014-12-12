using System;
using System.IO;
using System.Linq;
using Prototyped.Base.Generics;
using Prototyped.Base.Repository;

namespace Prototyped.Base.Dynamic
{
    public class DynamicLib
    {
        public string LastError { get; set; }

        protected ARepository Storage { get; set; }

        public DynamicLib(ARepository storage)
        {
            Storage = storage;
        }

        public Project CreateProject(string name, string defaultNamespace)
        {
            // Create the project file
            var prj = new Project
            {
                Name = name,
                Namespace = defaultNamespace
            };

            return prj;
        }

        public Project LoadProject(string filePath)
        {
            try
            {
                if (!Storage.FileExists(filePath))
                {
                    throw new Exception("The file '" + filePath + "' does not exists.");
                }

                var file = Storage.LoadFile(filePath);
                if (file != null)
                {
                    var buffer = new byte[file.Stream.Length];
                    file.Stream.Read(buffer, 0, buffer.Length);

                    var proj = Serializer.DeserializeObject<Project>(new string(buffer.Select(b => (char)b).ToArray()));
                    if (proj != null)
                    {
                        proj.FileName = filePath;
                    }
                    return proj;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
            return null;
        }

        public bool SaveProject(Project proj, string path)
        {
            var success = true;
            try
            {
                proj.FileName = null;

                var filePath = string.Format("{0}{1}{2}.dlib", path, Path.DirectorySeparatorChar, proj.Name);
                var fileOutput = Serializer.SerializeObject(proj);
                var stream = new MemoryStream(fileOutput.Select(c => (byte)c).ToArray());
                if (Storage.FileExists(filePath))
                {
                    Storage.UpdateFile(filePath, stream);
                }
                else
                {
                    Storage.CreateFile(path, stream);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                success = false;
            }
            finally
            {
                proj.FileName = path;
            }
            return success;
        }
    }
}