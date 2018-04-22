using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class OrderDetail
    {
        public OrderDetail(int id, string itemName, decimal quantity)
        {
            this.Id = id;
            this.ItemName = itemName;
            this.Quantity = quantity;
        }

        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public UnitOfMeasure UoM { get; set; }
    }
}
