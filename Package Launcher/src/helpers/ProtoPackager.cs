using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prototyped.exe.helpers
{
    public class ProtoPackager
    {
        public static string[] StaticIncludes = new[] { 
            AppConfig.PackageFile,
            AppConfig.PackageDir + Path.DirectorySeparatorChar,
            "Setup.cmd",
            "Start.cmd",
        };

        public static ProgressTracker WorkerProgress { get; private set; }
        public class ProgressTracker
        {
            public bool IsBusy { get; set; }
            public float Progress { get; set; }
            public float Total { get; set; }
            public string StatusText { get; set; }
        }

        public static void Unpack(string workingFolder, string resourcePath)
        {
            // Ensure base folder exists
            var baseFolder = AppConfig.GetPackageFolder();
            if (!Directory.Exists(baseFolder)) Directory.CreateDirectory(baseFolder).Attributes |= FileAttributes.Hidden;

            // Create working folder (if not exists)
            if (!Directory.Exists(workingFolder)) Directory.CreateDirectory(workingFolder).Attributes |= FileAttributes.Hidden;

            // Extract resources
            ExtractEmbeddedResources(typeof(ProtoPackager).Assembly, resourcePath, workingFolder);
        }

        private static void ExtractEmbeddedResources(Assembly assembly, string resourceLocation, string outputDir)
        {
            // Initialise a progress indicator
            WorkerProgress = new ProgressTracker
            {
                IsBusy = true,
                Total = 1.0f,
                Progress = 0.0f,
                StatusText = "Initialising..."
            };

            // Build a list of resources to extract according to some criteria
            var list = new List<string> { };
            try
            {
                WorkerProgress.StatusText = "Extracting resources...";
                var prefix = assembly.ManifestModule.Name;
                if (!string.IsNullOrEmpty(resourceLocation))
                {
                    prefix += "." + resourceLocation;
                }
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    var res = assembly.GetManifestResourceInfo(resourceName);
                    var inc = StaticIncludes.Any(name => resourceName.StartsWith(assembly.ManifestModule.Name + "." + name));
                    if (inc || resourceName.StartsWith(prefix))
                    {
                        var ident = inc ? resourceName.Substring(assembly.ManifestModule.Name.Length + 1)
                                        : resourceLocation + "." + resourceName.Substring(prefix.Length + 1);
                        var ext = Path.GetExtension(ident);
                        var val = ident.Substring(0, ident.Length - ext.Length);
                        var pos = val.IndexOf(".", 1);
                        var cwd = outputDir + Path.DirectorySeparatorChar;
                        while (pos > 0)
                        {
                            var dir = val.Substring(0, pos);
                            var path = Path.Combine(cwd, dir);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(path).Attributes |= FileAttributes.Hidden;
                            }
                            val = val.Substring(pos + 1);
                            cwd = Path.Combine(cwd, dir);
                            pos = val.IndexOf(".", 1);
                        }
                        list.Add(Path.Combine(cwd.Substring(outputDir.Length + 1), val + ext));
                    }
                }
            }
            catch (Exception ex)
            {
                WorkerProgress.StatusText = "Error: " + ex.Message;
                return;
            }

            // Setup step counters
            long stepIndex = 0;
            long stepTotal = list.Count;
            foreach (var file in list)
            {
                // Changes status message (to user)
                WorkerProgress.StatusText = "Extracting: " + file;

                // Define progress parts
                var progSize = 1 / (float)stepTotal;
                var progDone = stepIndex * progSize;

                // Parse the file path
                var filePath = Path.Combine(outputDir, file);
                var cpath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(cpath))
                {
                    Directory.CreateDirectory(cpath).Attributes |= FileAttributes.Hidden;
                }

                // Create the file only if it does not exists
                if (!File.Exists(filePath))
                {
                    var resFile = assembly.ManifestModule.Name + "." + file.Replace("\\", ".");
                    using (var stream = assembly.GetManifestResourceStream(resFile))
                    {
                        var fileSize = stream.Length;
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            for (var i = 0; i < stream.Length; i++)
                            {
                                fileStream.WriteByte((byte)stream.ReadByte());
                                WorkerProgress.Progress = progDone + progSize * i / fileSize;
                            }
                            fileStream.Close();
                        }
                    }
                    // Make the file a temp file (caches it in memory)
                    //File.SetAttributes(filePath, FileAttributes.Temporary);
                }
                stepIndex++;
            }
            WorkerProgress.StatusText = "Done";
            WorkerProgress.IsBusy = false;
        }

        public static Process Run(string filename, string workingDir = null, string args = null)
        {
            if (workingDir == null)
            {
                workingDir = AppConfig.GetPackageFolder();
            }

            // Define what to run
            var info = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = " /C " + filename + " " + args,
                WorkingDirectory = workingDir,
                UseShellExecute = false, // Required for environment variables
                //RedirectStandardOutput = false,
                //RedirectStandardError = true,
            };

            // Define the process that will run it
            var proc = new Process
            {
                StartInfo = info,                
            };

            // Start the process!
            var success = proc.Start();
            if (success)
            {
                // Program was Run successfully
            }
            return proc;
        }

        public static bool IsInstalled(string workingFolder)
        {
            var targetDir = AppConfig.GetPackageFolder();
            var isInstalled = Directory.Exists(targetDir);
            return isInstalled;
        }

        public static void Uninstall(string workingFolder)
        {
            // Clear and delete working folder
            var targetDir = AppConfig.GetPackageFolder();
            if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
            foreach (var path in StaticIncludes.Where(path => path.EndsWith("\\")))
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }

            // Clear the working folder of logs and stuff
            foreach (var file in new string[] { 
                    // Add files that should be manually deleted here...
                }
                .Concat(StaticIncludes.Where(path => path != AppConfig.PackageFile))
                .Concat(Directory.GetFiles(workingFolder).Where(file => file.EndsWith(".log"))))
            {
                File.Delete(file);
            };
        }
    }
}
