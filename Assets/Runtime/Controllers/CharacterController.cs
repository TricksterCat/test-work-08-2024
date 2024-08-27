using System;
using R3;
using Runtime.Commands;
using Runtime.Data.Characters;
using Runtime.Data.Weapons;
using Runtime.Input;
using Runtime.Services.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Runtime.Controllers
{
    public class CharacterController : BaseEntry, RuntimePlayerInput.IPlayerControlsActions
    {
        public WeaponController ActiveWeapon => _activeWeapon;
        
        public CharacterState State { get; private set; }
        public Vector2 ShotDir { get; private set; }

        private Vector2 _size;

        private DisposableBag _destroyHandler;
        private PlayerController _player;
        private IObjectResolver _objectResolver;
        
        private WeaponController _activeWeapon;
        private Vector2 _velocity;

        private Rigidbody2D _rigidbody;
        private string _deadFx;

        private void Awake()
        {
            _size = GetComponent<SpriteRenderer>().bounds.size;
        }
        
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Setup(PlayerController controller, CharacterConfig config, IObjectResolver objectResolver, IDisposable disposable)
        {
            State = new CharacterState(config);
            
            _player = controller;
            _objectResolver = objectResolver;

            _deadFx = config.DeadFx;
            
            _destroyHandler = new DisposableBag();
            controller.Weapon.WhereNotNull().Subscribe(OnWeaponChanged).AddTo(ref _destroyHandler);
            controller.Input.Subscribe(this).AddTo(ref _destroyHandler);
            Disposable.Create(OnDisposeWeapon).AddTo(ref _destroyHandler);
            disposable.AddTo(ref _destroyHandler);
        }

        private void OnDisposeWeapon()
        {
            if (_activeWeapon != null)
            {
                Destroy(_activeWeapon.gameObject);
            }
        }

        private void OnWeaponChanged(WeaponState state)
        {
            OnDisposeWeapon();
            _activeWeapon = _objectResolver.Instantiate(state.Model.ControllerPrefab, transform);
            _activeWeapon.Setup(state);
        }

        public override Rect ResolveRect()
        {
            return new Rect(Vector2.zero, _size)
            {
                center = transform.position
            };
        }

        public override bool PushDamage(float value)
        {
            State.Hp -= value * (1f - State.Config.Defense);
            if (State.Hp < 0)
            {
                Destroy();
                return false;
            }

            return true;
        }

        public void OnBeginUpdate(float time, float deltaTime)
        {
            if (_rigidbody != null)
            {
                _rigidbody.velocity = _velocity * State.Config.Speed;
            }

            _activeWeapon.OnBeginUpdate();
        }
        
        public void OnCompleteUpdate()
        {
            _activeWeapon.OnCompleteUpdate();
        }


        public override void Destroy()
        {
            if (!string.IsNullOrEmpty(_deadFx))
            {
                Router.Default.Enqueue(new PlayVfxCommand(_deadFx, transform.position));
            }
            _destroyHandler.Dispose();
        }

        void RuntimePlayerInput.IPlayerControlsActions.OnMovement(InputAction.CallbackContext context)
        {
            _velocity = context.ReadValue<Vector2>();
            
            if(Mathf.Abs(_velocity.x) + Mathf.Abs(_velocity.y) > 0.1f)
                ShotDir = _velocity.normalized;
        }

        void RuntimePlayerInput.IPlayerControlsActions.OnFire(InputAction.CallbackContext context)
        {
            if (_activeWeapon.CanShot)
            {
                _activeWeapon.Shot(transform.position, ShotDir);
            }
        }

        void RuntimePlayerInput.IPlayerControlsActions.OnLeftWeapon(InputAction.CallbackContext context)
        {
            _player.NextWeapon(-1);
        }

        void RuntimePlayerInput.IPlayerControlsActions.OnRightWeapon(InputAction.CallbackContext context)
        {
            _player.NextWeapon(1);
        }

        void RuntimePlayerInput.IPlayerControlsActions.OnJoin(InputAction.CallbackContext context)
        {
        }
    }
}