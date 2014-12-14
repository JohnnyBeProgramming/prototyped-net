using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Prototyped.Base.Generics;
using Prototyped.Base.Interfaces;
using Prototyped.Data;
using Prototyped.Data.Migrations;
using Prototyped.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Data.Commands
{
    [ProtoCommand("sql", "Command Line Database Generation Utility.")]
    public class SqlCommand : IConsoleCommand
    {
        public string CommandName { get; internal set; }
        public string CommandDescription { get; internal set; }
        public string CommandHelpText { get; internal set; }

        public string ActionType { get; internal set; }
        protected SqlConnectionStringBuilder ConnInfo { get; set; }

        [ProtoCmdArg("/Q", true, Hint = "Enable quiet mode. ")]
        public bool SilentMode { get; set; }

        [ProtoCmdArg("/Y", false, Hint = "Do no display any user promts.")]
        public bool ShowPromts { get; set; }

        public SqlCommand()
        {
            ShowPromts = true;
            ConnInfo = new SqlConnectionStringBuilder
            {
                DataSource = "."
            };

            CommandDescription = "Command Line Database Generation Utility.";
            CommandHelpText = @"Usage:
sql <command> [ -conn <conn_str_name> | -db <db_name> ] [ -trusted | -wa | -un <user> -pw <password> ] 

Command Options:
info        Shows additional information for the specified database.
init        Create and Initialise the specified database.
update      Update and migrate the current database version.
backup      Warning! Backup the current database to offline storage.
restore     Warning! Restore a snapshot to the current database.
delete      Warning! Drops the currently selected database.

Parameters:
-conn       The connection string name (defined by <conn_str_name>).
-db         The database name (defined by <db_name>).
-un         The SQL login user name for the current database.
-pw         The SQL login password that goes with the username.
-wa         Use Windows Authentication to connect to the database.
-trusted    Use Windows Authentication to connect to the database.
"; ;
        }

        public void RunCommand(string[] args)
        {
            try
            {
                // Parse the options
                ParseArguments(args);

                // Check and confirm connection
                var conn = GetInfo();
                switch (ActionType)
                {
                    case "info":
                        DatabaseShowInfo(conn.ConnectionString);
                        break;
                    case "init":
                        DatabaseInitialize(conn.ConnectionString);
                        break;
                    case "update":
                        DatabaseUpdate(conn.ConnectionString);
                        break;
                    case "backup":
                        DatabaseBackup(conn.ConnectionString);
                        break;
                    case "restore":
                        DatabaseRestore(conn.ConnectionString);
                        break;
                    case "delete":
                        DatabaseDelete(conn.ConnectionString);
                        break;
                    default:
                        throw new Exception("Command '" + ActionType + "' not recondised.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        private void ParseArguments(string[] args)
        {
            if (args.Length > 1)
            {
                // Extract the arguments
                ActionType = args[0];
                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "/Q":
                            SilentMode = true;
                            break;
                        case "/Y":
                            ShowPromts = false;
                            break;
                        case "-conn":
                            if (args.Length <= (i + 1)) continue;
                            var connKey = args[i + 1];
                            var connStr = !string.IsNullOrEmpty(connKey) ? ConfigurationManager.ConnectionStrings[connKey] : null;
                            if (connStr != null)
                            {
                                // Parse the connection string
                                var builder = new SqlConnectionStringBuilder { ConnectionString = connStr.ConnectionString };
                                ConnInfo.DataSource = builder.DataSource;
                                ConnInfo.InitialCatalog = builder.InitialCatalog;
                                ConnInfo.UserID = builder.UserID;
                                ConnInfo.Password = builder.Password;
                                ConnInfo.IntegratedSecurity = builder.IntegratedSecurity;
                            }
                            break;
                        case "-db":
                            if (args.Length <= (i + 1)) continue;
                            ConnInfo.InitialCatalog = args[i + 1];
                            break;
                        case "-un":
                            if (args.Length <= (i + 1)) continue;
                            ConnInfo.UserID = args[i + 1];
                            break;
                        case "-pw":
                            if (args.Length <= (i + 1)) continue;
                            ConnInfo.Password = args[i + 1];
                            break;
                        case "-wa":
                            ConnInfo.IntegratedSecurity = true;
                            break;
                        case "-trusted":
                            ConnInfo.IntegratedSecurity = true;
                            break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid number of arguments.");
            }
        }

        private SqlConnectionStringBuilder GetInfo()
        {
            if (ShowPromts && string.IsNullOrEmpty(ConnInfo.InitialCatalog))
            {
                Console.WriteLine(" - Enter target database name: ");
                ConnInfo.InitialCatalog = Console.ReadLine();
            }
            if (string.IsNullOrEmpty(ConnInfo.InitialCatalog))
            {
                if (!ShowPromts) throw new Exception("Database name (InitialCatalog) not defined.");
            }
            return ConnInfo;
        }

        [ProtoCmdAction("info", "Shows additional information for the specified database.")]
        private void DatabaseShowInfo(string connString)
        {
            throw new NotImplementedException();
        }

        private void DatabaseInitialize(string connString)
        {
            if (ShowPromts)
            {
                var conn = new SqlConnectionStringBuilder(connString);
                Console.WriteLine(" - Warning: You are about to initialize [" + conn.InitialCatalog + "]");
                Console.Write(" - Are you sure you want to continue? ");
                Console.ReadLine();
            }

            // Generate the database...
            using (var db = new ProtoDB(connString))
            {
                if (!SilentMode) Console.WriteLine(" - Initialising Database [ " + db.Database.Connection.Database + " ]...");

                // Create the database roles
                DatabaseDefineRoles(db, new[] { 
                    AppRoles.Admin, 
                    AppRoles.Tester, 
                    AppRoles.Support 
                });

                // Create the admin user (if not exists)                
                if (!db.Users.Any() && ShowPromts)
                {
                    Console.WriteLine(" - You need to create an admin user...");
                    var isValid = false;
                    var username = string.Empty;
                    var password = string.Empty;
                    do
                    {
                        Console.Write(" - Admin username: "); username = Console.ReadLine();
                        Console.Write(" - Admin password: "); password = Console.ReadLine();
                        isValid = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
                        if (!isValid) Console.WriteLine(" - Invalid username and / or password...");
                    }
                    while (!isValid);
                    DatabaseDefineAdminUser(db, username, password);
                }

                // Display all Blogs from the database 
                var query = from u in db.Users
                            orderby u.UserName
                            select u;

                if (!SilentMode && query.Any())
                {
                    Console.WriteLine(" - Listing all active users:");
                    Console.WriteLine("   ----------------------------------------------------------------------------");
                    foreach (var item in query)
                    {
                        Console.WriteLine(" - [ " + item.UserName.PadRight(9) + " | " + (item.EmailConfirmed ? "Confirmed" : "").PadRight(8) + " | " + (item.LockoutEndDateUtc == null ? "Active" : "Locked").PadRight(5) + " | " + item.Email.PadRight(39) + " ]");
                    }
                    Console.WriteLine("   ----------------------------------------------------------------------------");
                }                
            }
        }

        private void DatabaseUpdate(string connString)
        {
            throw new NotImplementedException();
        }

        private void DatabaseBackup(string connString)
        {
            throw new NotImplementedException();
        }

        private void DatabaseRestore(string connString)
        {
            throw new NotImplementedException();
        }

        private void DatabaseDelete(string connString)
        {
            throw new NotImplementedException();
        }

        private void DatabaseDefineAdminUser(ProtoDB db, string username, string password)
        {
            var admin = new Person
            {
                UserName = username,
                PasswordHash = password,
                FirstName = "Administrator",
                LastName = "User",
                Email = "admin@prototyped.net",
                EmailConfirmed = true,
                LockoutEnabled = false,
            };

            // Attach the user roles
            DatabaseDefineUserRoles(db, admin, new[] { 
                    AppRoles.Admin 
                });

            // Create the admin user
            db.Users.Add(admin);
            db.SaveChanges();
        }

        private void DatabaseDefineUserRoles(ProtoDB db, Person user, string[] roles)
        {
            foreach (var role in db.Roles.Where(r => roles.Contains(r.Name)))
            {
                user.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = user.Id });
            }
        }

        private void DatabaseDefineRoles(ProtoDB db, string[] roles)
        {
            if (!db.Roles.Any(r => r.Name == AppRoles.Admin))
            {
                using (var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db)))
                {
                    // Define the default user roles
                    foreach (var roleName in roles.Where(name => !roleManager.RoleExists(name)))
                    {
                        // Create a new role
                        roleManager.Create(new IdentityRole(roleName));
                    }
                }
            }
        }
    }
}
