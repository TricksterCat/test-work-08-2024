using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Data.Characters;
using Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Pool;
using VContainer.Unity;
using CharacterInfo = Runtime.Data.Characters.CharacterInfo;

namespace Runtime.Services.Player
{
    public class PlayerService : IDisposable
    {
        private readonly CharactersProvider _charactersProvider;
        private readonly CharacterInfoProvider _characterInfoProvider;
        private readonly List<string> _allCharacters;
        private readonly string _defaultCharacter;
        private readonly Dictionary<string, PlayerController> _players = new();

        private bool _resolveDefaultPlayers;
        private DisposableBag _disposableBag;
        
        
        public int CharactersCount => _allCharacters.Count;
        public IEnumerable<PlayerController> Players => _players.Values;



        public PlayerService(CharacterInfoProvider infoProvider, CharactersProvider charactersProvider,
            WeaponService weaponService, string defaultCharacter)
        {
            _charactersProvider = charactersProvider;
            _characterInfoProvider = infoProvider;

            _defaultCharacter = defaultCharacter;
            _allCharacters = _characterInfoProvider.EnumerableInjectedEntries().ToList();
            var defaultCharacterIndex = _allCharacters.IndexOf(_defaultCharacter);

            _resolveDefaultPlayers = defaultCharacterIndex != -1;
            
            _disposableBag = new DisposableBag();
            
            _players["Player 1"] = new PlayerController(this, weaponService, CreateUser("Player 1"),
                    defaultCharacterIndex, true, false)
                .AddTo(ref _disposableBag);
            _players["Player 2"] = new PlayerController(this, weaponService, CreateUser("Player 2"),
                    defaultCharacterIndex, false, true)
                .AddTo(ref _disposableBag);

            _characterInfoProvider.InjectedNext
                .Subscribe(OnInjected)
                .AddTo(ref _disposableBag);
            _characterInfoProvider.DisposedNext
                .Subscribe(OnDisposed)
                .AddTo(ref _disposableBag);
        }


        private PlayerInput CreateUser(string scheme)
        {
            var input = new RuntimePlayerInput();
            var player = InputUser.CreateUserWithoutPairedDevices();
            player.AssociateActionsWithUser(input);
            var controlScheme = input.controlSchemes.First(controlScheme => controlScheme.name == scheme);
            InputUser.PerformPairingWithDevice(controlScheme.PickDevicesFrom(InputSystem.devices, Keyboard.current).devices.First(), player);
            player.ActivateControlScheme(scheme);
            player.actions.Enable();
            
            return new PlayerInput(player);
        }
        
        
        private void OnInjected(string id)
        {
            _allCharacters.Add(id);
            
            if (!_resolveDefaultPlayers && id == _defaultCharacter)
            {
                _resolveDefaultPlayers = true;
                foreach (var value in _players.Values)
                {
                    value.PlayerCharacterIndex.Value = _allCharacters.Count - 1;
                }
            }
        }
        
        private void OnDisposed(string id)
        {
            using var _ = ListPool<(string, PlayerController)>.Get(out var list);
            foreach (var player in Players)
            {
                list.Add((_allCharacters[player.PlayerCharacterIndex.Value], player));
            }
            
            if(_allCharacters.Remove(id))
            {
                foreach (var tuple in list)
                {
                    tuple.Item2.PlayerCharacterIndex.OnNext(Mathf.Max(_allCharacters.IndexOf(tuple.Item1), 0));
                }
            }
        }

        public CharacterInfo ResolveCharacter(int index)
        {
            if (index < 0)
                return null;
            
            var id = _allCharacters[index];
            return _characterInfoProvider.ResolveInstant(id);
        }

        void IDisposable.Dispose()
        {
            _disposableBag.Dispose();
        }

        public bool TryPlayerSpawn(string playerTag, Vector3Int position)
        {
            if (_players.TryGetValue(playerTag, out var playerController) && playerController.Enable.Value && playerController.Character == null)
            {
                var id = _allCharacters[playerController.PlayerCharacterIndex.Value];

                var character = _charactersProvider.CreateCharacter(id, playerController, new Vector3(position.x, position.y));
                playerController.Attach(character);
                
                return true;
            }

            return false;
        }

        public PlayerController ResolvePlayer(string playerTag)
        {
            return _players[playerTag];
        }
    }
}