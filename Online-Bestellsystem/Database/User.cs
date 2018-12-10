using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public String Firstname { get; set; }
        [Required]
        public String Lastname { get; set; }
        [Required]
        public String Username { get; set; }
        [Required]
        public String Email { get; set; }
        [Required]
        public String Password { get; set; }
        public virtual Role Role { get; set; }
    }
}
