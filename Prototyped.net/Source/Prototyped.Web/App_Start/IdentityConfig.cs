﻿using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Prototyped.Web.Models;
using Prototyped.Data.Models;
using Prototyped.Web.Models.DataModels;

namespace Prototyped.Web
{
    /// <summary>
    /// Configure the application user manager used in this application. UserManager is defined 
    /// in ASP.NET Identity and is used by the application.
    /// </summary>
    public class ApplicationUserManager : UserManager<User>
    {
        public ApplicationUserManager(IUserStore<User> store) : base(store) { }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var store = new UserStore<User>(context.Get<DBContext>());
            var manager = new ApplicationUserManager(store);
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };
            
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 4,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                var ident = "ASP.NET Identity";
                var provider = dataProtectionProvider.Create(ident);
                manager.UserTokenProvider = new DataProtectorTokenProvider<User>(provider);
            }
            return manager;
        }
    }
}
