using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public class OpenStopLoss
    {
        public string symbol { get; set; }
        public string id { get; set; }
        public decimal price { get; set; }
        public decimal quantity { get; set; }
    }
}
