using System.ComponentModel.Composition;

namespace DataFlowWPF
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        public ITickerViewModel TickerViewModel { get; }
        public ILogViewModel LogViewModel { get; }

        [ImportingConstructor]
        public MainWindowViewModel(ITickerViewModel tickerViewModel, ILogViewModel logViewModel)
        {
            TickerViewModel = tickerViewModel;
            LogViewModel = logViewModel;
        }
    }
}
