using UnityEngine;
using VitalRouter;

namespace Runtime.Commands
{
    public readonly struct PlayVfxCommand : ICommand
    {
        public PlayVfxCommand(string id, Vector2 position)
        {
            ID = id;
            Position = position;
        }

        public string ID { get; }
        public Vector2 Position { get; }
    }
}