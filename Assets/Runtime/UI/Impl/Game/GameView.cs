using System;
using Runtime.Services;
using Runtime.Services.Player;
using UnityEngine;
using VContainer;

namespace Runtime.UI.Impl.Game
{
    public class GameView : View
    {
        [SerializeField]
        private CharacterWidget[] _characterWidgets;
        
        [Serializable]
        private struct CharacterWidget
        {
            public string Tag;
            public CharacterStatsView View;
        }

        [Inject]
        private void OnInjecting(PlayerService playerService)
        {
            foreach (var widget in _characterWidgets)
            {
                var playerController = playerService.ResolvePlayer(widget.Tag);
                widget.View.Initialize(playerController);
            }
        }
    }
}