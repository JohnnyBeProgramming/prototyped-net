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
    public class ProtoDB : IdentityDbContext<Person>, IDisposable
    {
        #region Constructors
        public ProtoDB() : base("ProtoDB", throwIfV1Schema: false) { }
        public ProtoDB(string connStr, bool throwIfV1Schema = false) : base(connStr, throwIfV1Schema) { }
        public ProtoDB(ConnectionStringSettings connStr, bool throwIfV1Schema = false) : base(connStr.ConnectionString, throwIfV1Schema) { }
        #endregion

        #region  Organisational

        public DbSet<Organisation> Organisations { get; set; }

        #endregion

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("Users").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<Person>().ToTable("Users").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        }
    }
}
