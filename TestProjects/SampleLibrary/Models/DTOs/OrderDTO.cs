using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public ICollection<OrderDetailDTO> OrderDetails { get; set; }
        public CustomerDTO Customer { get; set; }
    }
}
