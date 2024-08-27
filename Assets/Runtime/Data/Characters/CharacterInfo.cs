using UnityEngine;

namespace Runtime.Data.Characters
{
    public class CharacterInfo
    {
        public CharacterInfo(string name, string description, Sprite preview, string[] weapons)
        {
            Name = name;
            Description = description;
            Preview = preview;
            Weapons = weapons;
        }

        public string Name { get; }
        public string Description { get; }
        
        public Sprite Preview { get; }
        public string[] Weapons { get; }
    }
}