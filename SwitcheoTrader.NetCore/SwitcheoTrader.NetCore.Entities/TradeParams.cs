using SwitcheoApi.NetCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public class TradeParams
    {
        public string symbol { get; set; }
        public Side side { get; set; }
        public string type { get; set; }
        public string timeInForce { get; set; }
        public decimal quantity { get; set; }
        public decimal price { get; set; }
        public decimal stopPrice { get; set; }
        public decimal icebergQty { get; set; }
        public long timestamp { get; set; }
        public bool useSWTH { get; set; }
    }
