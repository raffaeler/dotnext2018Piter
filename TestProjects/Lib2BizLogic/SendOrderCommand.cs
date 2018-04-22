using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SampleLibrary;

namespace Lib2BizLogic
{
    public class SendOrderCommand
    {
        public void ProcessBusinessRules(Order order)
        {
            var processing = new OrderProcessing();
            processing.ProcessOrder(order);
        }
    }
}
