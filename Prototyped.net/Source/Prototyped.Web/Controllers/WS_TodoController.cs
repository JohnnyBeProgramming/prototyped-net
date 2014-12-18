using Prototyped.Web.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Prototyped.Web.Models.DataModels;
using Prototyped.Data.Models;

namespace Prototyped.Web.Controllers
{

    public class WS_TodoController : ApiController
    {
        private DBContext db = new DBContext();
        //HttpContext httpContext = new HttpContext(new Http

        public RoleManager<IdentityRole> RoleManager { get; private set; }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        [Authorize]
        public List<ToDoItem> GetUserTodoItems()
        {
            var userId = Request.GetOwinContext().Authentication.User.Identity.GetUserId();
            var currentUser = UserManager.FindById(userId);
            return currentUser.ToDoItems;
        }

        [HttpPost]
        [Authorize]
        public HttpResponseMessage PostTodoItem(TodoItemViewModel item)
        {
            var modelStateErrors = ModelState.Values.ToList();
            var errors = new List<string>();

            foreach (var s in modelStateErrors)
                foreach (var e in s.Errors)
                    if (e.ErrorMessage != null && e.ErrorMessage.Trim() != "")
                        errors.Add(e.ErrorMessage);

            if (errors.Count == 0)
            {
                try
                {
                    var userId = Request.GetOwinContext().Authentication.User.Identity.GetUserId();
                    var currentUser = UserManager.FindById(userId);
                    currentUser.ToDoItems.Add(new ToDoItem()
                    {
                        Completed = false,
                        Description = item.Description
                    });

                    UserManager.Update(currentUser);
                    return Request.CreateResponse(HttpStatusCode.Accepted);
                }
                catch
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return Request.CreateResponse<List<string>>(HttpStatusCode.BadRequest, errors);
            }
        }

        [HttpPost]
        [Authorize]
        async public Task<HttpResponseMessage> CompleteTodoItem(int id)
        {
            var item = db.ToDo.Where(t => t.ID == id).FirstOrDefault();
            if (item != null)
            {
                item.Completed = true;
                await db.SaveChangesAsync();
            }
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        [HttpPost]
        [Authorize]
        async public Task<HttpResponseMessage> DeleteTodoItem(int id)
        {
            var item = db.ToDo.Where(t => t.ID == id).FirstOrDefault();
            if (item != null)
            {
                db.ToDo.Remove(item);
                await db.SaveChangesAsync();
            }
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }




    }
}
