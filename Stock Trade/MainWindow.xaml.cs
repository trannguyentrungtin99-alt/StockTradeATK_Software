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
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel; 

namespace Stock_Trade
{
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public Func<ChartPoint, string> LabelPoint { get; set; }
        private ChartValues<OhlcPoint> candleValues;

        public readonly HttpClient client = new HttpClient();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                OnPropertyChanged(nameof(Labels));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            candleValues = new ChartValues<OhlcPoint>();

            LabelPoint = point =>
            {
                var candlePoint = (OhlcPoint)point.Instance;
                return $"O: {candlePoint.Open} \nH: {candlePoint.High} \nL: {candlePoint.Low} \nC: {candlePoint.Close}";
            };

            SeriesCollection = new SeriesCollection
            {
                new CandleSeries
                {
                    Values = candleValues,
                    LabelPoint = LabelPoint
                }
            };

            YFormatter = value => value.ToString("N2");

            DateTimeX.Labels = new string[] { string.Empty };

            DataContext = this;
        }

        public Func<double, string> YFormatter { get; set; }

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
            await Task.Run(() =>
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var yahooData = JsonSerializer.Deserialize<Root>(jsonResult, options);

                if (yahooData?.chart?.result != null && yahooData.chart.result.Count > 0)
                {
                    var resultObj = yahooData.chart.result[0];
                    var meta = resultObj.meta;
                    string symbol = meta?.symbol;
                    double price = Math.Round(meta.regularMarketPrice, 3, MidpointRounding.AwayFromZero);

                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Stock Code: {symbol}\nMarket Price: {price}");
                    });

                    var quotes = resultObj.indicators?.quote?[0];
                    var timestamps = resultObj.timestamp;

                    if (quotes != null && quotes.open != null && timestamps != null)
                    {
                        var newCandleValues = new ChartValues<OhlcPoint>();
                        var newLabels = new List<string>();

                        TimeSpan vnOffset = TimeSpan.FromHours(7);

                        for (int i = 0; i < quotes.open.Count; i++)
                        {
                            if (quotes.open[i].HasValue && quotes.high[i].HasValue &&
                                quotes.low[i].HasValue && quotes.close[i].HasValue)
                            {
                                newCandleValues.Add(new OhlcPoint(
                                    quotes.open[i].Value,
                                    quotes.high[i].Value,
                                    quotes.low[i].Value,
                                    quotes.close[i].Value
                                ));

                                if (i < timestamps.Count)
                                {
                                    long ts = timestamps[i];
                                    DateTimeOffset dtUTC = DateTimeOffset.FromUnixTimeSeconds(ts);
                                    DateTimeOffset dtVN = dtUTC.ToOffset(vnOffset);
                                    newLabels.Add(dtVN.ToString("yyyy-MM-dd HH:mm"));
                                }
                                else
                                {
                                    newLabels.Add(string.Empty);
                                }
                            }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            candleValues.Clear();
                            foreach (var val in newCandleValues)
                            {
                                candleValues.Add(val);
                            }

                            Labels = newLabels.ToArray();
                            DateTimeX.Labels = Labels; // Refresh the Axis control labels
                        });
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Data not found for this ticker.");
                    });
                }
            });
        }
    }
}