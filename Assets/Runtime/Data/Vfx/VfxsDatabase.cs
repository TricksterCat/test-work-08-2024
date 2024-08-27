using Runtime.Data.Abstract;
using UnityEngine;

namespace Runtime.Data.Vfx
{
    [CreateAssetMenu(menuName = "Database/Vfxs database", fileName = DefaultAssetName)]
    public class VfxsDatabase : ConfigDatabase<VfxConfig>
    {
        public const string DefaultAssetName = "Vfxs";
    }
}