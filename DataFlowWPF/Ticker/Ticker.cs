using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Timers;

namespace DataFlowWPF
{
    public class Ticker : ITicker
    {
        #region Constants

        private const int MaxSpeed = 10;
        private const int SpeedMultiplier = 500;

        #endregion Constants

        #region Fields

        private readonly ITargetBlock<TickerMessage> _messagesAgent;
        private readonly BroadcastBlock<TickerEvent> _eventsAgent;
        private readonly ISourceBlock<TickerEvent> _eventsAgentSource;
        private readonly ILogger _log;
        private readonly Timer _timer = new Timer();

        #endregion Fields

        #region Constructors

        [ImportingConstructor]
        public Ticker(ILogger log) : this(log, speed: 5) { }

        public Ticker(ILogger log, int speed)
        {
            Guard.That(speed).IsBetween(0).And(MaxSpeed).Inclusive();

            _log = log;            
            _messagesAgent = new ActionBlock<TickerMessage>(new Action<TickerMessage>(process));
            _eventsAgent = new BroadcastBlock<TickerEvent>(tickerEvent => tickerEvent);
            _eventsAgentSource = _eventsAgent;
            setTimerSpeed(speed);
            _timer.Elapsed += (sender, e) =>
            {
                _eventsAgent.Post(new TickerEvent());
                Beeper.PlayBeep(440, 100);
            };
        }

        // maybe should be something like:
        // |                     x
        // |                 x 
        // |              x
        // |            x 
        // |           x
        // |          x
        // |         x   
        // |       x
        // |    x
        // x-----------------------
        //
        // guess that's a sigmoid?
        // 

        private readonly Random random = new Random();
             
        private void setTimerSpeed(int speed)
        {
            _log.Verbose($"Setting speed to {speed}");
            var cappedSpeed = speed > MaxSpeed ? MaxSpeed : speed;

            if (cappedSpeed < speed)
                _log.Verbose($"Speed capped at {cappedSpeed}");

            var interval = (MaxSpeed - cappedSpeed + 1) * SpeedMultiplier;

            _log.Verbose($"Interval calculated as {interval}ms");

            _timer.Interval = interval <= 0 ? 1 : interval;

            if (random.Next(3) == 0)
                throw new Exception("Random failure");
        }

        #endregion Constructors

        #region Methods

        private void process(TickerMessage message)
        {
            StartMessage startMessage = message as StartMessage;
            StopMessage stopMessage = message as StopMessage;
            ChangeSpeedMessage changeSpeedMessage = message as ChangeSpeedMessage;

            if (startMessage != null)
            {
                _log.Verbose("Start message received");
                _timer.Start();                
            }

            if (stopMessage != null)
            {
                _log.Verbose("Stop message received");
                _timer.Stop();
            }

            if (changeSpeedMessage != null)
            {
                var speed = changeSpeedMessage.NewSpeed;
                _log.Verbose($"Change speed message received: {speed}");
                setTimerSpeed(speed);
            }
        }

        #endregion Methods

        // N.B. these interface implementations could quite easily have been left out and the underlying agents exposed.
        //      Doing it this way does add a lot of code, but it was easy because VS can auto-generate them.
        //      The other option would be to expose the agents as readonly public properties, which would be fine.
        //      It would just mean losing the ability to do viewModel.LinkTo(logger) or whatever, you'd need to do:
        //      viewModel.MessageAgent.LinkTo(logger.Agent
        //
        // Turns out this is like a delegate in Pony, if I understand the tutorial that is :)


        #region ITargetBlock

        public Task Completion
        {
            get
            {
                return _messagesAgent.Completion;
            }
        }

        public void Complete()
        {
            _messagesAgent.Complete();
        }

        public void Fault(Exception exception)
        {
            _log.Error($"There was a fault in Ticker: {exception}");
            _messagesAgent.Fault(exception);
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TickerMessage messageValue, ISourceBlock<TickerMessage> source, bool consumeToAccept)
        {
            return _messagesAgent.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion ITargetBlock

        #region ISourceBlock

        public IDisposable LinkTo(ITargetBlock<TickerEvent> target, DataflowLinkOptions linkOptions)
        {
            return _eventsAgent.LinkTo(target, linkOptions);
        }

        public TickerEvent ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TickerEvent> target, out bool messageConsumed)
        {
            return _eventsAgentSource.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TickerEvent> target)
        {
            return _eventsAgentSource.ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TickerEvent> target)
        {
            _eventsAgentSource.ReleaseReservation(messageHeader, target);
        }

        #endregion

        #region IDisposable
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}
