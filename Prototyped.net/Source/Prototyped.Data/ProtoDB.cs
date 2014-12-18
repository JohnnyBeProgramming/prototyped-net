using Microsoft.AspNet.Identity.EntityFramework;
using Prototyped.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Data
{
    public class ProtoDB : IdentityDbContext<User>, IDisposable
    {
        public static class Schema
        {
            public readonly static string Users = "usr";
            public readonly static string Proto = "dbo";
        }

        #region Constructors
        public ProtoDB() : base("ProtoDB", throwIfV1Schema: false) { }
        public ProtoDB(string connStr, bool throwIfV1Schema = false) : base(connStr, throwIfV1Schema) { }
        public ProtoDB(ConnectionStringSettings connStr, bool throwIfV1Schema = false) : base(connStr.ConnectionString, throwIfV1Schema) { }
        #endregion

        #region  Custom Datasets (tables) are defined here

        public DbSet<ToDoItem> ToDo { get; set; }

        #endregion

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Generate the Identity tables (note how IdentityUser and User map to the same table)
            modelBuilder.Entity<IdentityUser>().ToTable("Users", Schema.Users).Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<User>().ToTable("Users", Schema.Users).Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles", Schema.Users);
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins", Schema.Users);
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims", Schema.Users);
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", Schema.Users);
            
            // Generate our custom data tables
            //modelBuilder.Entity<ToDoItem>().ToTable("ToDoItem", Schema.Proto);
        }
    }
}
