using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Commands;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using VitalRouter;

namespace Runtime.Data.Abstract
{
    [Routes]
    public abstract partial class DataProvider<TModel>
    {
        public Subject<string> InjectedNext { get; }= new();
        public Subject<string> DisposedNext { get; } = new();
        
        private readonly Dictionary<string, Handler> _handlers = new();
        
        public abstract bool CanInject(string key);
        
        protected abstract UniTask<TModel> InternalInjectAsync(string key, Handler handler,
            CancellationToken cancellationToken);

        public IEnumerable<string> EnumerableInjectedEntries()
        {
            foreach (var handler in _handlers)
            {
                if (handler.Value.Refs > 0)
                {
                    yield return handler.Key;
                }
            }
        }
        
        public virtual bool CanResolve(string key)
        {
            return _handlers.TryGetValue(key, out var handler) &&
                   handler.CompletionSource.UnsafeGetStatus() is UniTaskStatus.Succeeded;
        }

        public TModel ResolveInstant(string key) => _handlers[key].Value;

        protected virtual void OnInjected(string key, Handler handler)
        {
            InjectedNext.OnNext(key);
        }

        protected virtual void OnDisposing(string key, Handler handler)
        {
            DisposedNext.OnNext(key);
        }

        public async UniTask<TModel> ResolveAsync(string key)
        {
            if (!_handlers.TryGetValue(key, out var handler))
            {
                throw new Exception($"\"{key}\" is not injected in {GetType().FullName}!");
            }

            if (handler.CompletionSource.UnsafeGetStatus() is not UniTaskStatus.Succeeded)
            {
                await handler.CompletionSource.Task;
            }

            return handler.Value;
        }

        [Route]
        private async UniTask On(FeatureBindingRequestCommand request)
        {
            var key = request.ID;
            if (!CanInject(key))
            {
                return;
            }
            
            if (!_handlers.TryGetValue(key, out var handler))
            {
                _handlers[key] = handler = new Handler(key, OnDisposing);
            }

            request.Use();
            handler.Increment(out var needLoading);
            request.FeatureHandler.Disposed += handler.Decrement;
            
            if(!needLoading)
                return;
            
            try
            {
                var value = await InternalInjectAsync(key, handler, handler.ResolveCancellation());
                handler.Success(value);
                
                OnInjected(key, handler);
            }
            catch (Exception e)
            {
                handler.Failed(e);
                Debug.LogException(e);
            }
        }

        class InjectException : Exception
        {
            private readonly Exception _inner;
            
            public InjectException(string key, Exception inner) : base($"Failed inject resource: \"{key}\"!")
            {
                _inner = inner;
            }

            public override string StackTrace => _inner?.StackTrace ?? base.StackTrace;
        }
        
        protected sealed class Handler : IDisposable
        {
            public int Refs { get; private set; }
            public TModel Value { get; private set; }

            public UniTaskCompletionSource CompletionSource { get; private set; } = new();

            private readonly string _key;
            private readonly Action<string, Handler> _disposedCallback;
            private readonly List<AsyncOperationHandle> _handles = new();
            private CancellationTokenSource _cts = new();


            public Handler(string key, Action<string, Handler> disposedCallback)
            {
                _key = key;
                _disposedCallback = disposedCallback;
            }

            public CancellationToken ResolveCancellation() => _cts.Token;

            public void Increment(out bool needLoading)
            {
                needLoading = Refs == 0;
                Refs++;
            }
            
            public void Decrement()
            {
                Assert.AreNotEqual(Refs, 0);
                
                Refs--;
                if (Refs == 0)
                {
                    Dispose();
                }
            }

            public void Success(TModel model)
            {
                Value = model;
                CompletionSource.TrySetResult();
            }

            public void Failed(Exception exception)
            {
                CompletionSource.TrySetException(new InjectException(_key, exception));
            }

            public void Dispose()
            {
                _cts?.Dispose();
                _cts = new();
                _disposedCallback?.Invoke(_key, this);
                
                foreach (var handle in _handles)
                {
                    if(handle.IsValid())
                        Addressables.Release(handle);
                }
                _handles.Clear();
                if (CompletionSource.UnsafeGetStatus() != UniTaskStatus.Pending)
                    CompletionSource = new();
            }

            public AsyncOperationHandle<TValue> LoadAsset<TValue>(AssetReferenceT<TValue> assetReference)
                where TValue : UnityEngine.Object
            {
                return LoadAssetUnsafe<TValue>(assetReference);
            }
            
            public AsyncOperationHandle<TValue> LoadAssetUnsafe<TValue>(AssetReference assetReference)
                where TValue : UnityEngine.Object
            {
                var handle = assetReference.LoadAssetAsync<TValue>();
                _handles.Add(handle);
                return handle;
            }
        }
    }
}