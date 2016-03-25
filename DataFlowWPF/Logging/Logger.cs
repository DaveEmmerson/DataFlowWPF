using System;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlowWPF
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Logger : ILogger
    {
        #region Fields

        private readonly BroadcastBlock<string> _agent;
        private int _logCount = 1;

        #endregion Fields

        #region Constructors

        public Logger() : this(null) { }

        public Logger([Optional] TaskScheduler scheduler)
        {
            var options = new DataflowBlockOptions()
            {
                TaskScheduler = scheduler ?? TaskScheduler.Current
            };

            _agent = new BroadcastBlock<string>(message => message, options);
        }

        #endregion Constructors

        #region Methods

        public void Verbose(string message, [CallerMemberName] string originalCaller = null) {

            Write(message, originalCaller);

        }

        public void Error(string message, [CallerMemberName] string originalCaller = null) {

            Write(message, originalCaller);

        }

        private void Write(string message, string originalCaller, [CallerMemberName] string type = null )
        {
            var thread = Thread.CurrentThread;

            var threadId = thread.ManagedThreadId;
            var threadType = thread.IsThreadPoolThread ? "pool" : "UI  ";

            _agent.Post($"{_logCount++:0000} {type}: {threadType} ({threadId:000}): {originalCaller}: {message}");
        }

        #endregion Methods

        #region ISourceBlock

        public Task Completion
        {
            get
            {
                return _agent.Completion;
            }
        }

        public IDisposable LinkTo(ITargetBlock<string> target, DataflowLinkOptions linkOptions)
        {
            return _agent.LinkTo(target, linkOptions);
        }

        public string ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<string> target, out bool messageConsumed)
        {
            return ((ISourceBlock<string>)_agent).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<string> target)
        {
            return ((ISourceBlock<string>)_agent).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<string> target)
        {
            ((ISourceBlock<string>)_agent).ReleaseReservation(messageHeader, target);
        }

        public void Complete()
        {
            _agent.Complete();
        }

        public void Fault(Exception exception)
        {
            ((ISourceBlock<string>)_agent).Fault(exception);
        }

        #endregion ISourceBlock
    }
}