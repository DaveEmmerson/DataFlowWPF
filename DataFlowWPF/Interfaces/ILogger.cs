using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace DataFlowWPF
{
    [InheritedExport]
    public interface ILogger : ISourceBlock<string>
    {
        void Verbose(string message, [CallerMemberName] string originalCaller = null);
        void Error(string message, [CallerMemberName] string originalCaller = null);
    }
}