﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class OrderCreated : IEvent
    {
        public Customer Item { get; set; }
    }
}