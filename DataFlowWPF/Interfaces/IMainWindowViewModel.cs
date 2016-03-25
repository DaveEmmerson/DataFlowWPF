using System.ComponentModel.Composition;

namespace DataFlowWPF
{
    [InheritedExport]
    public interface IMainWindowViewModel
    {
        ITickerViewModel TickerViewModel { get; }
        ILogViewModel LogViewModel { get; }
    }
}
