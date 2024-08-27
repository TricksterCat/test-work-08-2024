using VitalRouter;

namespace Runtime.Commands
{
    public readonly struct CharacterInjectingCommand : ICommand
    {
        public CharacterInjectingCommand(string id)
        {
            ID = id;
        }

        public string ID { get; }
    }
}