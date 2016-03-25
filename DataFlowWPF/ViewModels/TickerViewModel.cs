using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlowWPF
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TickerViewModel : ViewModelBase, ITickerViewModel
    {
        #region Properties

        public Command StartCommand { get; }
        public Command StopCommand { get; }

        private int _speed;
        public int Speed
        {
            get { _log.Verbose($"Getting speed (is {_speed})"); return _speed; }
            set {
                _log.Verbose($"Setting speed to {value}");
                Set(ref _speed, value);

                if(_messagesAgent.Completion.IsCompleted || !_messagesAgent.Post(new ChangeSpeedMessage(value)))
                    _log.Verbose("Could not post to MessagesAgent.");


            }
        }

        private bool _running;
        private bool Running
        {
            set
            {
                if (value == _running)
                    return;

                _running = value;
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion Properties

        #region Fields

        private readonly BroadcastBlock<TickerMessage> _messagesAgent;
        private readonly ILogger _log;

        #endregion Fields

        #region Constructors

        [ImportingConstructor]
        public TickerViewModel(ILogger log)
        {
            Guard.NotNull(log);

            _log = log;

            var options = new DataflowBlockOptions()
            {
                TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
            };

            _messagesAgent = new BroadcastBlock<TickerMessage>(message => message, options);

            StartCommand = new Command(
                canExecute: _ => {
                    log.Verbose("CanExecute of _startCommand");
                    return !_running;
                },
                execute: _ =>
                {
                    log.Verbose("Execute of _startCommand - before set running");
                    Running = true;
                    log.Verbose("Execute of _startCommand - after set running");
                    log.Verbose("Execute of _startCommand - before post");
                    if(!_messagesAgent.Post(new StartMessage()))
                    {
                        log.Error("Could not post StartMessage");
                        Running = false;
                    }
                        
                    log.Verbose("Execute of _startCommand - after post");
                }
            );

            StopCommand = new Command(
                canExecute: _ => {
                    log.Verbose("CanExecute of _stopCommand");
                    return _running;
                },
                execute: _ =>
                {
                    log.Verbose("Execute of _stopCommand - before set running");
                    Running = false;
                    log.Verbose("Execute of _stopCommand - after set running");
                    log.Verbose("Execute of _stopCommand - before post");
                    if (!_messagesAgent.Post(new StopMessage()))
                    {
                        log.Error("Could not post StopMessage");
                        Running = true;
                    }
                        

                    log.Verbose("Execute of _stopCommand - after post");
                }
            );
        }

        #endregion Constructors

        #region ISourceBlock<TickerMessage>

        public Task Completion
        {
            get
            {
                return _messagesAgent.Completion;
            }
        }

        public IDisposable LinkTo(ITargetBlock<TickerMessage> target, DataflowLinkOptions linkOptions)
        {
            return _messagesAgent.LinkTo(target, linkOptions);
        }

        public TickerMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TickerMessage> target, out bool messageConsumed)
        {
            return ((ISourceBlock<TickerMessage>)_messagesAgent).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TickerMessage> target)
        {
            return ((ISourceBlock<TickerMessage>)_messagesAgent).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TickerMessage> target)
        {
            ((ISourceBlock<TickerMessage>)_messagesAgent).ReleaseReservation(messageHeader, target);
        }

        public void Complete()
        {
            _messagesAgent.Complete();
        }

        public void Fault(Exception exception)
        {
            ((ISourceBlock<TickerMessage>)_messagesAgent).Fault(exception);
        }

        #endregion ISourceBlock
    }
}
