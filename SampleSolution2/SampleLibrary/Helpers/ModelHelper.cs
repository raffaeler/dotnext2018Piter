using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary.Helpers
{
    public static class ModelHelper
    {
        public static Dictionary<string, UnitOfMeasure> Uoms =
            new Dictionary<string, UnitOfMeasure>()
        {
            { "Units", new UnitOfMeasure(1, "Units") },
        };

        public static IList<Customer> CreateCustomers()
        {
            var customers = new List<Customer>();
            var str = customers.ToString(); // demo
            Customer customer;

            customer = new Customer();
            customer.Id = 1;
            customer.Name = "Acme Inc";
            //customer = new Customer(1, "Acme Inc");

            customer.Orders.AddRange(CreateOrders(1, 10, customer));
            customers.Add(customer);

            customer = new Customer(1, "Jolly Corp");
            customer.Orders.AddRange(CreateOrders(1, 10, customer));
            customers.Add(customer);

            customer = new Customer(1, "Futile Gmbh");
            customer.Orders.AddRange(CreateOrders(1, 10, customer));
            customers.Add(customer);

            customer = new Customer(1, "Fried Chicken LLC");
            customer.Orders.AddRange(CreateOrders(1, 10, customer));
            customers.Add(customer);

            return customers;
        }


        public static IList<Order> CreateOrders(int startNum, int count, Customer customer)
        {
            Random rnd = new Random();
            var orders = new List<Order>();
            for (int i = startNum; i < startNum + count; i++)
            {
                var qtt = (decimal)rnd.NextDouble();
                var order = new Order(i, DateTimeOffset.Now.AddMinutes(rnd.Next(0, 2000) - 1000), $"Description_{i}", customer);
                order.OrderDetails.AddRange(CreateOrderDetails(1000, 5));
                orders.Add(order);
            }

            return orders;
        }


        public static IList<OrderDetail> CreateOrderDetails(int startNum, int count)
        {
            Random rnd = new Random();
            var details = new List<OrderDetail>();
            for (int i = startNum; i < startNum + count; i++)
            {
                var qtt = (decimal)rnd.NextDouble();
                var detail = new OrderDetail(i, $"ItemName_{i}", qtt);
                detail.UoM = Uoms["Units"];
                details.Add(detail);
            }

            return details;
        }


    }
}
