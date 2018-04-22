using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SampleLibrary;

namespace Lib1Analyzers
{
    public class CreateEntities
    {
        public Customer CreateCustomer1()
        {
            var customer = new Customer();
            customer.Id = 1;
            customer.Name = "Acme Inc";
            return customer;
        }

        public Customer CreateCustomer2()
        {
            var customer = new Customer()
            {
                Id = 1,
                Name = "Acme Inc",
            };

            return customer;
        }

        public Customer CreateCustomer3()
        {
            var customer = new Customer(1, "Acme Inc");
            return customer;
        }
    }
}
