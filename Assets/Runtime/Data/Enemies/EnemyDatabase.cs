using Runtime.Data.Abstract;
using UnityEngine;

namespace Runtime.Data.Enemies
{
    [CreateAssetMenu(menuName = "Database/Enemies database", fileName = DefaultAssetName)]
    public sealed class EnemyDatabase : ConfigDatabase<EnemyConfig>
    {
        public const string DefaultAssetName = "Enemies";
    }
}