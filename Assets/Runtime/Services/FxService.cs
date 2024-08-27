using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Commands;
using Runtime.Common;
using Runtime.Data.Characters;
using Runtime.Data.Enemies;
using Runtime.Data.Vfx;
using VitalRouter;

namespace Runtime.Services
{
    [Routes]
    public partial class FxService : IDisposable
    {
        private readonly CharactersDatabase _characterDatabase;
        private readonly EnemyDatabase _enemyDatabase;
        private readonly VfxProvider _vfxProvider;
        
        private readonly Dictionary<string, FxHandler> _fxHandlers = new();

        private DisposableBag _disposable;
        
        public FxService(VfxProvider vfxProvider, 
            CharactersDatabase characterDatabase, 
            CharactersProvider charactersProvider,
            EnemyDatabase enemyDatabase,
            EnemiesProvider enemiesProvider)
        {
            _vfxProvider = vfxProvider;
            _characterDatabase = characterDatabase;
            _enemyDatabase = enemyDatabase;
            
            charactersProvider.DisposedNext.Subscribe(OnEntryDisposed).AddTo(ref _disposable);
            enemiesProvider.DisposedNext.Subscribe(OnEntryDisposed).AddTo(ref _disposable);
        }

        [Route]
        private void On(PlayVfxCommand command)
        {
            _vfxProvider.PlayVfx(command.ID, command.Position);
        }
        
        
        [Route]
        private UniTask On(CharacterInjectingCommand command)
        {
            var config = _characterDatabase.Get(command.ID);
            return InjectFXs(command.ID, config.DeadFx);
        }
        
        [Route]
        private UniTask On(EnemyInjectingCommand command)
        {
            var config = _enemyDatabase.Get(command.ID);
            return InjectFXs(command.ID, config.DeadFx);
        }

        private UniTask InjectFXs(string id, params string[] ids)
        {
            if (!_fxHandlers.TryGetValue(id, out var handler))
            {
                handler = new FxHandler(new FeatureHandler(ids));
                _fxHandlers[id] = handler;
            }

            return handler.RentAsync();
        }
        
        private void OnEntryDisposed(string id)
        {
            if (_fxHandlers.TryGetValue(id, out var handler))
            {
                handler.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
            foreach (var handler in _fxHandlers.Values)
            {
                handler.Dispose();
            }
            _fxHandlers.Clear();
        }
        
        private class FxHandler : IDisposable
        {
            private readonly FeatureHandler _feature;
            private CancellationTokenSource _cancellationTokenSource;

            public FxHandler(FeatureHandler feature)
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