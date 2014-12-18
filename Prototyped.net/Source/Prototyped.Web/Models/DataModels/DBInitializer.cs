using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Prototyped.Data;
using Prototyped.Data.Models;
using Prototyped.Web.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Prototyped.Web.Models.DataModels
{    
    // This function will ensure the database is created and seeded with any default data.
    public class DBInitializer : CreateDatabaseIfNotExists<DBContext>
    {
        protected override void Seed(DBContext context)
        {
            // Create an seed data you wish in the database.
            
        }
    }
}

