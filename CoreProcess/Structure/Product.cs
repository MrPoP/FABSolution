using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Product
    {
        private double price = 0.0;
        private string name = string.Empty;
        private ProductFlag flag = ProductFlag.None;

        public double Price { get { return Math.Round(this.price, 2); } }
        public string Name { get { return this.name; } }
        public ProductFlag Flag { get { return this.flag; } }

        public Product(string productname, ProductFlag _flag, double inprice = 0.0)
        {
            name = productname;
            price = inprice;
            flag = _flag;
        }

        public void EditPrice(double newprice)
        {
            price = newprice;
        }

        ~Product()
        {
            price = 0.0;
            name = string.Empty;
            flag = ProductFlag.None;
        }
    }
}
