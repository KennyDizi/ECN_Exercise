using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace ECN_Exercise
{
    public partial class MainPage
    {
        private IObservable<bool> _observableSignalLoading;
        private IObservable<double> _observableCalResult;

        public MainPage()
        {
            InitializeComponent();
        }

        public override void SetupReactiveObservables()
        {
            _observableSignalLoading = this.WhenAnyValue(view => view.ViewModel.SignalLoading)
                .ObserveOn(RxApp.MainThreadScheduler);

            _observableCalResult = this.WhenAnyValue(view => view.ViewModel.CalResult)
                .Where(x => x > 0)
                .ObserveOn(RxApp.MainThreadScheduler);
        }

        public override void SetupReactiveSubscriptions()
        {
            _observableSignalLoading.Subscribe(signalLoading =>
                {
                    StackLayoutCalculating.IsVisible = signalLoading;
                    ActivityIndicatorCalculating.IsRunning = signalLoading;
                })
                .DisposeWith(RxCompositeDisposable);

            _observableCalResult.Subscribe(calResult =>
                {
                    /*https://api.fixer.io/2017-01-15?symbols=USD,TRY
                 {"base":"EUR","date":"2017-01-13","rates":{"TRY":4.0387,"USD":1.0661}}
                 */

                    DisplayAlert("Message", $"The calculating result: {calResult}, real result: 4.0387", "OK");
                })
                .DisposeWith(RxCompositeDisposable);
        }
    }
}