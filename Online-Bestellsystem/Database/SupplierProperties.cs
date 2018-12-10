using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class SupplierProperties
    {
        public int Id { get; set; }
        public String CompanyName { get; set; }
        public String DeadLine { get; set; }
        public bool WantEmail { get; set; }
        public List<DateTime> DeliveryDates { get; set; }
        public List<Product> Products { get; set; }
        public virtual User User { get; set; }
    }
}
