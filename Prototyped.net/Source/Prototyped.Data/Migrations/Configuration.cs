using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Data.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Prototyped.Data.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;

    public class AppRoles
    {
        public const string Admin = "Admin";
        public const string Tester = "Tester";
        public const string Support = "Support";
    }

    internal sealed class ProtoConfigDB : DbMigrationsConfiguration<ProtoDB>
    {
        public ProtoConfigDB()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ProtoDB ctx)
        {
            //  This method will be called after migrating to the latest version.
            base.Seed(ctx);

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.

            // Add Default User Roles
            SeedRole(ctx, AppRoles.Admin);
            SeedRole(ctx, AppRoles.Tester);
            SeedRole(ctx, AppRoles.Support);

            // Add Administrator Account
            SeedUser(ctx, new Person
            {
                UserName = AppRoles.Admin,
                FirstName = "Administrator",
                LastName = "User",
                Email = "admin@prototyped.net",
                EmailConfirmed = true,
                LockoutEnabled = false,
            }, "1Admin2", AppRoles.Admin);

            // Add Demo Sample Accounts (for Organisation leves)
            SeedUser(ctx, "Test", "1Test2", AppRoles.Tester, AppRoles.Support);
            SeedUser(ctx, "Support", "1Support2", AppRoles.Support);

            ctx.SaveChanges();
        }

        private void SeedRole(ProtoDB ctx, string roleName)
        {
            // Check if the role exists
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(ctx));
            var roleExists = roleManager.RoleExists(roleName);
            if (roleExists) return;

            // Create a new role
            roleManager.Create(new IdentityRole(roleName));
        }

        private void SeedUser(ProtoDB ctx, string username, string password, params string[] roles)
        {
            SeedUser(ctx, new Person
            {
                UserName = username,
                FirstName = username,
                LastName = "User",
                Email = username + "@Centimex.net",
                EmailConfirmed = true,
                LockoutEnabled = false,
            }, password, roles);
        }
        private void SeedUser(ProtoDB ctx, Person usr, string password, params string[] roles)
        {
            // Create the Default Accounts
            using (var userManager = new UserManager<Person>(new UserStore<Person>(ctx)))
            {
                //userManager.UserValidator = new EmailUserValidator<Person>(userManager);

                // Try and find the user in the existing DB
                var user = userManager.Find(usr.UserName, password);
                if (user == null)
                {
                    // Use the supplied model
                    user = usr;

                    // Create new user account
                    var result = userManager.Create(user, password);
                    if (!result.Succeeded)
                    {
                        // Create failed
                        var e = new Exception("Could not add user '" + user.UserName + "' in the DB Seeding process...");
                        var enumerator = result.Errors.GetEnumerator();
                        foreach (var error in result.Errors)
                        {
                            e.Data.Add(enumerator.Current, error);
                        }
                        throw e;
                    }
                }
                else
                {
                    // Update Existing User
                    user.UserName = usr.UserName;
                    user.FirstName = usr.FirstName;
                    user.LastName = usr.LastName;
                    user.Email = usr.Email;
                    user.EmailConfirmed = usr.EmailConfirmed;
                    user.LockoutEnabled = usr.LockoutEnabled;
                }

                // Add to specified roles
                foreach (var role in roles)
                {
                    if (!userManager.IsInRole(user.Id, role))
                    {
                        userManager.AddToRole(user.Id, role);
                    }
                }
            }
        }
    }
}
