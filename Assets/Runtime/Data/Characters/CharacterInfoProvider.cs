using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Commands;
using Runtime.Data.Abstract;
using VitalRouter;

namespace Runtime.Data.Characters
{
    public class CharacterInfoProvider : DataProvider<CharacterInfo>
    {
        private readonly CharactersDatabase _database;

        public CharacterInfoProvider(CharactersDatabase database)
        {
            _database = database;
        }

        public override bool CanInject(string key) => _database.Contains(key);
        
        protected override async UniTask<CharacterInfo> InternalInjectAsync(string key, Handler handler, CancellationToken cancellationToken)
        {
            var config = _database.Get(key);
            var preview = await handler.LoadAsset(config.Preview);
            
            await Router.Default.PublishAsync(new CharacterInjectingCommand(key), cancellationToken);
            
            return new CharacterInfo(config.Name, config.Description, preview, config.Weapons);
        }
    }
}