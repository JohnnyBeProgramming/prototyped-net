using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Data.Models
{
    public class ToDoItem
    {
        [Key]
        public int ID { get; set; }
        public string Description { get; set; }
        public bool Completed { get; set; }
    }
}
