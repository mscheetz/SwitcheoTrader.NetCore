﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public class OpenOrderDetail
    {
        public decimal price { get; set; }
        public long timestamp { get; set; }
    }
}
