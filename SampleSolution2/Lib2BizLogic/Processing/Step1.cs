using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SampleLibrary;

namespace Lib2BizLogic.Processing
{
    public class Step1
    {
        public void Apply(Order order)
        {
            var discount = new DiscountRule();
            discount.Apply(order);
        }
    }
}
