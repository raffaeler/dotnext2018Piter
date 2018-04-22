using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class BusManager : IBusManager
    {
        public static readonly IBusManager Instance = new BusManager();

        public bool Publish(object eventObject)
        {
            throw new NotImplementedException();
        }

        public bool Send(object commandObject)
        {
            throw new NotImplementedException();
        }
    }
}
