using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ECN_Exercise.Sources.Models;
using ECN_Exercise.Sources.Utils;
using ReactiveUI;

namespace ECN_Exercise.Sources.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private const string RequestUrlLastest = "https://api.fixer.io/latest?symbols=USD,TRY";

        private const string RequestUrlDate = "https://api.fixer.io/{0}?symbols=USD,TRY";

        public MainPageViewModel()
        {
            CalculateCommand = ReactiveCommand.CreateFromTask(CalculateTask);

            CalculateMultiPointCommand = ReactiveCommand.CreateFromTask(CalculateMultiPointTask);
        }

        #region CalculateCommand

        public ICommand CalculateCommand { get; }

        private async Task CalculateTask()
        {
            //show loading
            SignalLoading = true;

            //do in background
            await Task.Run(async () => { await FillData(); });

            //hide loading
            SignalLoading = false;
        }

        private async Task FillData()
        {
            var xVals = new double[12];
            var yVals = new double[12];

            for (var x = 1; x <= 12; x++)
            {
                var month = x < 10 ? $"0{x}" : x.ToString();
                var requestUri = string.Format(RequestUrlDate, $"2016-{month}-15");
                var responseMessage = await XModernHttpClientService.Instance.GetAsync(requestUri: requestUri,
                    cancellationToken: CancellationToken.None,
                    completionOption: HttpCompletionOption.ResponseContentRead);

                if (responseMessage == null) continue;

                responseMessage = responseMessage.EnsureSuccessStatusCode();
                var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                var result = XModernHttpClientService.Instance.Serializer
                    .DeserializeFromJsonStream<CurrencyModel>(jsonStream, requestUri);

                xVals[x - 1] = result.Rates.USD;
                yVals[x - 1] = result.Rates.TRY;
            }

            /*https://api.fixer.io/2017-01-15?symbols=USD,TRY
             {"base":"EUR","date":"2017-01-13","rates":{"TRY":4.0387,"USD":1.0661}}
             */
            var equation = LinearRegressionV2(xVals: xVals, yVals: yVals, xVal: 1.0661);

            Debug.WriteLine($"equation: {equation}");

            CalResult = equation;
        }

        #endregion

        #region CalculateMultiPointCommand

        public ICommand CalculateMultiPointCommand { get; }

        private async Task CalculateMultiPointTask()
        {
            //show loading
            SignalLoading = true;

            //do in background
            await Task.Run(async () => { await FillMultiData(); });

            //hide loading
            SignalLoading = false;
        }

        private async Task FillMultiData()
        {
            var xVals = new double[120];
            var yVals = new double[120];
            var index = 0;

            for (var x = 1; x <= 12; x++)
            {
                var month = x < 10 ? $"0{x}" : x.ToString();
                for (var y = 10; y < 20; y++)
                {
                    var requestUri = string.Format(RequestUrlDate, $"2016-{month}-{y}");
                    var responseMessage = await XModernHttpClientService.Instance.GetAsync(requestUri: requestUri,
                        cancellationToken: CancellationToken.None,
                        completionOption: HttpCompletionOption.ResponseContentRead);

                    if (responseMessage == null) continue;

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var result = XModernHttpClientService.Instance.Serializer
                        .DeserializeFromJsonStream<CurrencyModel>(jsonStream, requestUri);

                    xVals[index] = result.Rates.USD;
                    yVals[index] = result.Rates.TRY;

                    index = index + 1;
                }
            }

            /*https://api.fixer.io/2017-01-15?symbols=USD,TRY
             {"base":"EUR","date":"2017-01-13","rates":{"TRY":4.0387,"USD":1.0661}}
             */
            var equation = LinearRegressionV2(xVals: xVals, yVals: yVals, xVal: 1.0661);

            Debug.WriteLine($"equation: {equation}");

            CalResult = equation;
        }

        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="xVals"></param>
        /// <param name="yVals"></param>
        /// <param name="xVal"></param>
        /// <returns></returns>
        private static double LinearRegressionV2(IReadOnlyList<double> xVals, IReadOnlyList<double> yVals, double xVal)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumCodeviates = 0;
            double count = xVals.Count;

            for (var index = 0; index < count; index++)
            {
                var x = xVals[index];
                var y = yVals[index];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
            }

            var slopeB = (count * sumCodeviates - sumOfX * sumOfY) / (count * sumOfXSq - sumOfX * sumOfX);

            var interceptA = (sumOfY - slopeB * sumOfX) / count;

            var equationY = interceptA + slopeB * xVal;

            Debug.WriteLine("slopeB: {0}, interceptA: {1}, equationY: {2}", slopeB, interceptA, equationY);

            return equationY;
        }

        #region properties

        private string _dateTimeRequest;

        public string DateTimeRequest
        {
            get { return _dateTimeRequest; }
            set { this.RaiseAndSetIfChanged(ref _dateTimeRequest, value); }
        }

        private bool _signalLoading;

        public bool SignalLoading
        {
            get { return _signalLoading; }
            set { this.RaiseAndSetIfChanged(ref _signalLoading, value); }
        }

        private double _calResult;

        public double CalResult
        {
            get { return _calResult; }
            set { this.RaiseAndSetIfChanged(ref _calResult, value); }
        }

        #endregion
    }
}