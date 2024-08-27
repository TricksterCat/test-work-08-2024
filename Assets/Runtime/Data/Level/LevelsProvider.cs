using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Controllers;
using Runtime.Data.Abstract;
using VContainer;
using VContainer.Unity;

namespace Runtime.Data.Level
{
    public class LevelsProvider : DataProvider<LevelModel>
    {
        private readonly IObjectResolver _resolver;
        private readonly LevelsDatabase _database;

        public LevelsProvider(LevelsDatabase database, IObjectResolver resolver)
        {
            _database = database;
            _resolver = resolver;
        }

        public override bool CanInject(string key) => _database.Contains(key);

        protected override async UniTask<LevelModel> InternalInjectAsync(string key, Handler handler, CancellationToken cancellationToken)
        {
            var config = _database.Get(key);
            var prefab = await handler.LoadAsset(config.Prefab).ToUniTask(cancellationToken: cancellationToken);
            var controllerPrefab = prefab.GetComponent<LevelController>();
            return new LevelModel(controllerPrefab);
        }

        public LevelController CreateLevel(string id)
        {
            var model = ResolveInstant(id);
            return _resolver.Instantiate(model.Prefab);
        }
    }
}