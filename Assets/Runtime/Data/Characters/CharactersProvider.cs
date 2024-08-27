using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Data.Abstract;
using Runtime.Services.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CharacterController = Runtime.Controllers.CharacterController;
using Object = UnityEngine.Object;

namespace Runtime.Data.Characters
{
    public class CharactersProvider : DataProvider<CharacterController>
    {
        private readonly CharactersDatabase _database;
        private readonly IObjectResolver _resolver;

        public CharactersProvider(CharactersDatabase database, IObjectResolver resolver)
        {
            _database = database;
            _resolver = resolver;
        }
        
        public override bool CanInject(string key)
        {
            return _database.Contains(key);
        }

        protected override async UniTask<CharacterController> InternalInjectAsync(string key, Handler handler,
            CancellationToken resolveCancellation)
        {
            var config = _database.Get(key);
            var prefab = handler.LoadAsset(config.Prefab);

            var value = await prefab;
            if (!value.TryGetComponent(out CharacterController controller))
            {
                throw new InvalidCastException($"CharacterController component not found for key: \"{key}\"");
            }

            return controller;
        }

        public CharacterController CreateCharacter(string key, PlayerController controller, Vector2 position)
        {
            var config = _database.Get(key);
            var prefab = ResolveInstant(key);

            var instance = _resolver.Instantiate(prefab, position, Quaternion.identity);
            var disposable = Disposable.Create(instance.gameObject, Object.Destroy);
            instance.Setup(controller, config, _resolver, disposable);

            return instance;
        }
    }
}