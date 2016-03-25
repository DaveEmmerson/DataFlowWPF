using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks.Dataflow;

namespace DataFlowWPF
{
    [InheritedExport]
    public interface ITicker : ITargetBlock<TickerMessage>, ISourceBlock<TickerEvent>, IDisposable
    {
    }
}