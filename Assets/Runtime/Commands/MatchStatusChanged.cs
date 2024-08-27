using VitalRouter;

namespace Runtime.Commands
{
    public readonly struct MatchStatusChangedCommand : ICommand
    {
        public MatchStatusChangedCommand(bool status)
        {
            Status = status;
        }

        public bool Status { get; }
    }
}