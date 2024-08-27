using Runtime.UI;
using VitalRouter;

namespace Runtime.Commands
{
    public readonly struct ShowViewCommand : ICommand
    {
        public ShowViewCommand(string id)
        {
            ID = id;
        }

        public string ID { get; }
    }   
    
    public readonly struct HideViewCommand : ICommand
    {
        public HideViewCommand(string id)
        {
            ID = id;
        }

        public string ID { get; }
    }

    public readonly struct ViewStartedCommand : ICommand
    {
        public ViewStartedCommand(View instance)
        {
            Instance = instance;
        }

        public View Instance { get; }
    }
}