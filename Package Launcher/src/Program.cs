using prototyped.exe.forms;
using prototyped.exe.helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prototyped.exe
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var packageDir = AppConfig.GetPackageFolder();
            var autoStart = ProtoPackager.IsInstalled(packageDir);
#if DEBUG
            autoStart = false;
#endif
            if (autoStart)
            {
                // Step 1: Unpack the resources into the working folder
                //ProtoPackager.Unpack(Environment.CurrentDirectory, AppConfig.PackageDir);

                // Step 2: Use the npm installer to initialise and setup the globals
                if (Directory.Exists("node_modules"))
                {
                    // Do NPM Update, its faster and saves bandwith
                    ProtoPackager.Run(AppConfig.PackageUpdate, Environment.CurrentDirectory).WaitForExit();
                }
                else if (File.Exists("Setup.cmd"))
                {
                    // A Custom command script was found, use this instead of normal npm install
                    ProtoPackager.Run("Setup.cmd", Environment.CurrentDirectory).WaitForExit();
                }
                else
                {
                    // Do NPM Install normally, no custom command script found
                    ProtoPackager.Run(AppConfig.PackageSetup, Environment.CurrentDirectory).WaitForExit();
                }

                ProtoPackager.Run(AppConfig.PackageStart, Environment.CurrentDirectory, string.Join(" ", args)).WaitForExit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
