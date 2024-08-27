using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Commands;
using VitalRouter;

namespace Runtime.Common
{
    public class FeatureHandler : IDisposable
    {
        private static readonly Func<FeatureBindingRequestCommand> BindingCommandFactory = () => new FeatureBindingRequestCommand();
        
        private Status _status;
        
        public enum Status
        {
            None,
            Requested,
            Disposed
        }
        
        public readonly string[] Requested;
        public event Action Disposed;

        public FeatureHandler(string[] requested)
        {
            Requested = requested;
        }

        public void Dispose()
        {
            if(_status is not Status.Requested)
                return;

            _status = Status.Disposed;
            
            Disposed?.Invoke();
            Disposed = null;
        }

        public async UniTask RequestAsync(ICommandPublisher publisher, CancellationToken cancellation = default)
        {
            if(_status is Status.Requested)
                return;
            
            _status = Status.Requested;
            
            var pool = CommandPool<FeatureBindingRequestCommand>.Shared;
            
            //TODO: warnings or error when request not have (DependenciesCounts == 0)
            await UniTask.WhenAll(Requested.Select(id =>
            {
                var command = pool.Rent(BindingCommandFactory);
                command.Initialize(this, id);
                return publisher.PublishAsync(command, cancellation);
            }));
        }

    }
}