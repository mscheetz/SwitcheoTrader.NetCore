﻿using DateTimeHelpers;
using Switcheo.NetCore;
using SwitcheoApi.NetCore.Entities;
using SwitcheoTrader.NetCore.Business.Interfaces;
using SwitcheoTrader.NetCore.Core;
using SwitcheoTrader.NetCore.Data;
using SwitcheoTrader.NetCore.Data.Interfaces;
using SwitcheoTrader.NetCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitcheoTrader.NetCore.Business
{
    public class TradeBuilder : ITradeBuilder
    {
        #region Private Members

        private IFileRepository _fileRepo;
        private SwitcheoApiClient _switcheo;
        private DateTimeHelper _dtHelper = new DateTimeHelper();
        private Helper _helper;
        private BotSettings _botSettings;
        private string _symbol;
        private string _asset;
        private string _pair;
        private List<BotBalance> _botBalances;
        private int _tradeNumber;
        private SignalType _signalType;
        private List<TradeInformation> _tradeInformation;
        private TradeInformation _lastTrade;
        private List<OpenOrder> _openOrderList;
        private List<OpenStopLoss> _openStopLossList;
        private decimal _lastBuy = 0.00000000M;
        private decimal _lastSell = 0.00000000M;
        private decimal _lastPrice = 0.00000000M;
        private decimal _lastQty = 0.00000000M;
        private Side _lastTradeType;
        private decimal _resistance = 0.00000000M;
        private decimal _support = 0.00000000M;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo">Repository interface</param>
        public TradeBuilder()
        {
            _fileRepo = new FileRepository();
            SetupBuilder();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo">Repository interface</param>
        public TradeBuilder(IFileRepository fileRepo)
        {
            _fileRepo = fileRepo;
            SetupBuilder();
        }

        /// <summary>
        /// Constructor for unit tests
        /// </summary>
        /// <param name="repo">Repository interface</param>
        public TradeBuilder(IFileRepository fileRepo, SwitcheoApiClient switcheo)
        {
            _fileRepo = fileRepo;
            _switcheo = switcheo;
            SetupBuilder();
        }

        /// <summary>
        /// Constructor for unit tests
        /// </summary>
        /// <param name="repo">Repository interface</param>
        public TradeBuilder(IFileRepository fileRepo, SwitcheoApiClient switcheo, List<BotBalance> botBalanceList)
        {
            _fileRepo = fileRepo;
            _switcheo = switcheo;
            SetupBuilder(botBalanceList);
        }

        #endregion Constructors

        #region Builder Setup

        private void SetupBuilder()
        {
            _helper = new Helper();
            _botSettings = GetBotSettings();
            _botBalances = new List<BotBalance>();
            _tradeInformation = new List<TradeInformation>();
            _openOrderList = new List<OpenOrder>();
            _openStopLossList = new List<OpenStopLoss>();
            SetupRepository();
        }

        /// <summary>
        /// Setup Builder for unit tests
        /// </summary>
        /// <param name="botBalanceList"></param>
        private void SetupBuilder(List<BotBalance> botBalanceList)
        {
            SetupBuilder();
            _botBalances = botBalanceList;
        }

        #endregion Builder Setup

        #region Settings Management

        /// <summary>
        /// Check if config file exists
        /// </summary>
        /// <returns>Boolean of result</returns>
        public bool ConfigFileExits()
        {
            return _fileRepo.ConfigExists();
        }

        /// <summary>
        /// Check if settings file exists
        /// </summary>
        /// <returns>Boolean of result</returns>
        public bool SettingsFileExists()
        {
            return _fileRepo.BotSettingsExists();
        }

        /// <summary>
        /// Get password
        /// </summary>
        /// <returns>String of password</returns>
        public string GetPassword()
        {
            return _botSettings.botPassword;
        }

        /// <summary>
        /// Update bot password
        /// </summary>
        /// <param name="password">String of new password</param>
        /// <returns>Bool when complete</returns>
        public bool UpdatePassword(string password)
        {
            _botSettings.botPassword = password;

            return _botSettings.botPassword.Equals(password);
        }

        /// <summary>
        /// Get Neo Address
        /// </summary>
        /// <returns>String of address</returns>
        public string GetNeoAddress()
        {
            return _switcheo.GetWallet().address;
        }

        /// <summary>
        /// Get BotSettings
        /// </summary>
        /// <returns>BotSettings object</returns>
        public BotSettings GetBotSettings()
        {
            if (_botSettings == null)
                LoadBotSettingsFile();

            return _botSettings;
        }

        /// <summary>
        /// Get BotConfig
        /// </summary>
        /// <returns>BotConfig object</returns>
        public BotConfig GetBotConfig()
        {
            var botSettings = GetBotSettings();

            return BotSettingsToBotConfig(botSettings);
        }

        private BotConfig BotSettingsToBotConfig(BotSettings botSettings)
        {
            var botConfig = new BotConfig
            {
                buyPercent = botSettings.buyPercent,
                chartInterval = botSettings.chartInterval,
                exchange = botSettings.exchange,
                lastBuy = botSettings.lastBuy,
                lastSell = botSettings.lastSell,
                mooningTankingPercent = botSettings.mooningTankingPercent,
                mooningTankingTime = botSettings.mooningTankingTime,
                openOrderTimeMS = botSettings.openOrderTimeMS,
                orderBookQuantity = botSettings.orderBookQuantity,
                priceCheck = botSettings.priceCheck,
                runBot = botSettings.runBot,
                samePriceLimit = botSettings.samePriceLimit,
                sellPercent = botSettings.sellPercent,
                startBotAutomatically = botSettings.startBotAutomatically,
                startingAmount = botSettings.startingAmount,
                stopLoss = botSettings.stopLoss,
                stopLossCheck = botSettings.stopLossCheck,
                tradePercent = botSettings.tradePercent,
                traderResetInterval = botSettings.traderResetInterval,
                tradeValidationCheck = botSettings.tradeValidationCheck,
                tradingCompetition = botSettings.tradingCompetition,
                tradingCompetitionEndTimeStamp = botSettings.tradingCompetitionEndTimeStamp,
                tradingFee = botSettings.tradingFee,
                tradingPair = botSettings.tradingPair,
                tradingStatus = botSettings.tradingStatus,
                tradingStrategy = botSettings.tradingStrategy
            };

            return botConfig;
        }

        private BotSettings BotConfigToBotSettings(BotConfig botConfig)
        {
            var botSettings = new BotSettings
            {
                buyPercent = botConfig.buyPercent,
                chartInterval = botConfig.chartInterval,
                exchange = botConfig.exchange,
                lastBuy = botConfig.lastBuy,
                lastSell = botConfig.lastSell,
                mooningTankingPercent = botConfig.mooningTankingPercent,
                mooningTankingTime = botConfig.mooningTankingTime,
                openOrderTimeMS = botConfig.openOrderTimeMS,
                orderBookQuantity = botConfig.orderBookQuantity,
                priceCheck = botConfig.priceCheck,
                runBot = botConfig.runBot,
                samePriceLimit = botConfig.samePriceLimit,
                sellPercent = botConfig.sellPercent,
                startBotAutomatically = botConfig.startBotAutomatically,
                startingAmount = botConfig.startingAmount,
                stopLoss = botConfig.stopLoss,
                stopLossCheck = botConfig.stopLossCheck,
                tradePercent = botConfig.tradePercent,
                traderResetInterval = botConfig.traderResetInterval,
                tradeValidationCheck = botConfig.tradeValidationCheck,
                tradingCompetition = botConfig.tradingCompetition,
                tradingCompetitionEndTimeStamp = botConfig.tradingCompetitionEndTimeStamp,
                tradingFee = botConfig.tradingFee,
                tradingPair = botConfig.tradingPair,
                tradingStatus = botConfig.tradingStatus,
                tradingStrategy = botConfig.tradingStrategy
            };

            return botSettings;
        }

        /// <summary>
        /// Load bot settings from disk
        /// </summary>
        public void LoadBotSettingsFile()
        {
            _botSettings = _fileRepo.GetSettings();
            _symbol = _botSettings.tradingPair;
            switch (_botSettings.tradingStrategy)
            {
                case Strategy.OrderBook:
                    _signalType = SignalType.OrderBook;
                    break;
                case Strategy.Percentage:
                    _signalType = SignalType.Percent;
                    break;
                case Strategy.Volume:
                    _signalType = SignalType.Volume;
                    break;
                default:
                    _signalType = SignalType.None;
                    break;
            }
            GetAssetAndPair();
        }

        /// <summary>
        /// Update bot settings from file
        /// </summary>
        /// <param name="_lastBuy">Last buy value</param>
        /// <param name="_lastSell">Last sell value</param>
        /// <returns>Boolean when complete</returns>
        public bool UpdateBotSettings(decimal _lastBuy, decimal _lastSell)
        {
            var settings = _fileRepo.GetSettings();
            settings.lastBuy = _lastBuy;
            settings.lastSell = _lastSell;
            _botSettings = settings;
            _symbol = settings.tradingPair;
            GetAssetAndPair();

            return true;
        }

        /// <summary>
        /// Set BotSettings in builder
        /// </summary>
        /// <param name="botConfig">Updated BotConfig values</param>
        /// <returns>Boolean when complete</returns>
        public bool SetBotSettings(BotConfig botConfig)
        {
            var settings = BotConfigToBotSettings(botConfig);

            return SetBotSettings(settings);
        }

        /// <summary>
        /// Set BotSettings in builder
        /// </summary>
        /// <param name="settings">Updated Settings</param>
        /// <returns>Boolean when complete</returns>
        public bool SetBotSettings(BotSettings settings)
        {
            var updatedSettings = new BotSettings
            {
                buyPercent = _botSettings.buyPercent,
                chartInterval = _botSettings.chartInterval,
                exchange = _botSettings.exchange,
                lastBuy = _botSettings.lastBuy,
                lastSell = _botSettings.lastSell,
                mooningTankingPercent = _botSettings.mooningTankingPercent,
                mooningTankingTime = _botSettings.mooningTankingTime,
                openOrderTimeMS = _botSettings.openOrderTimeMS,
                orderBookQuantity = _botSettings.orderBookQuantity,
                priceCheck = _botSettings.priceCheck,
                samePriceLimit = _botSettings.samePriceLimit,
                sellPercent = _botSettings.sellPercent,
                startBotAutomatically = _botSettings.startBotAutomatically,
                startingAmount = _botSettings.startingAmount,
                stopLoss = _botSettings.stopLoss,
                stopLossCheck = _botSettings.stopLossCheck,
                tradePercent = _botSettings.tradePercent,
                tradeValidationCheck = _botSettings.tradeValidationCheck,
                traderResetInterval = _botSettings.traderResetInterval,
                tradingCompetitionEndTimeStamp = _botSettings.tradingCompetitionEndTimeStamp,
                tradingFee = _botSettings.tradingFee,
                tradingPair = _botSettings.tradingPair,
                tradingStatus = _botSettings.tradingStatus,
                tradingStrategy = _botSettings.tradingStrategy,
                runBot = _botSettings.runBot
            };

            if (settings.buyPercent > 0)
                updatedSettings.buyPercent = settings.buyPercent;
            updatedSettings.chartInterval = settings.chartInterval;
            if (settings.exchange != Exchange.NONE)
                updatedSettings.exchange = settings.exchange;
            if (settings.lastBuy > 0)
                updatedSettings.lastBuy = settings.lastBuy;
            if (settings.lastSell > 0)
                updatedSettings.lastSell = settings.lastSell;
            if (settings.mooningTankingTime > 0)
                updatedSettings.mooningTankingTime = settings.mooningTankingTime;
            if (settings.orderBookQuantity > 0)
                updatedSettings.orderBookQuantity = settings.orderBookQuantity;
            if (settings.priceCheck > 0)
                updatedSettings.priceCheck = settings.priceCheck;
            if (settings.sellPercent > 0)
                updatedSettings.sellPercent = settings.sellPercent;
            if (settings.startBotAutomatically != null)
                updatedSettings.startBotAutomatically = settings.startBotAutomatically;
            if (settings.startingAmount > 0)
                updatedSettings.startingAmount = settings.startingAmount;
            if (settings.stopLoss > 0)
                updatedSettings.stopLoss = settings.stopLoss;
            if (settings.tradePercent > 0)
                updatedSettings.tradePercent = settings.tradePercent;
            if (settings.traderResetInterval > 0)
                updatedSettings.traderResetInterval = settings.traderResetInterval;
            if (settings.tradeValidationCheck > 0)
                updatedSettings.tradeValidationCheck = settings.tradeValidationCheck;
            if (!string.IsNullOrEmpty(settings.tradingPair))
                updatedSettings.tradingPair = settings.tradingPair;
            if (settings.tradingStatus != TradeStatus.None)
                updatedSettings.tradingStatus = settings.tradingStatus;
            if (settings.tradingStrategy != Strategy.None)
                updatedSettings.tradingStrategy = settings.tradingStrategy;
            if (!string.IsNullOrEmpty(settings.botPassword))
                updatedSettings.botPassword = settings.botPassword;

            updatedSettings.mooningTankingPercent = settings.mooningTankingPercent;
            updatedSettings.runBot = settings.runBot;
            updatedSettings.samePriceLimit = settings.samePriceLimit;
            updatedSettings.stopLossCheck = settings.stopLossCheck;
            updatedSettings.tradingCompetition = settings.tradingCompetition;
            updatedSettings.tradingCompetitionEndTimeStamp = settings.tradingCompetitionEndTimeStamp;
            updatedSettings.tradingFee = settings.tradingFee;

            _fileRepo.UpdateBotSettings(updatedSettings);
            _botSettings = updatedSettings;
            _symbol = _botSettings.tradingPair;
            GetAssetAndPair();

            return true;
        }

        /// <summary>
        /// Set up respository
        /// </summary>
        /// <returns>Boolean when complete</returns>
        public bool SetupRepository()
        {
            if (_switcheo != null)
            {
                var wallet = _switcheo.GetWallet();

                if (wallet.privateKey != null)
                    return true;

            }
            var apiInfo = GetApiInformation();

            _switcheo = new SwitcheoApiClient(apiInfo.wif);

            return true;
        }

        /// <summary>
        /// Set api information
        /// </summary>
        /// <param name="apiInformation">Updated ApiInformation</param>
        /// <returns>Boolean when complete</returns>
        public bool SetApiInformation(ApiInformation apiInformation)
        {
            var result = _fileRepo.SetConfig(apiInformation);

            return result;
        }

        /// <summary>
        /// Get NEO address from wallet
        /// </summary>
        /// <returns>String of NEO address</returns>
        public string GetAddress()
        {
            var wallet = _switcheo.GetWallet();

            return wallet.address != null ? wallet.address : string.Empty;
        }

        /// <summary>
        /// Get ApiInformation from config
        /// </summary>
        /// <returns>ApiInformation for repository</returns>
        private ApiInformation GetApiInformation()
        {
            var config = _fileRepo.GetConfig();

            var apiInfo = new ApiInformation
            {
                wif = config.wif
            };

            return apiInfo;
        }

        /// <summary>
        /// Update balances and get initial trade type
        /// </summary>
        /// <returns>TradeType value</returns>
        public TradeType GetInitialTradeType()
        {
            return GetTradingType(true);
        }

        /// <summary>
        /// Update balances and get current trade type
        /// </summary>
        /// <param name="logBalances">Write balances to log?</param>
        /// <returns>TradeType value</returns>
        public TradeType GetTradingType(bool logBalances = false)
        {
            SetBalances(logBalances);
            var assetQty = _botBalances.Where(b => b.symbol.Equals(_asset)).Select(b => b.quantity).FirstOrDefault();
            var pairQty = _botBalances.Where(b => b.symbol.Equals(_pair)).Select(b => b.quantity).FirstOrDefault();

            if (((_pair == "USD" || _pair == "USDT")
                && pairQty < 10.0M)
                || ((_pair == "BTC" || _pair == "ETH")
                && pairQty < 0.0002M))
            {
                if (((_asset == "BTC" || _asset == "ETH") && assetQty < 0.0002M)
                    || assetQty < 0.5M)
                {
                    return TradeType.NONE;
                }
                else
                {
                    return TradeType.SELL;
                }
            }
            else
            {
                return TradeType.BUY;
            }
        }

        #endregion Settings Management

        #region Trade History

        /// <summary>
        /// Get all transactions
        /// </summary>
        /// <param name="transactionCount">Count of transactions to return</param>
        /// <returns>Collection of TradeInformation</returns>
        public IEnumerable<TradeInformation> GetTradeHistory(int transactionCount)
        {
            var tradeList = _fileRepo.GetTransactions();

            return tradeList.Skip(Math.Max(0, tradeList.Count - transactionCount));
        }

        #endregion Trade History

        #region Signal History

        /// <summary>
        /// Get all trade signals
        /// </summary>
        /// <param name="signalCount">Count of trade signals to return</param>
        /// <returns>Collection of TradeSignal objects</returns>
        public IEnumerable<TradeSignal> GetSignalHistory(int signalCount)
        {
            var signalList = _fileRepo.GetSignals();

            return signalList.Skip(Math.Max(0, signalList.Count - signalCount));
        }

        #endregion Signal History

        #region Balance Managers

        /// <summary>
        /// Get current balance
        /// </summary>
        /// <param name="recordsToReturn">Number of records to return (default 1)</param>
        /// <returns>BotBalance object</returns>
        public IEnumerable<List<BotBalance>> GetBalance(int recordsToReturn = 1)
        {
            var balanceList = _fileRepo.GetBalances();

            if (recordsToReturn == 0)
            {
                return balanceList;
            }

            return balanceList.Skip(Math.Max(0, balanceList.Count - recordsToReturn));
        }

        /// <summary>
        /// Get Balance history
        /// </summary>
        /// <returns>Collection of BotBalance objects</returns>
        public IEnumerable<IEnumerable<BotBalance>> GetBalanceHistory()
        {
            return _fileRepo.GetBalances();
        }

        /// <summary>
        /// Get current balances
        /// </summary>
        /// <returns>Collection of BotBallance objects</returns>
        public List<BotBalance> GetBotBalance()
        {
            return _botBalances;
        }

        /// <summary>
        /// Set balances
        /// </summary>
        /// <param name="logBalance">Log the balance bool</param>
        public void SetBalances(bool logBalance = true)
        {
            _botBalances = new List<BotBalance>();

            var balances = GetBalances(_botSettings.startingAmount);

            for (var i = 0; i < balances.Count; i++)
            {
                var botBalance = new BotBalance
                {
                    symbol = balances[i].asset,
                    quantity = balances[i].free + balances[i].locked,
                    timestamp = DateTime.UtcNow
                };

                _botBalances.Add(botBalance);
            }

            if (logBalance)
            {
                LogBalances();
            }
        }

        /// <summary>
        /// Get paper balances available
        /// </summary>
        /// <param name="startingAmount">Starting amount for paper trading (default = 0)</param>
        /// <returns>Collection of balance objects</returns>
        public List<Balance> GetPaperBalances(decimal startingAmount = 0)
        {
            var symbol = _botSettings.tradingPair;

            var balances = new List<Balance>();
            decimal pairQuantity = 0;
            decimal assetQuantity = 0;

            if (startingAmount > 0)
            {
                pairQuantity = startingAmount;
            }
            else
            {
                if (_lastTradeType == Side.buy)
                {
                    pairQuantity = 0;
                    assetQuantity = _lastQty;
                }
                else if (_lastTradeType == Side.sell)
                {
                    pairQuantity = _lastQty;
                    assetQuantity = 0;
                }
            }

            balances.Add(
                new Balance
                {
                    asset = _pair,
                    free = pairQuantity,
                    locked = 0
                });

            balances.Add(
                new Balance
                {
                    asset = _asset,
                    free = assetQuantity,
                    locked = 0
                });

            return balances;
        }

        /// <summary>
        /// Get balances available
        /// </summary>
        /// <param name="startingQuantity">Starting quantity (for paper trading, default 0)</param>
        /// <returns>Collection of balance objects</returns>
        public List<Balance> GetBalances(decimal startingQuantity = 0M)
        {
            var balances = new List<Balance>();

            if (_botSettings.tradingStatus == TradeStatus.LiveTrading)
            {
                var exchangeBalances = _switcheo.GetBalances();

                foreach(var item in exchangeBalances.confirmed)
                {
                    if (item.Key.Equals(_asset) || item.Key.Equals(_pair))
                    {
                        var balance = new Balance
                        {
                            asset = item.Key,
                            free = item.Value
                        };
                        balances.Add(balance);
                    }
                }
                foreach (var item in exchangeBalances.confirming)
                {
                    if (item.Key.Equals(_asset) || item.Key.Equals(_pair))
                    {
                        var locked = item.Value[0].amount;
                        var balance = balances.Where(b => b.asset == item.Key).FirstOrDefault();
                        if (balance != null)
                        {
                            balance.locked = locked;
                        }
                        else
                        {
                            balance = new Balance
                            {
                                asset = item.Key,
                                locked = item.Value[0].amount
                            };

                            balances.Add(balance);
                        }
                    }
                }
                foreach (var item in exchangeBalances.locked)
                {
                    if (item.Key.Equals(_asset) || item.Key.Equals(_pair))
                    {
                        var locked = item.Value;
                        var balance = balances.Where(b => b.asset == item.Key).FirstOrDefault();
                        if (balance != null)
                        {
                            balance.locked += locked;
                        }
                        else
                        {
                            balance = new Balance
                            {
                                asset = item.Key,
                                locked = item.Value
                            };

                            balances.Add(balance);
                        }
                    }
                }
            }
            else if (_botSettings.tradingStatus == TradeStatus.PaperTrading)
            {
                balances = GetPaperBalances(startingQuantity);
            }
            return balances;
        }

        /// <summary>
        /// Update balances from exchange
        /// </summary>
        public void UpdateBalances()
        {
            _botBalances = new List<BotBalance>();

            var balances = GetBalances();

            for (var i = 0; i < balances.Count; i++)
            {
                var botBalance = new BotBalance
                {
                    symbol = balances[i].asset,
                    quantity = balances[i].free + balances[i].locked,
                    timestamp = DateTime.UtcNow
                };

                _botBalances.Add(botBalance);
            }

            LogBalances();
        }

        #endregion Balance Managers

        #region Logging
        /// <summary>
        /// Log balances to file
        /// </summary>
        /// <returns>Boolean when complete</returns>
        public bool LogBalances()
        {
            return _fileRepo.LogBalances(_botBalances);
        }

        /// <summary>
        /// Log Trades
        /// </summary>
        /// <param name="tradeInformation">New TradeInformation object</param>
        /// <returns></returns>
        public bool LogTransaction(TradeInformation tradeInformation)
        {
            return _fileRepo.LogTransaction(tradeInformation);
        }

        /// <summary>
        /// Write a signal to file
        /// </summary>
        /// <param name="signal">Signal to log</param>
        /// <returns>Boolean when complete</returns>
        public bool LogTradeSignal(SignalType signalType, TradeType tradeType, decimal price, decimal volume = 0M)
        {
            var signal = new TradeSignal
            {
                bandLower = 0M,
                bandUpper = 0M,
                currentVolume = volume,
                lastBuy = _lastBuy,
                lastSell = _lastSell,
                pair = _symbol,
                price = price,
                signal = signalType,
                tradeType = tradeType,
                transactionDate = DateTime.UtcNow
            };
            return _fileRepo.LogSignal(signal);
        }

        #endregion Logging

        #region Candlesticks

        /// <summary>
        /// Get Candlesticks from exchange API
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="interval">Candlestick Interval</param>
        /// <param name="range">Number of candlesticks to get</param>
        /// <returns>Array of BotStick objects</returns>
        public Candlstick[] GetCandlesticks(string symbol, Interval interval, int range)
        {
            var candleSticks = _switcheo.GetCandlesticks(symbol, interval, 0, range);

            while (candleSticks == null || candleSticks.Count() == 0)
            {
                candleSticks = GetCandlesticks(symbol, interval, range);
            }

            return candleSticks;
        }

        #endregion Candlesticks

        #region OrderBook

        /// <summary>
        /// Get order book position of a price
        /// </summary>
        /// <param name="price">Decimal of price to find</param>
        /// <returns>Int of position in order book</returns>
        public int? GetPricePostion(decimal price)
        {
            int i = 0;
            var orderBook = _switcheo.GetOffers(_symbol);

            while (orderBook == null && i < 3)
            {
                Task.WaitAll(Task.Delay(1000));
                orderBook = _switcheo.GetOffers(_symbol);
                i++;
            }

            // TODO: FIX THIS ON SWITCHEO API
            //if (price >= orderBook.asks[0].price)
            //{
            //    for (i = 0; i < orderBook.asks.Length; i++)
            //    {
            //        if (price <= orderBook.asks[i].price)
            //        {
            //            return i;
            //        }
            //    }
            //}
            //else if (price <= orderBook.bids[0].price)
            //{
            //    for (i = 0; i < orderBook.bids.Length; i++)
            //    {
            //        if (price >= orderBook.bids[i].price)
            //        {
            //            return i;
            //        }

            //    }
            //}
            return null;
        }


        /// <summary>
        /// Get 1st price with the most support at or above specified volume
        /// </summary>
        /// <param name="symbol">String of trading pair</param>
        /// <param name="volume">Volume to set buy price</param>
        /// <returns>Decimal of price</returns>
        private OrderBookDetail OnGetSupport(string symbol, decimal volume)
        {
            decimal support = 0.00000000M;
            int i = 0;
            int precision = 0;
            OrderBookDetail response;
            var orderBook = _switcheo.GetOrderBook(symbol);

            while (orderBook == null && i < 3)
            {
                Task.WaitAll(Task.Delay(1000));
                orderBook = _switcheo.GetOrderBook(symbol);
                i++;
            }

            if (_botSettings.tradingCompetition)
            {
                var obPrice = orderBook.bids[i].price;
                var trimedPrice = obPrice.ToString().TrimEnd('0');
                var price = Convert.ToDecimal(trimedPrice);
                var thisPrecision = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
                response = new OrderBookDetail
                {
                    price = price,
                    precision = thisPrecision,
                    position = 0
                };

                return response;
            }
            var staleMate = StaleMateCheck(orderBook.bids.Take(2).ToArray(), orderBook.asks.Take(2).ToArray(), volume);

            if (!staleMate)
            {
                for (i = 0; i < orderBook.bids.Length; i++)
                {
                    var obPrice = orderBook.bids[i].price;
                    var trimedPrice = obPrice.ToString().TrimEnd('0');
                    var price = Convert.ToDecimal(trimedPrice);
                    var thisPrecision = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
                    precision = thisPrecision > precision ? thisPrecision : precision;
                    var vol = orderBook.bids[i].price * orderBook.bids[i].want_amount;

                    if (vol >= volume)
                    {
                        support = price;
                        break;
                    }
                }
            }
            response = new OrderBookDetail
            {
                price = support,
                precision = precision,
                position = i
            };

            return response;
        }

        /// <summary>
        /// Get 1st price with the most resistance at or above specified volume
        /// </summary>
        /// <param name="symbol">String of trading pair</param>
        /// <param name="volume">Volume to set buy price</param>
        /// <returns>Decimal of price</returns>
        private OrderBookDetail OnGetResistance(string symbol, decimal volume)
        {
            decimal resistance = 0.00000000M;
            OrderBookDetail response;
            int i = 0;
            int precision = 0;
            var orderBook = _switcheo.GetOrderBook(symbol);
            while (orderBook == null && i < 3)
            {
                Task.WaitAll(Task.Delay(1000));
                orderBook = _switcheo.GetOrderBook(symbol);
                i++;
            }

            if (_botSettings.tradingCompetition)
            {
                var obPrice = orderBook.asks[i].price;
                var trimedPrice = obPrice.ToString().TrimEnd('0');
                var price = Convert.ToDecimal(trimedPrice);
                var thisPrecision = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
                response = new OrderBookDetail
                {
                    price = price,
                    precision = thisPrecision,
                    position = 0
                };

                return response;
            }

            var staleMate = StaleMateCheck(orderBook.bids.Take(2).ToArray(), orderBook.asks.Take(2).ToArray(), volume);

            if (!staleMate)
            {
                for (i = 0; i < orderBook.asks.Length; i++)
                {
                    var obPrice = orderBook.asks[i].price;
                    var trimedPrice = obPrice.ToString().TrimEnd('0');
                    var price = Convert.ToDecimal(trimedPrice);
                    var thisPrecision = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
                    precision = thisPrecision > precision ? thisPrecision : precision;
                    var vol = orderBook.asks[i].price * orderBook.asks[i].want_amount;

                    if (vol >= volume)
                    {
                        resistance = orderBook.asks[i].price;
                        break;
                    }
                }
            }
            response = new OrderBookDetail
            {
                price = resistance,
                precision = precision,
                position = i
            };

            return response;
        }

        /// <summary>
        /// Compare 1st 2 SwitcheoOrder objects, if one or both at volume, stale mate is reached
        /// </summary>
        /// <param name="buys">Top Buy orders</param>
        /// <param name="sells">Bottom Sell orders</param>
        /// <param name="volume">Volume to trigger</param>
        /// <returns>Boolean if stale mate reached</returns>
        public bool StaleMateCheck(SwitcheoOffer[] buys, SwitcheoOffer[] sells, decimal volume)
        {
            if (_botSettings.tradingCompetition)
            {
                return false;
            }
            else if ((buys[0].price * buys[0].want_amount >= volume
                || buys[1].price * buys[1].want_amount >= volume)
                && (sells[0].price * sells[0].want_amount >= volume
                || sells[1].price * sells[1].want_amount >= volume))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Get next resistance level if within 3 spots of bottom resistance
        /// </summary>
        /// <param name="getNew">Boolean to get a new value</param>
        /// <returns>Decimal of next resistance</returns>
        public decimal GetResistance(bool getNew = false)
        {
            if (getNew)
            {
                var detail = OnGetResistance(_symbol, _botSettings.orderBookQuantity);
                var resistance = detail.price;
                var places = detail.position;
                var limit = _botSettings.tradingCompetition ? 1 : 3;
                var sellPrice = places == 0
                    ? resistance
                    : resistance - _helper.DecimalValueAtPrecision(detail.precision);

                _resistance = places <= limit ? sellPrice : 0.00000000M; ;
            }

            return _resistance;
        }

        /// <summary>
        /// Get next support level if within 3 spots of top support level
        /// </summary>
        /// <param name="getNew">Boolean to get a new value</param>
        /// <returns>Decimal of next support</returns>
        public decimal GetSupport(bool getNew = false)
        {
            if (getNew)
            {
                var detail = OnGetSupport(_symbol, _botSettings.orderBookQuantity);
                var support = detail.price;
                var places = detail.position;
                var limit = _botSettings.tradingCompetition ? 1 : 3;
                var buyPrice = places == 0
                    ? support
                    : support + _helper.DecimalValueAtPrecision(detail.precision);

                _support = places <= limit ? buyPrice : 0.00000000M;
            }

            return _support;
        }

        #endregion OrderBook

        #region Place Trade

        /// <summary>
        /// Place a trade
        /// </summary>
        /// <param name="tradeParams">Trade parameters</param>
        /// <returns>Order object</returns>
        public Order PlaceTrade(TradeParams tradeParams)
        {
            _tradeNumber++;
            if (_botSettings.tradingStatus == TradeStatus.LiveTrading)
                return _switcheo.PlaceOrder(tradeParams.symbol, tradeParams.side, tradeParams.price, tradeParams.quantity, tradeParams.useSWTH);
            else if (_botSettings.tradingStatus == TradeStatus.PaperTrading)
                return PlacePaperTrade(tradeParams);
            else
                return null;
        }

        /// <summary>
        /// Place a paper trade for testing purposes
        /// </summary>
        /// <param name="tradeParams">Trade parameters</param>
        /// <returns>Order object</returns>
        private Order PlacePaperTrade(TradeParams tradeParams)
        {
            var response = new Order
            {
                id = $"PaperTrade_{_tradeNumber}",
                offer_amount = tradeParams.quantity.ToString(),
                offer_asset_id = _pair,               
                want_amount = tradeParams.quantity.ToString(),
                want_asset_id = _asset,
                status = "processed",
                order_status = "completed"
            };

            return response;
        }

        #endregion Place Trade

        #region Stop Loss Management

        /// <summary>
        /// Check if Stop Loss Hit
        /// </summary>
        /// <param name="currentPrice">Current price of coin</param>
        /// <returns>Nullable decimal value of stop loss</returns>
        public decimal? StoppedOutCheck(decimal currentPrice)
        {
            if (_openStopLossList.Count == 0 || currentPrice >= _openStopLossList[0].price)
                return null;

            var trade = new Order
            {
                id = _openStopLossList[0].id
            };

            var stoppedOut = CheckTradeStatus(trade);

            if (stoppedOut)
            {
                ProcessStopLoss();
            }

            return _lastSell;
        }

        /// <summary>
        /// Process a stop loss
        /// </summary>
        /// <returns>Boolean when complete</returns>
        public bool ProcessStopLoss()
        {
            _lastSell = _openStopLossList[0].price;
            var slQty = _openStopLossList[0].quantity;
            _openStopLossList.RemoveAt(0);

            _lastTrade = new TradeInformation
            {
                pair = _symbol,
                price = _lastSell,
                quantity = slQty,
                timestamp = DateTime.UtcNow,
                tradeType = EnumHelper.GetEnumDescription((TradeType)TradeType.STOPLOSS)
            };
            _tradeInformation.Add(_lastTrade);

            LogTransaction(_lastTrade);

            _lastQty = GetTradeQuantity(Side.sell, _lastSell);

            UpdateBalances();

            return true;
        }

        #endregion Stop Loss Management

        #region Buy/Sell

        /// <summary>
        /// Buy crypto
        /// </summary>
        /// <param name="orderPrice">Buy price</param>
        /// <param name="tradeType">Trade Type</param>
        /// <param name="stopLoss">Place stoploss? default false</param>
        /// <param name="validateTrade">Validated trade complete, default = true</param>
        /// <returns>Boolean when complete</returns>
        public bool BuyCrypto(decimal orderPrice, TradeType tradeType, bool stopLoss = false, bool validateTrade = true)
        {
            var tradeComplete = false;
            int i = 0;
            Order trade = null;
            while (!tradeComplete && i < 2)
            {
                trade = MakeTrade(Side.buy, orderPrice);

                if (trade == null || trade.id == null)
                {
                    return false;
                }

                if (validateTrade)
                {
                    tradeComplete = ValidateTradeComplete(trade);
                }
                else
                {
                    tradeComplete = true;
                }

                if (i == 1 && !tradeComplete)
                {
                    return false;
                }

                i++;
                if (!tradeComplete) // If trade was not filled, try at a lower price
                {
                    orderPrice = orderPrice - 0.01M;
                }
            }

            if (!validateTrade)
            {
                return true;
            }

            UpdateBalances();

            var quantity = 0M;
            Decimal.TryParse(trade.offer_amount, out quantity);

            CaptureTransaction(orderPrice, quantity, trade.created_at.ToUnixTimeMilliseconds(), tradeType);

            _lastBuy = orderPrice;

            if (stopLoss)
            {
                var stopLossResponse = PlaceStopLoss(orderPrice, quantity);
            }

            return CheckTradeSuccess(TradeType.BUY);
        }

        /// <summary>
        /// Sell crypto
        /// </summary>
        /// <param name="orderPrice">Current price</param>
        /// <param name="tradeType">Trade Type</param>
        /// <param name="validateTrade">Validate trade is complete, default = true</param>
        /// <returns>Boolean when complete</returns>
        public bool SellCrypto(decimal orderPrice, TradeType tradeType, bool validateTrade = true)
        {
            var tradeComplete = false;
            int i = 0;
            Order trade = null;
            while (!tradeComplete && i < 2)
            {
                CancelStopLoss();

                trade = MakeTrade(Side.sell, orderPrice);

                if (trade == null || trade.id == null)
                {
                    return false;
                }

                if (validateTrade)
                {
                    tradeComplete = ValidateTradeComplete(trade);
                }
                else
                {
                    tradeComplete = true;
                }

                if (tradeComplete)
                {
                    break;
                }
                else if (i == 1 && !tradeComplete)
                {
                    return false;
                }

                i++;
                if (!tradeComplete) // If trade was not filled, try at a higher price
                {
                    orderPrice = orderPrice + 0.01M;
                }
            }
            if (!validateTrade)
            {
                return true;
            }

            UpdateBalances();

            var quantity = 0M;
            Decimal.TryParse(trade.offer_amount, out quantity);

            CaptureTransaction(orderPrice, quantity, trade.created_at.ToUnixTimeMilliseconds(), tradeType);

            _lastSell = orderPrice;

            return CheckTradeSuccess(TradeType.SELL);
        }

        /// <summary>
        /// Check success of trade based on updated balances
        /// </summary>
        /// <param name="type">TradeType executed</param>
        /// <returns>Boolean if trade was successful</returns>
        public bool CheckTradeSuccess(TradeType type)
        {
            var pairBalance = _botBalances.Where(b => b.symbol.Equals(_pair)).FirstOrDefault();
            var assetBalance = _botBalances.Where(b => b.symbol.Equals(_asset)).FirstOrDefault();
            if (pairBalance == null)
            {
                return false;
            }
            if (type == TradeType.BUY)
            {
                return pairBalance.quantity < 10.0M ? true : false;
            }
            else
            {
                return pairBalance.quantity > 10.0M ? true : false;
            }
        }

        /// <summary>
        /// Capture the current transaction and log it
        /// </summary>
        /// <param name="price">Transaction price</param>
        /// <param name="quantity">Transaction quantity</param>
        /// <param name="timeStamp">Transaction time</param>
        /// <param name="tradeType">Transaction TradeType</param>
        /// <returns>Boolean when complete</returns>
        public bool CaptureTransaction(decimal price, decimal quantity, long timeStamp, TradeType tradeType)
        {
            _lastTrade = new TradeInformation
            {
                pair = _symbol,
                price = price,
                quantity = quantity,
                timestamp = DateTime.UtcNow,
                tradeType = EnumHelper.GetEnumDescription((TradeType)tradeType)
            };

            _tradeInformation.Add(_lastTrade);

            LogTransaction(_lastTrade);

            return true;
        }

        #endregion Buy/Sell

        #region Validate Trade

        /// <summary>
        /// Validate that a placed trade was complete
        /// </summary>
        /// <param name="trade">TradeReponse object</param>
        /// <returns>Boolean value on trade validation</returns>
        public bool ValidateTradeComplete(Order trade)
        {
            bool tradeComplete = false;
            int i = 0;
            while (!tradeComplete || i > 2)
            {
                i++;
                tradeComplete = CheckTradeStatus(trade);
                if (tradeComplete)
                {
                    break;
                }
                if (!tradeComplete && i < 2)
                {
                    Task.WaitAll(Task.Delay(_botSettings.tradeValidationCheck));
                }
                else if (!tradeComplete && i == 2)
                {
                    var cancelTradeParams = new CancelTradeParams
                    {
                        Id = trade.id,
                        symbol = _symbol,
                        timestamp = trade.created_at.ToUnixTimeMilliseconds(),
                        type = trade.side.ToString()
                    };
                    CancelTrade(cancelTradeParams);

                    return false;
                }
            }

            return tradeComplete;
        }

        /// <summary>
        /// Check status of placed trade
        /// </summary>
        /// <param name="orderId">OrderId of trade</param>
        /// <returns>Boolean value of filled status</returns>
        public bool CheckTradeStatus(Order trade)
        {
            var orderStatus = GetOrderStatus(trade);

            if (orderStatus == null)
                return false;

            return orderStatus.order_status.Equals("completed") ? true : false;
        }

        #endregion Validate Trade

        #region Pair and Quantity

        /// <summary>
        /// Get current asset
        /// </summary>
        /// <returns>String of asset</returns>
        public string GetAsset()
        {
            return _asset;
        }

        /// <summary>
        /// Get current trading pair
        /// </summary>
        /// <returns>String of pair</returns>
        public string GetPair()
        {
            return _pair;
        }

        /// <summary>
        /// Get Asset and Pair from symbol
        /// </summary>
        public void GetAssetAndPair()
        {
            if (_symbol.Contains("USDT"))
            {
                _asset = _symbol.Replace("USDT", "");
                _pair = "USDT";
            }
            else if (_symbol.Contains("USD"))
            {
                _asset = _symbol.Replace("USD", "");
                _pair = "USD";
            }
            else if (_symbol.Contains("BTC"))
            {
                _asset = _symbol.Replace("BTC", "");
                _pair = "BTC";
            }
            else if (_symbol.Contains("ETH"))
            {
                _asset = _symbol.Replace("ETH", "");
                _pair = "ETH";
            }
            else if (_symbol.Contains("NEO"))
            {
                _asset = _symbol.Replace("NEO", "");
                _pair = "NEO";
            }
            else if (_symbol.Contains("BNB"))
            {
                _asset = _symbol.Replace("BNB", "");
                _pair = "BNB";
            }
        }

        /// <summary>
        /// Get quantity to trade based
        /// </summary>
        /// <param name="side">Trade Side</param>
        /// <param name="orderPrice">Requested trade price</param>
        /// <returns>decimal of quantity to purchase</returns>
        public decimal GetTradeQuantity(Side side, decimal orderPrice)
        {
            int roundTo = 1;
            decimal quantity = 0.00000000M;
            if (side == Side.buy)
            {
                var pairBalance = _botBalances.Where(b => b.symbol.Equals(_pair)).FirstOrDefault();

                quantity = pairBalance.quantity / orderPrice;
            }
            else if (side == Side.sell)
            {
                var symbolBalance = _botBalances.Where(b => b.symbol.Equals(_asset)).FirstOrDefault();

                quantity = symbolBalance.quantity;
            }


            decimal roundedDown = _helper.RoundDown(quantity, roundTo);

            return roundedDown;
        }

        #endregion Pair and Quantity

        #region Moon and Tank Check

        /// <summary>
        /// Check if mooning
        /// </summary>
        /// <param name="startingPrice">decimal of starting price</param>
        /// <param name="prevStick">Previous Candlstick (default null)</param>
        /// <param name="iteration">Int of iteration</param>
        /// <returns>decimal of sell price</returns>
        public decimal OrderBookSellCheck(decimal startingPrice = 0.00000000M
                                    , Candlstick prevStick = null
                                    , int iteration = 0)
        {
            var checkToSell = true;
            decimal sellPrice = 0.00000000M;
            while (checkToSell)
            {
                var sticks = GetNextCandlestick();
                var currentStick = sticks[0];
                var lastStick = sticks[1];

                if (prevStick == null)
                {
                    prevStick = lastStick;
                }

                if ((startingPrice > 0.00000000M && startingPrice < currentStick.close)
                    && currentStick.open < currentStick.close
                    && prevStick.close < currentStick.close)
                {
                    // TODO: set the latest price as the starting price to see if it is increasing during current candle
                    // TODO: do same on tanking
                    // If current price is greater than the previous check 
                    //  (price is increasing)
                    // and sell percent reached
                    // keep checking if increasing more
                    iteration++;
                    //sellPrice = OrderBookSellCheck(startingPrice, lastStick, iteration);
                    prevStick = lastStick;
                }
                else
                {
                    sellPrice = iteration == 0 ? startingPrice : currentStick.close;
                    checkToSell = false;
                }
            }

            return sellPrice;
        }

        /// <summary>
        /// Check if tanking
        /// </summary>
        /// <param name="startingPrice">decimal of starting price</param>
        /// <param name="prevStick">Previous Candlstick (default null)</param>
        /// <param name="iteration">Int of iteration</param>
        /// <returns>decimal of buy price</returns>
        public decimal OrderBookBuyCheck(decimal startingPrice = 0.00000000M
                                    , Candlstick prevStick = null
                                    , int iteration = 0)
        {
            var checkToBuy = true;
            decimal buyPrice = 0.00000000M;
            while (checkToBuy)
            {
                var sticks = GetNextCandlestick();

                var currentStick = sticks[0];
                var lastStick = sticks[1];

                if (prevStick == null)
                {
                    prevStick = lastStick;
                }

                if ((startingPrice > 0.00000000M && startingPrice > currentStick.close)
                    && currentStick.open > currentStick.close
                    && lastStick.close > currentStick.close
                    && prevStick.close > currentStick.close)
                {
                    // If current price is less than the previous check 
                    //  (price is dropping)
                    // and buy percent reached
                    // keep checking if dropping more
                    iteration++;
                    //buyPrice = OrderBookBuyCheck(startingPrice, lastStick, iteration);
                    prevStick = lastStick;
                }
                else
                {
                    buyPrice = iteration == 0 ? startingPrice : currentStick.close;
                    checkToBuy = false;
                }
            }
            return buyPrice;
        }

        /// <summary>
        /// Get next candlestick
        /// </summary>
        /// <param name="interval">Trade interval, default 1 minute</param>
        /// <param name="stickCount">Int of sticks to return, default 2</param>
        /// <returns>Candlestick object</returns>
        public Candlstick[] GetNextCandlestick()
        {
            Task.WaitAll(Task.Delay(_botSettings.mooningTankingTime));

            var candlesticks = GetCandlesticks(_symbol, _botSettings.chartInterval, 2);

            return candlesticks;
        }

        #endregion Moon and Tank Check

        #region Stop Loss Management

        /// <summary>
        /// Get all open stop losses
        /// </summary>
        /// <returns>Collection of OpenStopLoss</returns>
        public IEnumerable<OpenStopLoss> GetStopLosses()
        {
            return _openStopLossList;
        }

        /// <summary>
        /// Cancel a stop loss
        /// </summary>
        /// <returns>Boolean value when complete</returns>
        public bool CancelStopLoss()
        {
            if (_openStopLossList == null || _openStopLossList.Count == 0)
                return true;

            var tradeParams = new CancelTradeParams
            {
                symbol = _symbol,
                Id = _openStopLossList[0].id,
                type = "SELL"
            };

            var result = CancelTrade(tradeParams);

            var trade = new Order
            {
                id = _openStopLossList[0].id,
            };
            bool stopLossCanceled = false;
            while (!stopLossCanceled)
            {
                stopLossCanceled = CheckTradeStatus(trade);
            }

            _openStopLossList.RemoveAt(0);

            return stopLossCanceled;
        }

        /// <summary>
        /// Place a stop loss
        /// </summary>
        /// <param name="orderPrice">Trade price</param>
        /// <param name="quantity">Quantity to trade</param>
        /// <returns>Order object</returns>
        public Order PlaceStopLoss(decimal orderPrice, decimal quantity)
        {
            decimal stopLossPercent = (decimal)Math.Abs(_botSettings.stopLoss) / 100;
            decimal stopLossPrice = orderPrice - (orderPrice * stopLossPercent);

            var trade = new TradeParams
            {
                symbol = _symbol,
                side = Side.sell,
                type = "STOP LOSS",
                quantity = quantity,
                stopPrice = stopLossPrice,
                price = stopLossPrice
            };

            var response = PlaceTrade(trade);

            _openStopLossList.Add(
                new OpenStopLoss
                {
                    symbol = _symbol,
                    id = response.id,
                    price = stopLossPrice,
                    quantity = quantity
                });

            return response;
        }

        #endregion Stop Loss Management

        #region Trade Management

        /// <summary>
        /// Make a trade
        /// make 5 trade attempts if response comes back null
        /// </summary>
        /// <param name="tradeType">TradeType object</param>
        /// <param name="orderPrice">Trade price</param>
        /// <returns>Order object</returns>
        public Order MakeTrade(Side side, decimal orderPrice)
        {
            Order response = null;
            int i = 0;
            while (response == null && i < 5)
            {
                var quantity = GetTradeQuantity(side, orderPrice);
                if (i > 0)
                {
                    quantity = quantity - i;
                }
                _lastPrice = orderPrice;
                _lastQty = quantity;
                _lastTradeType = side;

                var trade = new TradeParams
                {
                    price = orderPrice,
                    symbol = _symbol,
                    side = side,
                    type = "limit",
                    quantity = quantity,
                    timeInForce = "GTC"
                };

                response = PlaceTrade(trade);
                i++;
            }

            return response;
        }

        /// <summary>
        /// Get price padding to avoid GDAX transaction fees
        /// </summary>
        /// <param name="tradeType">Current trade type</param>
        /// <param name="orderPrice">Order price</param>
        /// <returns>Update order price</returns>
        public decimal GetPricePadding(TradeType tradeType, decimal orderPrice)
        {
            return orderPrice;
        }

        /// <summary>
        /// Cancel trade
        /// </summary>
        /// <param name="orderId">OrderId to cancel</param>
        /// <param name="origClientOrderId">ClientOrderId to cancel</param>
        /// <param name="tradeType">Trade type</param>
        public void CancelTrade(string orderId, string tradeType = "")
        {
            var tradeParams = new CancelTradeParams
            {
                symbol = _symbol,
                Id = orderId,
                type = tradeType
            };

            var response = CancelTrade(tradeParams);
        }

        /// <summary>
        /// Cancel all open orders for the current trading pair
        /// </summary>
        /// <returns>Boolen when complete</returns>
        public bool CancelOpenOrders()
        {
            var openOrders = _switcheo.GetOpenSwitcheoOrders();

            while (openOrders != null && openOrders.Count() > 0)
            {
                for (var i = 0; i < openOrders.Length; i++)
                {
                    if (openOrders[i].pair.Equals(_symbol))
                    {
                        var signal = new TradeSignal
                        {
                            pair = _symbol,
                            price = openOrders[i].price,
                            signal = _signalType,
                            tradeType = TradeType.CANCELTRADE,
                            transactionDate = DateTime.UtcNow,
                        };
                        _fileRepo.LogSignal(signal);
                        CancelTrade(openOrders[i].id, openOrders[i].side);
                    }
                }
                openOrders = _switcheo.GetOpenSwitcheoOrders();
            }

            return true;
        }

        /// <summary>
        /// Gets latest buy and sell prices for the current pair
        /// </summary>
        /// <returns>Array of decimals</returns>
        public decimal[] GetLastBuySellPrice()
        {
            var orders = GetLatestOrders(_symbol);

            if (orders == null)
            {
                return new decimal[] { 0.00000000M, 0.00000000M };
            }

            var lastBuy = orders.Where(o => o.side.Equals("buy")).Select(o => o.price).FirstOrDefault();
            var lastSell = orders.Where(o => o.side.Equals("sell")).Select(o => o.price).FirstOrDefault();

            return new decimal[]
            {
                lastBuy > 0 ? lastBuy : 0.00000000M,
                lastSell > 0 ? lastSell : 0.00000000M
            };
        }

        /// <summary>
        /// Get orders for a pair
        /// </summary>
        /// <param name="symbol">String of symbol</param>
        /// <returns>SwitcheoOrder array</returns>
        public SwitcheoOrder[] GetOrders(string symbol)
        {
            int i = 0;
            var orders = _switcheo.GetSwitcheoOrders(symbol);

            while (orders == null && i < 3)
            {
                orders = _switcheo.GetSwitcheoOrders(symbol);
                i++;
            }

            return orders;
        }

        /// <summary>
        /// Get Latest Buy and Sell orders that were filled
        /// </summary>
        /// <param name="symbol">String of trading symbol</param>
        /// <returns>Array of SwitcheoOrder</returns>
        public SwitcheoOrder[] GetLatestOrders(string symbol)
        {
            var response = GetOrders(symbol);
            int i = 0;
            while (response == null && i < 3)
            {
                response = GetOrders(symbol);
                i++;
            }

            if (response == null)
            {
                return null;
            }

            var orderReverse = response.Where(o => o.orderStatus.Equals("completed"))
                                        .OrderByDescending(o => o.createdAt).ToArray();

            var orderList = new List<SwitcheoOrder>();

            var buyFound = false;
            var sellFound = false;
            for (i = 0; i < orderReverse.Length; i++)
            {
                if (orderReverse[i].side.Equals("buy") && !buyFound)
                {
                    orderList.Add(orderReverse[i]);
                    buyFound = true;
                }
                if (orderReverse[i].side.Equals("sell") && !sellFound)
                {
                    orderList.Add(orderReverse[i]);
                    sellFound = true;
                }

                if (buyFound && sellFound)
                {
                    break;
                }
            }

            return orderList.ToArray();
        }

        /// <summary>
        /// Open orders check
        /// </summary>
        /// <returns>OpenOrderDetail of open order</returns>
        public OpenOrderDetail OpenOrdersCheck()
        {
            OpenOrderDetail ooDetail = null;
            if (_botSettings.tradingStatus == TradeStatus.LiveTrading)
            {
                var orders = _switcheo.GetOpenOrders();

                var price = 0M;
                //TODO: get values
                //Decimal.TryParse()

                ooDetail = new OpenOrderDetail
                {
                    price = price,
                    timestamp = orders[0].created_at.ToUnixTimeMilliseconds()
                };
            }

            return ooDetail;
        }

        /// <summary>
        /// Cancel a trade
        /// </summary>
        /// <param name="id">Id of order to cancel</param>
        /// <returns>Order object</returns>
        public Order CancelTrade(CancelTradeParams cancelTradeParams)
        {
            if (_botSettings.tradingStatus == TradeStatus.LiveTrading)
                return _switcheo.CancelOrder(cancelTradeParams.Id);
            else if (_botSettings.tradingStatus == TradeStatus.PaperTrading)
                return CancelPaperTrade(cancelTradeParams.Id);
            else
                return null;
        }

        /// <summary>
        /// Cancel a paper trade for testing purposes
        /// </summary>
        /// <param name="id">Id of order to cancel</param>
        /// <returns>Order object</returns>
        public Order CancelPaperTrade(string id)
        {
            var response = new Order
            {
                id = $"PaperTrade_{_tradeNumber}",
                status = "processed",
                order_status = "cancelled"
            };

            return response;
        }

        #endregion Trade Management

        #region Order Management

        /// <summary>
        /// Get status of a trade
        /// </summary>
        /// <param name="orderId">OrderId of trade</param>
        /// <returns>OrderResponse</returns>
        public Order GetOrderStatus(Order trade)
        {
            if (_botSettings.tradingStatus == TradeStatus.LiveTrading)
                return _switcheo.GetOrder(trade.id);
            else if (_botSettings.tradingStatus == TradeStatus.PaperTrading)
                return GetPaperOrderStatus(trade.id);
            else
                return null;
        }

        /// <summary>
        /// Get status of a paper trade
        /// </summary>
        /// <param name="orderId">OrderId of trade</param>
        /// <returns>OrderResponse</returns>
        public Order GetPaperOrderStatus(string orderId)
        {
            var response = new Order
            {
                id = orderId,
                order_status = "completed"
            };

            return response;
        }

        #endregion Order Management
    }
}
