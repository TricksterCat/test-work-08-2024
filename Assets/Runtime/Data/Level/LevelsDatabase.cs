using Runtime.Data.Abstract;
using UnityEngine;

namespace Runtime.Data.Level
{
    [CreateAssetMenu(menuName = "Database/Levels database", fileName = DefaultAssetName)]
    public class LevelsDatabase : ConfigDatabase<LevelConfig>
    {
        public const string DefaultAssetName = "Levels";
    }
}