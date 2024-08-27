using Runtime.Data.Abstract;
using UnityEngine;

namespace Runtime.Data.UI
{
    [CreateAssetMenu(menuName = "Database/Views database", fileName = DefaultAssetName)]
    public sealed class ViewDatabase : ConfigDatabase<ViewConfig>
    {
        public const string DefaultAssetName = "Views";
    }
}