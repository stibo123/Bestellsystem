using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Product
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public double Price { get; set; }
        public virtual Category Category { get; set; }
        public ProductImage ProductImage { get; set; }
        public bool IsEnabled { get; set; }
    }
}
