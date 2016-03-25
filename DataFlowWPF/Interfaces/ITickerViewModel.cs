using System.ComponentModel.Composition;
using System.Threading.Tasks.Dataflow;

namespace DataFlowWPF
{
    [InheritedExport]
    public interface ITickerViewModel : ISourceBlock<TickerMessage>
    {
        Command StartCommand { get; }
        Command StopCommand { get; }
        int Speed { get; }
    }
}
