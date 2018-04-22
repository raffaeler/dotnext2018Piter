using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class Order : IDomainIdentifier
    {
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public Order(int id, DateTimeOffset date, string description, Customer customer)
            : this()
        {
            this.Id = id;
            this.OrderDate = date;
            this.Description = description;
            this.Customer = customer;
        }

        public int Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string Description { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Customer Customer { get; set; }
    }
}
