using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SampleLibrary;
using SampleLibrary.Helpers;

namespace Lib2BizLogic
{
    public class Client
    {
        public void PrepareOrders()
        {
            var sendOrderCommand = new SendOrderCommand();

            var customer = ModelHelper.CreateCustomers().First();

            var order = ModelHelper.CreateOrders(10, 1, customer).Single();
            sendOrderCommand.ProcessBusinessRules(order);

            order = ModelHelper.CreateOrders(11, 1, customer).Single();
            sendOrderCommand.ProcessBusinessRules(order);
        }


    }
}
