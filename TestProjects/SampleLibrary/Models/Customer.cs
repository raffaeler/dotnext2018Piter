using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class Customer : IDomainIdentifier
    {
        public Customer()
        {
            Orders = new List<Order>();
        }

        public Customer(int id, string name)
            : this()
        {
            this.Id = id;
            this.Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
