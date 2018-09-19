# SwitcheoTrader.NetCore
Trading bot for Switcheo exchange

create walletConfig.json in the root directory of the application with your NEO wallet WIF (private key begining with an **L**):
```
{
  "wif": ""
}
```

create botSettings.json in the root directory of the application with bot settings:
```
{
  "botPassword": "",                    -- password for bot functionality
  "tradingPair": "SWTH_NEO",            -- trading pair
  "tradingStrategy": "OrderBook",       -- OrderBook, Volume
  "tradingStatus": "LiveTrading",       -- LiveTrading, PaperTrading
  "buyPercent": 0.25,                   -- % difference to trigger buy
  "sellPercent": 0.25,                  -- % difference to trigger sell
  "stopLoss": 0.0,                      -- % to set stop-loss
  "stopLossCheck": false,               -- check if stop loss set?
  "tradePercent": 100.0,                -- % of funds to trade
  "priceCheck": 60000,                  -- time to check updated price (ms)
  "chartInterval": "FiveM",             -- candlestick size OneM, FiveM, ...
  "startBotAutomatically": true,        -- Start the bot automatically?
  "startingAmount": 200.0,              -- PaperTrading starting amount (want amount, NEO in this example)
  "mooningTankingTime": 1500,           -- Time to check if price is mooning/tanking (ms)
  "mooningTankingPercent": 0.0,         -- % change to count as mooning/tanking
  "exchange": "SWITCHEO",               -- Exchange
  "lastBuy": 0.0,                       -- Last buy amount
  "lastSell": 0.0,                      -- last sell amount
  "tradingFee": 0.0,                    -- not currently used
  "tradeValidationCheck": 2000,         -- time to check if trade has been executed (ms)
  "runBot": true,                       -- run the bot?
  "orderBookQuantity": 0.15,            -- min volume at a price when looking for resistance/support (want volume)
  "traderResetInterval": 30,            -- # of cycles before re-validating settings
  "tradingCompetition": true,           -- trading competition?
  "tradingCompetitionEndTimeStamp": 0,  -- end of trading competition (unix timestamp)
  "openOrderTimeMS": 300000,            -- For order book trading, cancel an open trade if it has been open for this long (ms)
  "samePriceLimit": 2                   -- For order book trading, sell if price has been stagnant for this many cycles
}
```
## Trading Strategies
  * OrderBook: trading based on orderbook volume 
  * Volume: trading based on spikes in volume 

## Trading Status
  * PaperTrading: fake trading to test out trading settings 
  * LiveTrading: no explanation needed 
  
## Using app
  * Run locally in Debug mode using Visual Studio 2017 
  * Deploy to a server, I have been successful using AWS: Elastic Beanstalk
