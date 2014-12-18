using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Prototyped.Base;
using Prototyped.Base.Commands;
using Prototyped.Base.Generics;
using Prototyped.Base.Interfaces;
using Prototyped.Data;
using Prototyped.Data.Migrations;
using Prototyped.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Data.Commands
{
    [Proto.Command("sql", "Command Line Database Generation Utility.")]
    public class ProtoSqlCmd
    {
        #region Options and Switches

        [Proto.Command.Arg("/Q", true, Hint = "Enable quiet mode.")]
        public bool SilentMode { get; set; }

        [Proto.Command.Arg("/Y", false, Hint = "Do no display any user promts.")]
        public bool ShowPromts { get; set; }

        [Proto.Command.Arg("-hn", AttrParser.UseNextArg, Hint = "The hostname used in the connnection string.")]
        public string Hostname { set { ConnInfo.DataSource = value; } }

        [Proto.Command.Arg("-db", AttrParser.UseNextArg, Hint = "The database used in the connnection string.")]
        public string DataSource { set { ConnInfo.InitialCatalog = value; } }

        [Proto.Command.Arg("-un", AttrParser.UseNextArg, Hint = "The username used in the connnection string.")]
        public string Username { set { ConnInfo.UserID = value; } }

        [Proto.Command.Arg("-pw", AttrParser.UseNextArg, Hint = "The password used in the connnection string.")]
        public string Password { set { ConnInfo.Password = value; } }

        [Proto.Command.Arg("-wa", true, Hint = "Use Integrated (Windows Authenticated) Security.")]
        [Proto.Command.Arg("-trusted", true, Hint = "Use Integrated (Windows Authenticated) Security.")]
        public bool WinAth { set { ConnInfo.IntegratedSecurity = value; } }

        [Proto.Command.Arg("-conn:(.*)", 1, Hint = "The connnection string key name found in the config.")]
        public string ConnectionName
        {
            set
            {
                var connKey = value;
                var connStr = !string.IsNullOrEmpty(connKey) ? ConfigurationManager.ConnectionStrings[connKey] : null;
                if (connStr != null)
                {
                    // Parse the new connection string
                    ConnInfo = new SqlConnectionStringBuilder { ConnectionString = connStr.ConnectionString };
                }
            }
        }

        #endregion

        protected SqlConnectionStringBuilder ConnInfo { get; set; }

        public ProtoSqlCmd()
        {
            ShowPromts = true;
            ConnInfo = new SqlConnectionStringBuilder { };
        }

        #region Command Actions

        [Proto.Command.Call("info", "Shows additional information for the specified database.")]
        public void DatabaseShowInfo(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine(" - Hostname: {0}", ConnInfo.DataSource);
            Console.WriteLine(" - Database: {0}", ConnInfo.InitialCatalog);
            Console.WriteLine(" - Username: {0}", string.IsNullOrEmpty(ConnInfo.UserID) ? "Not Set" : ConnInfo.UserID);
            Console.WriteLine(" - WindAuth: {0}", ConnInfo.IntegratedSecurity );
            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        [Proto.Command.Call("init", "Initialise target database with pre-populated tables.")]
        public void DatabaseInitialize(string[] args)
        {
            // Make sure that we have a target database defined
            var hn = DefineHostname();
            var db = DefineDatabase();

            DefineNewDatabase(args);

            if (!SilentMode && ShowPromts)
            {
                Console.Write(" - Initializing '" + ConnInfo.InitialCatalog + "'. Are you sure?");
                Console.ReadLine();
            }

            // Generate the database...
            using (var ctx = new ProtoDB(ConnInfo.ConnectionString))
            {
                if (!SilentMode) Console.WriteLine(" - Initialising Database '" + ctx.Database.Connection.Database + "'...");

                // Create the database roles
                DatabaseDefineRoles(ctx, new[] { 
                    AppRoles.Admin, 
                    AppRoles.Tester, 
                    AppRoles.Support 
                });

                // Create the admin user (if not exists)                
                if (!ctx.Users.Any() && ShowPromts)
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
                    DatabaseDefineAdminUser(ctx, username, password);
                }

                // Display all Blogs from the database 
                var query = from u in ctx.Users
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

        [Proto.Command.Call("create", "Create a new database on the specified host.")]
        public void DefineNewDatabase(string[] args)
        {
            var conn = ConnInfo;
            do
            {
                var sspi = conn.IntegratedSecurity;
                try
                {
                    // Make sure that we have a target
                    var hn = DefineHostname();
                    var db = DefineDatabase();

                    // First we need to create the database
                    if (!string.IsNullOrEmpty(db))
                    {
                        // Connect to master database (when creating)
                        conn.InitialCatalog = "master";

                        // Default to WinAuth if no username                                
                        if (string.IsNullOrEmpty(conn.UserID))
                        {
                            conn.IntegratedSecurity = true;
                        }

                        // Create the specified database
                        if (CreateDatabase(conn, db))
                        {
                            Console.WriteLine(" - Database '{0}' created successfully.", db);
                        }
                    }
                    conn.InitialCatalog = db;
                }
                catch (Exception ex)
                {
                    ConnInfo.InitialCatalog = null;
                    Console.WriteLine(" - Error: " + ex.Message);
                    Console.WriteLine(" - Press [ENTER] with a blank database name to abort.");
                }
                finally
                {
                    // Default to WinAuth if no username                                
                    if (!string.IsNullOrEmpty(conn.UserID))
                    {
                        conn.IntegratedSecurity = sspi;
                    }
                }
            } while (conn.InitialCatalog == "master");
        }

        [Proto.Command.Call("update", "Update and migrate the current database version.")]
        public void DatabaseUpdate(string[] args)
        {
            var conn = ConnInfo;
            try
            {
                // Make sure that we have a target
                var hn = DefineHostname();
                var db = DefineDatabase();

                // First we need to create the database
                if (!string.IsNullOrEmpty(db))
                {
                    // ToDo: Implement Updates
                    throw new NotImplementedException();
                }

                // Upgrades are done at this point
                Console.WriteLine(" - Database '{0}' has been updated.", db);
                conn.InitialCatalog = db;
            }
            catch (Exception ex)
            {
                ConnInfo.InitialCatalog = null;
                Console.WriteLine(" - Error: " + ex.Message);
                Console.WriteLine(" - Press [ENTER] with a blank database name to abort.");
            }
            finally
            {
                // Do Post update stuff                
            }
        }

        [Proto.Command.Call("backup", "Backup the current database to offline storage.", Prefix = "Warning: This cannot be undone!\r\n")]
        public void DatabaseBackup(string[] args)
        {
            throw new NotImplementedException();
        }

        [Proto.Command.Call("restore", "Restore a snapshot to the current database.", Prefix = "Warning: This cannot be undone!\r\n")]
        public void DatabaseRestore(string[] args)
        {
            throw new NotImplementedException();
        }

        [Proto.Command.Call("delete", "Drops the currently selected database.", Prefix = "Warning: This cannot be undone!\r\n")]
        public void DatabaseDelete(string[] args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Functions

        private string DefineHostname()
        {
            if (!SilentMode && ShowPromts && string.IsNullOrEmpty(ConnInfo.DataSource))
            {
                Console.Write(" - Hostname: ");
                ConnInfo.DataSource = Console.ReadLine();
            }
            else if (!ShowPromts) throw new Exception("The 'ConnInfo.DataSource' propty has not been set.");

            return ConnInfo.DataSource;
        }

        private string DefineDatabase()
        {
            if (!SilentMode && ShowPromts && string.IsNullOrEmpty(ConnInfo.InitialCatalog))
            {
                Console.Write(" - Database: ");
                ConnInfo.InitialCatalog = Console.ReadLine();
            }
            else if (!ShowPromts) throw new Exception("Database name (InitialCatalog) not defined.");

            return ConnInfo.InitialCatalog;
        }

        #endregion

        #region Database Helper Functions

        private bool CreateDatabase(SqlConnectionStringBuilder connInfo, string dbName)
        {
            // Try and connect to the SQL server
            using (var conn = new SqlConnection(connInfo.ConnectionString))
            {
                // Connnect...
                conn.Open();

                // Check if DB exists
                using (var myCommand = new SqlCommand("if db_id('" + dbName + "') is not null select 1", conn))
                {
                    using (var r = myCommand.ExecuteReader())
                    {
                        if (r.HasRows) return false;
                    }
                }

                // Detect the data directory
                var dataDir = ".";
                using (var cmd = new SqlCommand("SELECT TOP 1 Physical_Name FROM sys.master_files WHERE DB_NAME(database_id) LIKE 'master'", conn))
                {
                    var path = cmd.ExecuteScalar() as string;
                    if (path != null)
                    {
                        path = Path.GetDirectoryName(path);
                        dataDir = path;
                    }
                }

                if (!SilentMode)
                {
                    // Prompt the user about trying to create the database
                    var pathMax = 65;
                    var pathDesc = dataDir;
                    if (pathDesc.Length > pathMax)
                    {
                        var prefix = pathDesc.Substring(0, pathMax / 2);
                        var suffix = pathDesc.Substring(pathDesc.Length - prefix.Length + 5);
                        pathDesc = prefix + @"\...\" + suffix;
                    }
                    Console.WriteLine(" - DataPath: " + pathDesc);
                    Console.Write(" - Creating '" + dbName + "' on '" + conn.DataSource + "'. Are you sure?");
                    Console.ReadLine();
                }

                var query = string.Format(@"
CREATE DATABASE {0} ON PRIMARY 
(
    NAME = {0}_Data, 
    FILENAME = '{1}\{0}.mdf', 
    SIZE = 5MB, 
    MAXSIZE = 1GB, 
    FILEGROWTH = 10%
) 
LOG ON (
    NAME = {0}_Log, 
    FILENAME = '{1}\{0}.ldf', 
    SIZE = 1MB, 
    MAXSIZE = 5MB, 
    FILEGROWTH = 10%
)", dbName, dataDir);

                // Execute the SQL Command
                using (var myCommand = new SqlCommand(query, conn))
                {
                    myCommand.ExecuteNonQuery();
                }
            }

            return true;
        }

        protected void DatabaseDefineAdminUser(ProtoDB db, string username, string password)
        {
            var admin = new User
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

        protected void DatabaseDefineUserRoles(ProtoDB db, User user, string[] roles)
        {
            foreach (var role in db.Roles.Where(r => roles.Contains(r.Name)))
            {
                user.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = user.Id });
            }
        }

        protected void DatabaseDefineRoles(ProtoDB db, string[] roles)
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

        #endregion
    }
}
