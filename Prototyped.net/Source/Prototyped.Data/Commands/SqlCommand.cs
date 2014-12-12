using Prototyped.Base.Interfaces;
using Prototyped.Data;
using Prototyped.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Data.Commands
{
    public class SqlCommand : IConsoleCommand
    {
        public string CommandName { get; internal set; }
        public string CommandDescription { get; internal set; }
        public string CommandHelpText { get; internal set; }

        public SqlCommand()
        {
            CommandDescription = "Command Line Database Generation Utility.";
            CommandHelpText = @"Usage:
sql <command> -db <db_name> [ -trusted | -wa | -un <user> -pw <password> ] 

Command Options:
info        Shows additional information for the specified database.
init        Create and Initialise the specified database.
update      Update and migrate the current database version.
backup      Warning! Backup the current database to offline storage.
restore     Warning! Restore a snapshot to the current database.
delete      Warning! Drops the currently selected database.

Parameters:
-db         The database name (defined by <db_name>).
-un         The SQL login user name for the current database.
-pw         The SQL login password that goes with the username.
-wa         Use Windows Authentication to connect to the database.
-trusted    Use Windows Authentication to connect to the database.
"; ;
        }

        public void RunCommand(string[] args)
        {
            // Generate the database...
            using (var db = new ProtoDB())
            {
                Console.WriteLine(" - Generating Database [ " + db.Database.Connection.Database + " ]...");

                // Create and save the root organisation
                Console.Write(" - Enter a name for the root organisation: ");
                var name = Console.ReadLine();

                // Create the root organisation
                db.Organisations.Add(new Organisation
                {
                    Key = Guid.NewGuid(),
                    Name = name,
                });
                db.SaveChanges();

                // Display all Blogs from the database 
                var query = from o in db.Organisations
                            orderby o.Name
                            select o;

                Console.WriteLine(" - Listing all organisations:");
                foreach (var item in query)
                {
                    Console.WriteLine(" - [" + item.Key + "]" + item.Name);
                }

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

    }
}
