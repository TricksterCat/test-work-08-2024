using System;
using System.Linq;
using R3;
using Runtime.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Runtime.Services.Player
{
    public class PlayerInput : IDisposable
    {
        public string Tag => _inputGroup;
        
        private InputUser _input;
        
        private readonly string _inputGroup;
        private readonly RuntimePlayerInput _runtimePlayerInput;

        private bool _disposed;
        
        public PlayerInput(InputUser input)
        {
            _input = input;
            _inputGroup = input.controlScheme!.Value.bindingGroup;

            _runtimePlayerInput = (RuntimePlayerInput)input.actions;
        }
        
        public IDisposable ResolveAction(string key, Action<InputAction.CallbackContext> callback)
        {
            var input = _input.actions.First(action => action.name == key);
            return Observable.FromEvent<InputAction.CallbackContext>(
                    action => input.started += action,
                    action => input.started -= action)
                .Subscribe(callback);
        }

        public string ResolveActionBindingName(string name)
        {
            var action = _input.actions.First(action => action.name == name);
            return action.bindings.First(binding => binding.groups.Contains(_inputGroup, StringComparison.Ordinal)).ToDisplayString();
        }

        void IDisposable.Dispose()
        {
            if(_disposed)
                return;
            _disposed = true;
            _runtimePlayerInput?.Dispose();
            _input.UnpairDevicesAndRemoveUser();
        }

        public IDisposable Subscribe(RuntimePlayerInput.IPlayerControlsActions playerControl)
        {
            _runtimePlayerInput.PlayerControls.AddCallbacks(playerControl);
            return Disposable.Create(playerControl, actions => _runtimePlayerInput.PlayerControls.RemoveCallbacks(actions));
        }
    }
}