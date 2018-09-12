using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public enum Strategy
    {
        None,
        BollingerBands,
        OrderBook,
        Percentage,
        Volume
    }
}
