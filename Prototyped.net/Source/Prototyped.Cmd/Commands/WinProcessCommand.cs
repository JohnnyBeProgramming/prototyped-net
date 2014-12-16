using Prototyped.Base;
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
    [Proto.Command("winproc", "Manages Windows processes for actively running applications.")]
    public class WinProcessCommand
    {
        public const String C_PROMT_MSG = "You are about to terminate the process '{0}'. \r\n\r\nContinue?";

        #region Command Options

        [Proto.Command.Arg("-pid", AttrParser.UseNextArg, Hint = "The process ID (defined by <pid>) for the application.")]
        public int ProcessID { get; set; }

        [Proto.Command.Arg("-delay", AttrParser.UseNextArg, Hint = "Delays the operation for <delay> seconds.")]
        public int DelayTime { get; set; }

        [Proto.Command.Arg("-prompt", true, Hint = "Prompts the user to confirm the operation.")]
        public bool PromptUser { get; set; }

        [Proto.Command.Arg("-prompt", AttrParser.UseNextArg, Hint = "The custom message to show the user on prompt.")]
        public string PromptMessage { get; set; }

        #endregion

        public WinProcessCommand()
        {
            PromptUser = true;
        }

        #region Command Actions

        [Proto.Command.Call("list", "Lists all windows processes that are currently rinning.")]
        public void ListAll(string[] args)
        {
            Interop.WriteStatus("  - Listing all active processes.");
            foreach (var proc in FindProcessList().OrderBy(i => i.BasePriority))
            {
                Interop.WriteStatus("  [ PID = {0} Threads = {2} ] {1}", (proc.Id + ",").PadRight(6), proc.ProcessName, proc.Threads.Count.ToString().PadRight(3));
            }
        }

        /// <summary>
        /// Shows informmation about the currently selected process
        /// </summary>
        [Proto.Command.Call("info", "Shows additional information for the selected process.")]
        public void ShowInfo(string[] args)
        {
            var proc = FindProcessById(ProcessID);
            if (proc != null)
            {
                var output = ExtractProcessInfo(proc);
                Console.WriteLine(output);
            }
            else
            {
                Interop.WriteWarning(String.Format("Process with id '{0}' could not be found.", ProcessID));
            }
        }

        /// <summary>
        /// Kills the selected process
        /// </summary>
        [Proto.Command.Call("kill", "Kills the currently selected process.")]
        public void KillProcess(string[] args)
        {
            if (DelayTime > 0)
            {
                // Delay the action with the specified time
                Thread.Sleep(DelayTime * 1000);
            }
            var proc = FindProcessById(ProcessID);
            if (proc != null)
            {
                // Check if the user should be prompted
                if (PromptUser)
                {
                    // Check for a definned prompt message
                    if (String.IsNullOrEmpty(PromptMessage))
                    {
                        PromptMessage = C_PROMT_MSG;
                    }
                    if (PromptMessage.Contains("{0}"))
                    {
                        // Substitute the process name
                        PromptMessage = String.Format(PromptMessage, proc.ProcessName);
                    }
                    Console.WriteLine(PromptMessage);
                    Console.ReadLine();
                }

                // Try annd kill the process
                proc.Kill();
            }
            else
            {
                Interop.WriteWarning(String.Format("Process with id '{0}' could not be found.", ProcessID));
            }
        }

        #endregion

        #region Helper Methods

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

        protected Process FindProcessById(int procId)
        {
            return Process.GetProcesses().FirstOrDefault(clsProcess => clsProcess.Id == procId);
        }

        protected IEnumerable<Process> FindProcessList()
        {
            return Process.GetProcesses();
        }

        #endregion

    }
}
