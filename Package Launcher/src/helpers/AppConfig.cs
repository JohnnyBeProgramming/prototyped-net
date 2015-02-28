using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prototyped.exe
{
    public static class AppConfig
    {
        public static string PackageFile = "package.json";
        public static string PackageSetup = @"npm install";
        public static string PackageUpdate = @"npm update";
        public static string PackageStart = @"npm start";
        public static string PackageDir = "node_modules";

        public static bool UseShadowFolder { get; private set; }

        public static string GetPackageFolder(string name = null)
        {
            var relPath = PackageDir;
            var baseDir = UseShadowFolder
                                ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
                                + Path.DirectorySeparatorChar + typeof(Program).Assembly.ManifestModule.Name
                                : Environment.CurrentDirectory;

            if (!string.IsNullOrEmpty(relPath)) baseDir = baseDir + @"\" + relPath;
            if (!string.IsNullOrEmpty(name)) baseDir = baseDir + @"\" + name;
            
            return baseDir;
        }
    }

}
