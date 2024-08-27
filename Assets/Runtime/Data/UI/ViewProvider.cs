using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Data.Abstract;
using Runtime.UI;

namespace Runtime.Data.UI
{
    public sealed class ViewsProvider : DataProvider<View>
    {
        private readonly ViewDatabase _database;

        public ViewsProvider(ViewDatabase database)
        {
            _database = database;
        }

        public override bool CanInject(string key)
        {
            return _database.Contains(key);
        }
        
        protected override async UniTask<View> InternalInjectAsync(string key, Handler handler,
            CancellationToken resolveCancellation)
        {
            var config = _database.Get(key);
            
            var prefabHandle = handler.LoadAsset(config.Prefab);
            
            var result = await prefabHandle.ToUniTask(cancellationToken: resolveCancellation);
            if (!result.TryGetComponent<View>(out var view))
                throw new InvalidCastException($"View component not found for key: \"{key}\"");
            
            view.Prepare(config);
            if ((view.Flags & ViewFlags.UseSetActive) != 0)
            {
                result.SetActive(false);
            }
            return view;
        }
    }
}