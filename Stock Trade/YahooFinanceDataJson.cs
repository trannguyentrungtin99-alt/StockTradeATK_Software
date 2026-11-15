using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Stock_Trade
{
    class YahooFinanceDataJson
    {
        public class Chart
        {
            public List<Result> result { get; set; }
            public object error { get; set; }
        }

        public class CurrentTradingPeriod
        {
            public Pre pre { get; set; }
            public Regular regular { get; set; }
            public Post post { get; set; }
        }

        public class Indicators
        {
            public List<Quote> quote { get; set; }
        }

        public class Meta
        {
            public string currency { get; set; }
            public string symbol { get; set; }
            public string exchangeName { get; set; }
            public string fullExchangeName { get; set; }
            public string instrumentType { get; set; }
            public int firstTradeDate { get; set; }
            public int regularMarketTime { get; set; }
            public bool hasPrePostMarketData { get; set; }
            public int gmtoffset { get; set; }
            public string timezone { get; set; }
            public string exchangeTimezoneName { get; set; }
            public double regularMarketPrice { get; set; } // Changed to double {int}
            public double regularMarketChangePercent { get; set; }
            public double fulldayPrice { get; set; }
            public double fulldayChange { get; set; }
            public double fulldayChangePercent { get; set; }
            public double fiftyTwoWeekHigh { get; set; }
            public double fiftyTwoWeekLow { get; set; }
            public double regularMarketDayHigh { get; set; }
            public double regularMarketDayLow { get; set; }
            public long regularMarketVolume { get; set; } // Volume can be large, use long
            public string longName { get; set; }
            public string shortName { get; set; }
            public double chartPreviousClose { get; set; }
            public double previousClose { get; set; }
            public int scale { get; set; }
            public int priceHint { get; set; }
            public CurrentTradingPeriod currentTradingPeriod { get; set; }
            public List<List<Object>> tradingPeriods { get; set; }
            public string dataGranularity { get; set; }
            public string range { get; set; }
            public List<string> validRanges { get; set; }
        }

        public class Post
        {
            public string timezone { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public int gmtoffset { get; set; }
        }

        public class Pre
        {
            public string timezone { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public int gmtoffset { get; set; }
        }

        public class Quote
        {
            public List<double?> low { get; set; }     // Changed to double?
            public List<double?> close { get; set; }   // Changed to double?
            public List<double?> open { get; set; }    // Changed to double?
            public List<long?> volume { get; set; }    // Changed to long?
            public List<double?> high { get; set; }    // Changed to double?
        }

        public class Regular
        {
            public string timezone { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public int gmtoffset { get; set; }
        }

        public class Result
        {
            public Meta meta { get; set; }
            public List<long> timestamp { get; set; }
            public Indicators indicators { get; set; }
        }
        
        public class Root
        {
            public Chart chart { get; set; }
        }
    }
}