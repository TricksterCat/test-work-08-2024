using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Controllers;
using Runtime.Data.Abstract;

namespace Runtime.Data.Weapons
{
    public class WeaponsProvider : DataProvider<WeaponModel>
    {
        private readonly WeaponsDatabase _database;

        public WeaponsProvider(WeaponsDatabase database)
        {
            _database = database;
        }

        public override bool CanInject(string key) => _database.Contains(key);

        protected override async UniTask<WeaponModel> InternalInjectAsync(string key, Handler handler, CancellationToken cancellationToken)
        {
            var config = _database.Get(key);

            var projectileLoader = handler.LoadAsset(config.Projectile);
            var previewLoader =  handler.LoadAsset(config.Preview);

            var result = await UniTask.WhenAll(projectileLoader.ToUniTask(cancellationToken: cancellationToken),
                previewLoader.ToUniTask(cancellationToken: cancellationToken));

            var controllerPrefab = result.Item1.GetComponent<WeaponController>();

            return new WeaponModel(controllerPrefab, result.Item2, config);
        }
    }
}