using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class OrderDetailDTO
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public UnitOfMeasureDTO UoM { get; set; }
    }
}
