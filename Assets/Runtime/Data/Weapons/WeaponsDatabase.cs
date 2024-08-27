using Runtime.Data.Abstract;
using UnityEngine;

namespace Runtime.Data.Weapons
{
    [CreateAssetMenu(menuName = "Database/Weapons database", fileName = DefaultAssetName)]
    public class WeaponsDatabase : ConfigDatabase<WeaponConfig>
    {
        public const string DefaultAssetName = "Weapons";
    }
}