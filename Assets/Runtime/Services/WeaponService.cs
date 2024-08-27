using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Commands;
using Runtime.Common;
using Runtime.Data.Characters;
using Runtime.Data.Weapons;
using VitalRouter;

namespace Runtime.Services
{
    [Routes]
    public partial class WeaponService : IDisposable
    {
        private readonly WeaponsProvider _provider;
        private readonly CharactersDatabase _characterDatabase;
        private readonly Dictionary<string, WeaponHandler> _weaponHandlers = new();
        
        private DisposableBag _disposable;

        public WeaponService(WeaponsProvider weaponsProvider, CharactersProvider charactersProvider, CharactersDatabase charactersDatabase)
        {
            _provider = weaponsProvider;
            _characterDatabase = charactersDatabase;

            charactersProvider.DisposedNext.Subscribe(OnCharacterDisposed)
                .AddTo(ref _disposable);
        }


        public WeaponModel ResolveWeapon(string weapon)
        {
            return _provider.ResolveInstant(weapon);
        }

        [Route]
        private UniTask On(CharacterInjectingCommand command)
        {
            var config = _characterDatabase.Get(command.ID);
            if (!_weaponHandlers.TryGetValue(command.ID, out var handler))
            {
                handler = new WeaponHandler(new FeatureHandler(config.Weapons));
                _weaponHandlers[command.ID] = handler;
            }

            return handler.RentAsync();
        }
        
        private void OnCharacterDisposed(string id)
        {
            if (_weaponHandlers.TryGetValue(id, out var handler))
            {
                handler.Dispose();
            }
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
            foreach (var value in _weaponHandlers.Values)
            {
                value.Dispose();
            }
            _weaponHandlers.Clear();
        }
        
        private class WeaponHandler : IDisposable
        {
            private readonly FeatureHandler _feature;
            private CancellationTokenSource _cancellationTokenSource;

            public WeaponHandler(FeatureHandler feature)
            {
                _feature = feature;
            }

            public UniTask RentAsync()
            {
                if(_cancellationTokenSource is { IsCancellationRequested: false })
                    return UniTask.CompletedTask;
                _cancellationTokenSource = new CancellationTokenSource();
                return _feature.RequestAsync(Router.Default, _cancellationTokenSource.Token);
            }

            public void Dispose()
            {
                _feature?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
        }
    }
}