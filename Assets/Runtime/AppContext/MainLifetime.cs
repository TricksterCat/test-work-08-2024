using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Runtime.Commands;
using Runtime.Common;
using Runtime.Data.Characters;
using Runtime.Data.Enemies;
using Runtime.Data.Extensions;
using Runtime.Data.Level;
using Runtime.Data.UI;
using Runtime.Data.Vfx;
using Runtime.Data.Weapons;
using Runtime.Services;
using Runtime.Services.Player;
using Runtime.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using VitalRouter.VContainer;

namespace Runtime.AppContext
{
    public class MainLifetime : LifetimeScope
    {
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private Transform _viewsRoot;
        [SerializeField]
        private string _defaultLevel;

        [SerializeField]
        private string _defaultCharacter;
        
        [SerializeField]
        private string _startupView;
        [SerializeField]
        private ViewKeyResolver _viewsResolver;
        
        private FeatureHandler _default;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterDatabase<CharactersDatabase, CharacterConfig>(CharactersDatabase.DefaultAssetName);
            builder.RegisterDatabase<EnemyDatabase, EnemyConfig>(EnemyDatabase.DefaultAssetName);
            builder.RegisterDatabase<ViewDatabase, ViewConfig>(ViewDatabase.DefaultAssetName);
            builder.RegisterDatabase<LevelsDatabase, LevelConfig>(LevelsDatabase.DefaultAssetName);
            builder.RegisterDatabase<WeaponsDatabase, WeaponConfig>(WeaponsDatabase.DefaultAssetName);
            builder.RegisterDatabase<VfxsDatabase, VfxConfig>(VfxsDatabase.DefaultAssetName);

            builder.RegisterInstance(_viewsResolver);
            builder.RegisterInstance(_camera);
            builder.RegisterInstance(Router.Default)
                .AsSelf()
                .AsImplementedInterfaces();
            
            builder.RegisterVitalRouter(routingBuilder =>
            {
                routingBuilder.Map<ViewsProvider>();
                routingBuilder.Map<CharactersProvider>();
                routingBuilder.Map<CharacterInfoProvider>();
                routingBuilder.Map<WeaponsProvider>();
                routingBuilder.Map<LevelsProvider>();
                routingBuilder.Map<EnemiesProvider>();
                routingBuilder.Map<VfxProvider>();
                
                routingBuilder.Map<ViewManager>()
                    .WithParameter("root", _viewsRoot);
                
                routingBuilder.Map<LevelService>()
                    .WithParameter("level", _defaultLevel)
                    .AsSelf()
                    .AsImplementedInterfaces();
                
                routingBuilder.Map<WeaponService>()
                    .AsSelf()
                    .As<IDisposable>();
                
                routingBuilder.Map<EnemyService>();
                routingBuilder.Map<FxService>()
                    .AsSelf()
                    .As<IDisposable>();
            });

            builder.Register<PlayerService>(Lifetime.Singleton)
                .WithParameter("defaultCharacter", _defaultCharacter);
        }
        
        private void Start()
        {
            _default = new FeatureHandler(DefaultFeatureIDs().Distinct().ToArray());
            
            UniTask.Void(async () =>
            {
                await _default.RequestAsync(Router.Default);
                await Router.Default.PublishAsync(new ShowViewCommand(_startupView));
            });
        }

        private IEnumerable<string> DefaultFeatureIDs()
        {
            foreach (var kvp in Container.Resolve<CharactersDatabase>())
            {
                if (kvp.Value.AvailableDefault)
                {
                    yield return kvp.Key;
                }
            }
            foreach (var kvp in Container.Resolve<EnemyDatabase>())
            {
                if (kvp.Value.AvailableDefault)
                {
                    yield return kvp.Key;
                }
            }

            foreach (var view in _viewsResolver)
            {
                yield return view;
            }

            yield return _startupView;
            yield return _defaultCharacter;
            yield return _defaultLevel;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _default?.Dispose();
            _default = null;
        }
    }
}
