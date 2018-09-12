using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public class OpenOrder
    {
        public long orderId { get; set; }
        public string clientOrderId { get; set; }
    }
}
