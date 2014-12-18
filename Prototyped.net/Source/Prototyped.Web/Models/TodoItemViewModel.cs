using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prototyped.Web.Models
{
    public class TodoItemViewModel
    {
        [Required(ErrorMessage = "The Task Field is Required.")]
        public string Description { get; set; }
        public bool Completed { get; set; }
    }
}