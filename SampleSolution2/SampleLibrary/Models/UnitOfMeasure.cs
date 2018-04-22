using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class UnitOfMeasure
    {
        public UnitOfMeasure()
        {
        }

        public UnitOfMeasure(int id, string name)
            : this(id, name, name)
        {
        }

        public UnitOfMeasure(int id, string name, string shortSI)
        {
            this.Id = id;
            this.Name = name;
            this.ShortSI = shortSI;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortSI { get; set; }
    }
}
