using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Entities
{
    public enum SignalType
    {
        None,
        Percent,
        Volume,
        BollingerBandUpper,
        BollingerBandLower,
        OrderBook
    }
}
