using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using R3;
using Runtime.Data.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using CharacterController = Runtime.Controllers.CharacterController;
using CharacterInfo = Runtime.Data.Characters.CharacterInfo;

namespace Runtime.Services.Player
{
    public class PlayerController : IDisposable
    {
        public bool CanDisable { get; }
        public ReactiveProperty<bool> Enable { get; }
        public ReactiveProperty<int> PlayerCharacterIndex => _playerCharacterIndex;
        public ReadOnlyReactiveProperty<CharacterInfo> CharacterInfo { get; }
        public ReadOnlyReactiveProperty<WeaponState> Weapon => _playerWeaponState;

        public CharacterController Character => _character;

        public PlayerInput Input => _input;
        public string Tag => _input.Tag;

        private readonly PlayerService _playerService;
        private readonly WeaponService _weaponService;
        
        private readonly ReactiveProperty<int> _playerCharacterIndex;
        private readonly ReactiveProperty<WeaponState> _playerWeaponState;

        private readonly PlayerInput _input;
        private CharacterController _character;
        
        private int _playerWeaponIndex;
        private WeaponModel[] _weapons;
        private WeaponState[] _weaponStates;
        
        private DisposableBag _disposableBag;

        public PlayerController(PlayerService playerService, WeaponService weaponService, 
            PlayerInput input, int characterIndex, bool activeByDefault, bool canDisable)
        {
            _input = input;

            _disposableBag = new DisposableBag();
            
            _playerService = playerService;
            _playerCharacterIndex = new ReactiveProperty<int>(characterIndex);
            _playerWeaponState = new ReactiveProperty<WeaponState>();
            
            CanDisable = canDisable;
            _weaponService = weaponService;
            Enable = new ReactiveProperty<bool>(activeByDefault);

            CharacterInfo = _playerCharacterIndex
                .Select(OnUpdateCharacter)
                .ToReadOnlyReactiveProperty();
            
            _disposableBag.Add(_input);
            CharacterInfo.WhereNotNull().Subscribe(OnCharacterChange).AddTo(ref _disposableBag);
            input.ResolveAction("Join", OnJoin).AddTo(ref _disposableBag);
        }

        private void OnCharacterChange(CharacterInfo characterInfo)
        {
            _weapons = characterInfo.Weapons.Select(_weaponService.ResolveWeapon).ToArray();
            
            _playerWeaponIndex = 0;
        }

        private void OnJoin(InputAction.CallbackContext context)
        {
            if (!Enable.Value)
            {
                Enable.OnNext(true);
            }
        }
        
        public void NextCharacter(int diff)
        {
            var next = (_playerCharacterIndex.Value + diff) % _playerService.CharactersCount;
            if (next < 0)
            {
                next = _playerService.CharactersCount + next;
            }
            
            _playerCharacterIndex.OnNext(next);
        }

        public void NextWeapon(int diff)
        {
            var next = (_playerWeaponIndex + diff) % _weapons.Length;
            if (next < 0)
            {
                next = _weapons.Length + next;
            }

            _playerWeaponIndex = next;
            _playerWeaponState.OnNext(_weaponStates[next]);
        }

        private CharacterInfo OnUpdateCharacter(int index)
        {
            return _playerService.ResolveCharacter(index);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }

        public void Attach(CharacterController controller)
        {
            _weaponStates = new WeaponState[_weapons.Length];
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weaponStates[i] = new WeaponState(_weapons[i]);
            }
            _playerWeaponState.OnNext(_weaponStates[_playerWeaponIndex]);
            _character = controller;
        }
    }
}