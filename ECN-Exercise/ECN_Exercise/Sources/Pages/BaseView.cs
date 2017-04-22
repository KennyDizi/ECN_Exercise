using System.Reactive.Disposables;
using ECN_Exercise.Sources.ViewModels;
using ReactiveUI;
using Xamarin.Forms;

namespace ECN_Exercise.Sources.Pages
{
    public class BasePage<TViewModel> : ContentPage, IViewFor<TViewModel> where TViewModel : BaseViewModel
    {
        public BasePage()
        {
            SetupReactive();
        }

        private void SetupReactive()
        {
            RxCompositeDisposable = new CompositeDisposable();

            SetupReactiveObservables();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetupReactiveSubscriptions();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ClearReactiveSubscriptions();
        }

        #region setup rxui
        /// <summary>
        /// container to storage all IDispose
        /// </summary>
        public CompositeDisposable RxCompositeDisposable;

        /// <summary>
        /// this function to setup observables
        /// </summary>
        public virtual void SetupReactiveObservables() { }

        /// <summary>
        /// this function to setup subcriptions of observables
        /// </summary>
        public virtual void SetupReactiveSubscriptions() { }

        /// <summary>
        /// dispose all IDispose
        /// </summary>
        public virtual void ClearReactiveSubscriptions()
        {
            RxCompositeDisposable.Dispose();
        }

        #endregion

        /// <summary>
        /// The ViewModel to display
        /// </summary>
        public TViewModel ViewModel
        {
            get { return (TViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(TViewModel),
            typeof(BasePage<TViewModel>),
            default(TViewModel),
            BindingMode.OneWay,
            propertyChanged: OnViewModelChanged);

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            this.ViewModel = this.BindingContext as TViewModel;
        }

        private static void OnViewModelChanged(BindableObject bindableObject, object oldValue, object newValue)
        {
            bindableObject.BindingContext = newValue;
        }
    }
}