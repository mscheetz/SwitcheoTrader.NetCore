using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public class CancelTradeParams
    {
        public string symbol { get; set; }
        public string Id { get; set; }
        public long timestamp { get; set; }
        public string type { get; set; }
    }
}
