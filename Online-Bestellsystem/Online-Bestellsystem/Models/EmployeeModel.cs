using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_Bestellsystem.Models
{
    public class EmployeeModel
    {
        public List<Product> Products { get; set; }
        public List<User> Supplier { get; set; }
        public  List<Category> Categories { get; set; }
    }
}
