using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volunteers.Models
{
    public class TaskWithUsers
    {
        public string TaskDescription { get; set; }
        public List<User> Users { get; set; }
    }
}
