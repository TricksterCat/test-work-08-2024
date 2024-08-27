using Runtime.Common;
using Runtime.Controllers;
using VitalRouter;

namespace Runtime.Commands
{
    public readonly struct EntryDestroyedCommand : ICommand
    {
        public EntryDestroyedCommand(EntryType type, string id, BaseEntry entry)
        {
            Entry = entry;
            Type = type;
        }

        public EntryType Type { get; }
        public BaseEntry Entry { get; }
    }
}