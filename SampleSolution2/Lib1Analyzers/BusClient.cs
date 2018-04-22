using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SampleLibrary;

namespace Lib1Analyzers
{
    public class BusClient
    {
        public void NotifyOrderCreated()
        {
            var bus = BusManager.Instance;

            var evt = new OrderCreated();
            bus.Publish(evt);
            bus.Publish(evt);
        }

        public void RequestOrderCreation()
        {
            var bus = BusManager.Instance;

            var cmd = new CreateOrderCommand();
            bus.Send(cmd);
            bus.Publish(cmd);
        }
    }
}
