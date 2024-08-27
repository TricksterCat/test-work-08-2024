using System;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Data.Characters;
using Runtime.Data.Weapons;
using Runtime.Services.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Impl.Game
{
    public class CharacterStatsView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _nameTagLabel;

        [SerializeField]
        private Image _weaponIcon;

        [SerializeField]
        private Slider _weaponRecharge;

        [SerializeField]
        private Slider _hpBar;

        [SerializeField]
        private TextMeshProUGUI _hpValue;

        private CharacterState _characterState;
        private WeaponState _weaponState;
        private float _lastHp;

        public void Initialize(PlayerController playerController)
        {
            gameObject.SetActive(playerController.Enable.Value);
            if (!playerController.Enable.Value)
                return;

            _characterState = playerController.Character.State;
            _nameTagLabel.text = playerController.Tag;
            
            _hpBar.maxValue = _characterState.Config.Hp;
            InternalUpdateHp((int)_characterState.Config.Hp);

            playerController.Weapon.Subscribe(weapon =>
                {
                    _weaponIcon.sprite = weapon.Model.SpritePreview;
                    _weaponRecharge.maxValue = weapon.Model.Recharge;
                    _weaponState = weapon;
                })
                .AddTo(destroyCancellationToken);

            Observable.Interval(TimeSpan.FromMilliseconds(50))
                .Subscribe(OnGUIUpdate)
                .AddTo(destroyCancellationToken);
        }

        private void OnGUIUpdate(Unit obj)
        {
            var weapon = _weaponState;
            _weaponRecharge.value = Mathf.Max(weapon.CooldownComplete - Time.time, 0f);

            var clampedHp = Mathf.Clamp(_characterState.Hp, 0f, _hpBar.maxValue);
            if (Mathf.Abs(clampedHp - _lastHp) < 0.001)
            {
                return;
            }

            _lastHp = clampedHp;

            InternalUpdateHp((int)clampedHp);
        }

        private void InternalUpdateHp(int value)
        {
            _hpBar.value = value;
            _hpValue.text = $"{value} / {_hpBar.maxValue}";
        }
    }
}