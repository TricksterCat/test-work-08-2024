using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using Runtime.Services.Player;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Runtime.UI.Impl.Lobby
{
    [DeclareTabGroup("Tabs")]
    public class PlayerInfoView : MonoBehaviour
    {
        [SerializeField]
        [Group("Tabs"), Tab("Active")]
        private CanvasGroup _activeGroup;
        [SerializeField]
        [Group("Tabs"), Tab("Active")]
        private Button _left;
        [SerializeField]
        [Group("Tabs"), Tab("Active")]
        private Button _right;
        [SerializeField]
        [Group("Tabs"), Tab("Active")]
        private Button _dettach;

        [SerializeField]
        [Group("Tabs"), Tab("Active")]
        private Image _preview;

        [SerializeField]
        [Group("Tabs"), Tab("Active")]
        private TextMeshProUGUI _label;

        private bool _canDisable;

        [SerializeField]
        [Group("Tabs"), Tab("Disabled")]
        private CanvasGroup _disabledGroup;
        [SerializeField]
        [Group("Tabs"), Tab("Disabled")]
        private TextMeshProUGUI _joinLabel;

        private PlayerController _playerController;
        private IDisposable _input;
        public void Initialize(string label, PlayerController playerController)
        {
            _playerController = playerController;
            _label.text = label;
            _canDisable = playerController.CanDisable;
            
            _activeGroup.blocksRaycasts = false;
            _disabledGroup.blocksRaycasts = false;
            
            _dettach.gameObject.SetActive(playerController.CanDisable);
            playerController.Enable.Subscribe(OnSetActiveChanged).AddTo(destroyCancellationToken);
            playerController.CharacterInfo.WhereNotNull().Subscribe(info =>
            {
                _preview.sprite = info.Preview;
                _label.text = $"{label}\n{info.Name}";
            }).AddTo(destroyCancellationToken);

            
            _joinLabel.text = $"Press \"{_playerController.Input.ResolveActionBindingName("Join")}\" to join";
        }

        private void InjectInput()
        {
            _input = _playerController.Input.ResolveAction("Movement", OnMoveAction);
        }

        private void OnMoveAction(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            NextCharacter((int)Mathf.Sign(value.x));
        }

        public void NextCharacter(int diff)
        {
            if(!_playerController.Enable.Value)
                return;
            _playerController.NextCharacter(diff);
        }

        public void SetActive(bool value)
        {
            if(!value && !_playerController.CanDisable)
                return;
            
            _playerController.Enable.OnNext(value);
        }

        private void OnDestroy()
        {
            _input?.Dispose();
            _input = null;
        }

        private void OnSetActiveChanged(bool value)
        {
            _input?.Dispose();
            if (value)
            {
                InjectInput();
                
                _disabledGroup.blocksRaycasts = false;
                _disabledGroup.DOFade(0f, 0.1f);
                _activeGroup.DOFade(1f, 0.1f).OnComplete(() => _activeGroup.blocksRaycasts = true);
            }
            else if(_canDisable)
            {
                _input = null;
                
                _activeGroup.blocksRaycasts = false;
                _disabledGroup.DOFade(1f, 0.1f).OnComplete(() => _disabledGroup.blocksRaycasts = true);
                _activeGroup.DOFade(0f, 0.1f);
            }
        }
    }
}