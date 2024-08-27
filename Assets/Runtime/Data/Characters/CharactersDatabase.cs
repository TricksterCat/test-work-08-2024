using Runtime.Data.Abstract;
using UnityEngine;

namespace Runtime.Data.Characters
{
    [CreateAssetMenu(menuName = "Database/Characters database", fileName = DefaultAssetName)]
    
    public sealed class CharactersDatabase : ConfigDatabase<CharacterConfig>
    {
        public const string DefaultAssetName = "Characters";
    }
}