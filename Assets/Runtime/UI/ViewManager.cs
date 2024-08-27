using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Runtime.Commands;
using Runtime.Data.UI;
using UnityEngine;
using VContainer;
using VitalRouter;
using Object = UnityEngine.Object;

namespace Runtime.UI
{
    [Routes]
    public partial class ViewManager
    {
        private readonly IObjectResolver _resolver;
        private readonly ViewsProvider _viewsProvider;
        private readonly Transform _root;

        private View _currentScreen;
        private readonly List<View> _active = new();
        private readonly ViewKeyResolver _viewResolver;

        public View CurrentScreen => _currentScreen;
        
        public ViewManager(Transform root, ViewKeyResolver viewResolver, ViewsProvider viewsProvider, IObjectResolver resolver)
        {
            _viewResolver = viewResolver;
            _resolver = resolver;
            _viewsProvider = viewsProvider;
            _root = root;
        }

        [Route]
        private async UniTask On(ShowViewCommand command)
        {
            var prefab = await _viewsProvider.ResolveAsync(command.ID);
            var view = Object.Instantiate(prefab, _root);
            view.Initialize(command.ID);
            await view.ShowAsync();
        }
        
        [Route]
        private UniTask On(HideViewCommand command)
        {
            var view = _active.Find(v => string.Equals(v.ID, command.ID, StringComparison.Ordinal));
            if (view != null)
            {
                _active.Remove(view);
                if (view == _currentScreen)
                {
                    _currentScreen = null;
                }
                return view.HideAsync();
            }

            return UniTask.CompletedTask;
        }

        [Route]
        private async UniTask On(ViewStartedCommand command)
        {
            _resolver.Inject(command.Instance);
            
            _active.Add(command.Instance);
            switch (command.Instance.Type)
            {
                case ViewType.Screen:
                    var last = _currentScreen;
                    if(command.Instance == last)
                        return;

                    if (!command.Instance.IsShown)
                    {
                        await UniTask.WaitUntil(() => command.Instance.IsShown);
                    }
                    
                    _currentScreen = command.Instance;
                    if (last != null)
                    {
                        _active.Remove(last);
                        last.HideAsync().Forget();
                    }
                    break;
            }
        }

        [Route]
        private void On(RequestViewCommand command)
        {
            if (_viewResolver.TryResolve(command.View, out var id))
            {
                Router.Default.Enqueue(new ShowViewCommand(id));
            }
        }
        
        [Route]
        private void On(MatchStatusChangedCommand command)
        {
            Router.Default.Enqueue(command.Status
                ? new RequestViewCommand(ViewKey.Game)
                : new RequestViewCommand(ViewKey.GameOver));
        }
    }
}