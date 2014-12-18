using Prototyped.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Prototyped.Web.Models.DataModels
{
    public class DBContext : ProtoDB
    {
        // Override default table names
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public static DBContext Create() { return new DBContext(); }
    }
}