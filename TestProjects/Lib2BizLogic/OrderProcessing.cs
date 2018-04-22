using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SampleLibrary;
using Lib2BizLogic.Processing;

namespace Lib2BizLogic
{
    public class OrderProcessing
    {
        public void ProcessOrder(Order o)
        {
            var step1 = new Step1();
            var step2 = new Step2();


            step1.Apply(o);

            Order order = o;
            order = o;

            step2.Apply(order);
            step1.Apply(order);
        }
    }
}
