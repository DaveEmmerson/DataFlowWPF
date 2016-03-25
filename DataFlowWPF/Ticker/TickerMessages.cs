namespace DataFlowWPF
{
    public class TickerMessage {

        protected TickerMessage() { }

    }

    public sealed class StartMessage : TickerMessage { }
    public sealed class StopMessage : TickerMessage { }
    public sealed class ChangeSpeedMessage : TickerMessage
    {
        public int NewSpeed { get; }
        public ChangeSpeedMessage(int newSpeed)
        {
            NewSpeed = newSpeed;
        }
    }
}
