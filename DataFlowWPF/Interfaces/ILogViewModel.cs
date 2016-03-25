using System.ComponentModel.Composition;
using System.Threading.Tasks.Dataflow;

namespace DataFlowWPF
{
    [InheritedExport]
    public interface ILogViewModel : ITargetBlock<string>
    {
        string Text { get; }
    }
}