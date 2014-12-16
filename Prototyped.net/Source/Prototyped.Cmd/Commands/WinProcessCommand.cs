using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Prototyped.Cmd.Commands
{
    public class WinProcessCommand : IConsoleCommand
    {
        public const String C_LOG_PATH = @"\Logs\";
        public const String C_PROMT_MSG = "You are about to terminate the process '{0}'. \r\n\r\nAre you sure you want to continue?";
        
        #region Properties: IConsoleCommand

        public string HelpTitle { get; internal set; }
        public string HelpText { get; internal set; }

        #endregion

        #region Internal Properties

        private int processID;
        private int delayTime;
        private String actionType;
        private String promptMessage;
        private bool promptUser;

        #endregion

        public WinProcessCommand()
        {
            HelpTitle = "Manages Windows processes for actively running applications.";
            HelpText = @"Usage:
winproc [info|kill] -pid <pid> [-prompt [<msg>]] [-delay <n>]

Options:
info        Shows additional information for the selected process.
kill        Kills the currently selected process.
-pid        The process ID (defined by <pid>) for the application.
-prompt     Prompts the user to confirm the operation.
<msg>       The custom message to show the user on prompt.
-delay      Delays the operation for <n> seconds.
";
        }

        #region Command Actions

        public void RunCommand(string[] args)
        {
            // Innitialise the variables
            processID = -1;
            delayTime = -1;
            actionType = String.Empty;
            promptMessage = String.Empty;
            promptUser = false;

            if (args.Length > 1)
            {
                // Extract the arguments
                actionType = args[0];
                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-pid":
                            processID = Int32.Parse(args[i + 1]);
                            break;
                        case "-prompt":
                            if (args.Length > (i + 1))
                            {
                                promptMessage = args[i + 1];
                            }
                            promptUser = true;
                            break;
                        case "-delay":
                            delayTime = Int32.Parse(args[i + 1]);
                            break;
                    }
                }

                if (delayTime > 0)
                {
                    // Delay the action with the specified time
                    Thread.Sleep(delayTime * 1000);
                }

                if (processID > 0)
                {
                    // Resolve and run the typed action
                    switch (actionType)
                    {
                        case "info":
                            ShowInfo();
                            break;
                        case "kill":
                            KillProcess();
                            break;
                    }
                }
                else
                {
                    Interop.WriteWarning("Process ID is not a valid number.");
                }
            }
            else
            {
                Interop.WriteWarning("Invalid number of arguments.");
            }
        }

        /// <summary>
        /// Kills the selected process
        /// </summary>
        private void KillProcess()
        {
            var proc = FindProcessById(processID);
            if (proc != null)
            {
                // Check if the user should be prompted
                if (promptUser)
                {
                    // Check for a definned prompt message
                    if (String.IsNullOrEmpty(promptMessage))
                    {
                        promptMessage = C_PROMT_MSG;
                    }
                    if (promptMessage.Contains("{0}"))
                    {
                        // Substitute the process name
                        promptMessage = String.Format(promptMessage, proc.ProcessName);
                    }
                    promptMessage = promptMessage.Replace(@"\n", "\n");
                    //if (MessageBox.Show(promptMessage, "Confirm Termination", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        // User canceled
                        return;
                    }
                }

                // Log the Thread instances
                LogThreadInstances(proc);

                // Try annd kill the process
                proc.Kill();
            }
            else
            {
                Interop.WriteWarning(String.Format("Process with id '{0}' could not be found.", processID));
            }
        }

        /// <summary>
        /// Log the thread instances to a text file for later insppection and diagnostics
        /// </summary>
        /// <param name="proc"></param>
        protected void LogThreadInstances(Process proc)
        {
            // If the directory doesn't exist, create it.
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + C_LOG_PATH;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Write the string to the log file.
            var filename = String.Format(path + "{0} - {1}.log", proc.ProcessName, DateTime.Now.ToString("MM-dd-yyyy HH-MM"));
            var output = ExtractProcessInfo(proc);
            var file = new StreamWriter(filename);
            file.WriteLine(output);
            file.Close();
        }

        /// <summary>
        /// Shows informmation about the currently selected process
        /// </summary>
        private void ShowInfo()
        {
            var proc = FindProcessById(processID);
            if (proc != null)
            {
                var output = ExtractProcessInfo(proc);
                Console.WriteLine(output);
            }
            else
            {
                Interop.WriteWarning(String.Format("Process with id '{0}' could not be found.", processID));
            }
        }

        protected String ExtractProcessInfo(Process proc)
        {
            // Create the Header
            var output =
                @"Stack dump for process '{0}':
----------------------------------------------------------------------------
Machine Name:  {1}
Process ID:     {2}
Paged Memory:  {3} Kb
Private Memory: {4} Kb
Session ID:     {5}
----------------------------------------------------------------------------
Active Threads:
";
            output = String.Format(output,
                                   proc.ProcessName,
                                   proc.MachineName,
                                   proc.Id,
                                   proc.PagedMemorySize64 / 1024,
                                   proc.PrivateMemorySize64 / 1024,
                                   proc.SessionId);

            // Generate the stack trace
            foreach (ProcessThread thread in proc.Threads)
            {
                output += String.Format(" - Thread #{0} (State={2}, Priority={1})\r\n",
                                        thread.Id,
                                        thread.CurrentPriority,
                                        thread.ThreadState);
            }

            return output;
        }

        /// <summary>
        /// Finds the process with the specified pid
        /// </summary>
        /// <param name="procId"></param>
        /// <returns></returns>
        protected Process FindProcessById(int procId)
        {
            return Process.GetProcesses().FirstOrDefault(clsProcess => clsProcess.Id == procId);
        }

        #endregion
    }
}
