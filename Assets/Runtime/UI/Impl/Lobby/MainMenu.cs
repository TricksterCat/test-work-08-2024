using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using Runtime.Commands;
using Runtime.Services;
using Runtime.Services.Player;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VitalRouter;

namespace Runtime.UI.Impl.Lobby
{
    [DeclareBoxGroup("Binding")]
    public class MainMenu : View
    {
        [SerializeField]
        [Group("Binding")]
        private CanvasGroup _buttonsBox;
        
        [SerializeField]
        [Group("Binding")]
        private Button _playBtn;

        [SerializeField]
        [Group("Binding")]
        private TMP_InputField _maxSpawnInput;

        [SerializeField]
        [Group("Binding")]
        private PlayerInfo[] _players;
        
        [Serializable]
        private struct PlayerInfo
        {
            public string Tag;
            public PlayerInfoView View;
        }

        private bool _playClicked;
        public void OnPlayClicked()
        {
            if(_playClicked)
                return;
            _playClicked = true;
            
            Router.Default.Enqueue(new PressStartLevelCommand());
        }
        
        [Inject]
        private void OnInjecting(EnemyService enemyService, PlayerService playerService)
        {
            enemyService.SpawnMaxUnits
                .Subscribe(value => _maxSpawnInput.text = value.ToString())
                .AddTo(destroyCancellationToken);

            _maxSpawnInput.onValueChanged.AddListener(value =>
            {
                if (!int.TryParse(value.Trim(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
                {
                    result = enemyService.SpawnMaxUnits.Value;
                }
                enemyService.SpawnMaxUnits.OnNext(result);
            });

            foreach (var player in _players)
            {
                player.View.Initialize(player.Tag, playerService.ResolvePlayer(player.Tag));
            }
        }

        protected override UniTask OnHideAsync()
        {
            _playClicked = false;
            return base.OnHideAsync();
        }

        protected override async UniTask OnShowAsync()
        {
            var buttonsRectTransform = (RectTransform)_buttonsBox.transform;
            var lastPosition = buttonsRectTransform.anchoredPosition;
            buttonsRectTransform.anchoredPosition = new Vector2(lastPosition.x, lastPosition.y -100);
            _buttonsBox.alpha = 0;
            
            await base.OnShowAsync();
            
            _buttonsBox.DOFade(1f, 0.25f);
            buttonsRectTransform.DOAnchorPosY(lastPosition.y, 0.25f);
        }
    }
}