using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Order_Item
    {
        public int Id { get; set; }
        public virtual Product Product { get; set; }
        public int Amount { get; set; }
    }
}
