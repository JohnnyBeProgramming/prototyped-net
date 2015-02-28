using prototyped.exe.helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prototyped.exe.forms
{
    public partial class MainForm : Form
    {
        protected string WorkingFolder { get { return Environment.CurrentDirectory; } }
        protected Process WorkingProcess { get; set; }
        protected bool DoUninstall { get; set; }
        public bool AutoStart { get; set; }

        public MainForm()
        {
            InitializeComponent();

            AutoStart = true;

            Load += (s, e) =>
            {
                UpdateUI();
            };
            KeyUp += (s, e) =>
            {
                UpdateUI();
            };
        }

        private void UpdateUI()
        {
            DoUninstall = true; // (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            var isInstalled = ProtoPackager.IsInstalled(WorkingFolder);
            var isRunning = WorkingProcess != null && !WorkingProcess.HasExited;
            var prog = ProtoPackager.WorkerProgress;
            if (prog != null && prog.IsBusy)
            {
                pnlProgress.Visible = true;
                lblStatus.Text = prog.StatusText ?? "Busy...";
                progBar.Minimum = 0;
                progBar.Maximum = 100;
                progBar.Value = (int)(100 * prog.Progress / prog.Total);
            }
            else
            {
                pnlProgress.Visible = false;
                button1.Text = isInstalled ? (DoUninstall ? "Uninstall" : "Update") : "Install";
                button1.Enabled = !isRunning;
                button2.Text = isRunning ? "Close" : "Run";
                button2.Enabled = isInstalled;
            }
        }

        private void RunInstall()
        {
            var ctx = this;
            var checker = new Thread(() =>
            {
                while (!ctx.IsDisposed)
                {
                    Thread.Sleep(500);
                    ctx.Invoke((Action)UpdateUI);
                }
            });
            var runner = new BackgroundWorker { };
            runner.DoWork += (snd, evt) =>
            {
                try
                {
                    checker.Start();

                    // Step 1: Unpack the resources into the working folder
                    ProtoPackager.Unpack(WorkingFolder, AppConfig.PackageDir);

                    // Step 2: Use the npm installer to initialise and setup the globals
                    if (Directory.Exists("node_modules"))
                    {
                        // Do NPM Update, its faster and saves bandwith
                        ProtoPackager.Run(AppConfig.PackageUpdate, WorkingFolder).WaitForExit();
                    }
                    else if (File.Exists("Setup.cmd"))
                    {
                        // A Custom command script was found, use this instead of normal npm install
                        ProtoPackager.Run("Setup.cmd", WorkingFolder).WaitForExit();
                    }
                    else
                    {
                        // Do NPM Install normally, no custom command script found
                        ProtoPackager.Run(AppConfig.PackageSetup, WorkingFolder).WaitForExit();
                    }

                    if (AutoStart) RunStart();
                }
                finally
                {
                    checker.Abort();
                    Invoke((Action)UpdateUI);
                }
            };
            runner.RunWorkerAsync();

        }

        private void RunUpdate()
        {
            var ctx = this;
            var checker = new Thread(() =>
            {
                while (!ctx.IsDisposed)
                {
                    Thread.Sleep(500);
                    ctx.Invoke((Action)UpdateUI);
                }
            });
            var runner = new BackgroundWorker { };
            runner.DoWork += (snd, evt) =>
            {
                try
                {
                    checker.Start();

                    // Do NPM Update, its faster and saves bandwith
                    ProtoPackager.Run(AppConfig.PackageUpdate, WorkingFolder).WaitForExit();
                }
                finally
                {
                    checker.Abort();
                    Invoke((Action)UpdateUI);
                }
            };
            runner.RunWorkerAsync();
        }

        private void RunStart()
        {
            // Check for custom command
            if (File.Exists("Start.cmd"))
            {
                // A Custom command script was found, use this instead of normal npm install
                ProtoPackager.Run("Start.cmd", WorkingFolder).WaitForExit();
            }
            else
            {
                // Do NPM Start normally, no custom command script found
                WorkingProcess = ProtoPackager.Run(AppConfig.PackageStart);
            }
        }

        private void RunUninstall()
        {
            var msg = "Are you sure you want to remove:\r\n" + WorkingFolder;
            var res = MessageBox.Show(msg, "Uninstall", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                ProtoPackager.Uninstall(WorkingFolder);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                switch (button1.Text)
                {
                    case "Install":
                        RunInstall();
                        break;
                    case "Update":
                        RunUpdate();
                        break;
                    case "Uninstall":
                        RunUninstall();
                        break;
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var isRunning = WorkingProcess != null && !WorkingProcess.HasExited;
                if (isRunning)
                {
                    WorkingProcess.CloseMainWindow();
                    WorkingProcess.Kill();
                    WorkingProcess = null;
                    UpdateUI();
                }
                else
                {
                    RunStart();

                    var ctx = this;
                    new Thread(() =>
                    {
                        if (WorkingProcess != null && !WorkingProcess.HasExited)
                        {
                            WorkingProcess.WaitForExit();
                        }
                        ctx.Invoke((Action)ClearWorkerProcess);
                    }).Start();
                }
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearWorkerProcess()
        {
            WorkingProcess = null;
            UpdateUI();
        }
    }
}
