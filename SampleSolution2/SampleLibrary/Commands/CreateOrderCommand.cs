﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
    public class CreateOrderCommand : ICommand
    {
        public Customer Item { get; set; }
    }
}