using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public virtual User User { get; set; }
        public List<Order_Item> Order_Items { get; set; }
    }
}
