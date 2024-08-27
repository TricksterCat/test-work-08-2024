using Runtime.UI;
using VitalRouter;

namespace Runtime.Commands
{
    public readonly struct RequestViewCommand : ICommand
    {
        public RequestViewCommand(ViewKey view)
        {
            View = view;
        }

        public ViewKey View { get; }
    }
}