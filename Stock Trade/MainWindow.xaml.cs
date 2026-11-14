using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using static Stock_Trade.YahooFinanceDataJson;

namespace Stock_Trade
{
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<ChartPoint, string> LabelPoint { get; set; }
        private ChartValues<OhlcPoint> candleValues;

        public readonly HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            candleValues = new ChartValues<OhlcPoint>();

            LabelPoint = point =>
            {
                var candlePoint = (OhlcPoint)point.Instance;
                return $"O: {candlePoint.Open} | H: {candlePoint.High} | L: {candlePoint.Low} | C: {candlePoint.Close}";
            };

            SeriesCollection = new SeriesCollection
            {
                new CandleSeries
                {
                    Values = candleValues,
                    LabelPoint = LabelPoint
                }
            };

            DataContext = this;
        }

        private async void txtTicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var stockCode = txtTicker.Text.Trim().ToUpper();
                if (string.IsNullOrEmpty(stockCode))
                {
                    MessageBox.Show("Please enter a valid stock code.");
                    return;
                }
                await LoadStockDataAsync(stockCode);
                txtName.Content = txtTicker.Text;
            }
        }

        private async Task LoadStockDataAsync(string stockCode)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://query1.finance.yahoo.com/v8/finance/chart/{stockCode}");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string jsonResult = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(jsonResult);

                await SaveAndDisplayStockData(jsonResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private async Task SaveAndDisplayStockData(string jsonResult)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var yahooData = JsonSerializer.Deserialize<Root>(jsonResult, options);

            if (yahooData?.chart?.result != null && yahooData.chart.result.Count > 0)
            {
                var resultObj = yahooData.chart.result[0];
                var meta = resultObj.meta;
                string symbol = meta?.symbol;
                double price = meta.regularMarketPrice;

                MessageBox.Show($"Stock Code: {symbol}\nMarket Price: {price}");

                var quotes = resultObj.indicators?.quote?[0];
                if (quotes != null && quotes.open != null)
                {
                    candleValues.Clear();

                    for (int i = 0; i < quotes.open.Count; i++)
                    {
                        if (quotes.open[i].HasValue && quotes.high[i].HasValue &&
                            quotes.low[i].HasValue && quotes.close[i].HasValue)
                        {
                            candleValues.Add(new OhlcPoint(
                                quotes.open[i].Value,
                                quotes.high[i].Value,
                                quotes.low[i].Value,
                                quotes.close[i].Value
                            ));
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Data not found for this ticker.");
            }
        }
    }
}