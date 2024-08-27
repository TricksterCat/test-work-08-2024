using Runtime.AppContext;
using Runtime.Services.Player;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;

namespace Runtime.Tiles
{
    [CreateAssetMenu(menuName = "SpecialTiles/Spawn point")]
    public class SpawnPointTile : TileBase
    {
        [SerializeField]
        private Sprite _sprite;
        [SerializeField]
        private string _playerTag;
        
        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            if (!string.IsNullOrEmpty(_playerTag))
            {
                var lifetime = LifetimeScope.Find<MainLifetime>();
                if (lifetime == null || lifetime.Container is null)
                {
                    return false;
                }
                return lifetime.Container.Resolve<PlayerService>().TryPlayerSpawn(_playerTag, position);
            }

            return false;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            if (Application.isPlaying)
            {
                return;
            }

            tileData.sprite = _sprite;
        }
    }
}