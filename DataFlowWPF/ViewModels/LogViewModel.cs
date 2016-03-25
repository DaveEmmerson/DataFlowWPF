using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace DataFlowWPF
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LogViewModel : ViewModelBase, ILogViewModel
    {
        #region Properties

        private string _text;
        public string Text {
            get { return _text; }
            private set { Set(ref _text, value); }
        }

        #endregion Properties

        #region Fields

        private ActionBlock<string> _agent;

        private List<string> _messages = new List<string>();

        #endregion Fields

        #region Constructors

        public LogViewModel()
        {
            Action<string> action = message =>
            {
                _messages.Insert(0, message);
                Text = string.Join(Environment.NewLine, _messages);
            };

            _agent = new ActionBlock<string>(action);        
        }

        #endregion Constructors

        #region ITargetBlock

        public Task Completion
        {
            get
            {
                return _agent.Completion;
            }
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, string messageValue, ISourceBlock<string> source, bool consumeToAccept)
        {
            return ((ITargetBlock<string>)_agent).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete()
        {
            _agent.Complete();
        }

        public void Fault(Exception exception)
        {
            ((ITargetBlock<string>)_agent).Fault(exception);
        }

        #endregion ITargetBlock
    }
}