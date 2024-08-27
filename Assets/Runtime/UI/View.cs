using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Runtime.Commands;
using Runtime.Data.UI;
using UnityEngine;
using VitalRouter;

namespace Runtime.UI
{
    public class View : MonoBehaviour
    {
        [SerializeField]
        private ViewFlags _flags;
        [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField]
        private bool _destroyOnHide;

        [SerializeField]
        private ViewType _type;
        
        public string ID { get; private set; }
        
        public ViewType Type => _type;
        public bool IsShown { get; protected set; }
        public ViewFlags Flags => _flags;
        
        public virtual void Prepare(ViewConfig config)
        {
            _destroyOnHide = config.DestroyOnHide;
            if (_canvasGroup != null && (_flags & ViewFlags.ShownByDefault) == 0)
            {                
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void Initialize(string id)
        {
            ID = id;
        }

        protected virtual void Awake()
        {
            IsShown = (_flags & ViewFlags.ShownByDefault) != 0;
        }

        protected virtual void Start()
        {
            Router.Default.Enqueue(new ViewStartedCommand(this));
        }

        public async UniTask ShowAsync()
        {
            await OnShowAsync();
            IsShown = true;
        }

        public async UniTask HideAsync()
        {
            await OnHideAsync();
            IsShown = false;
        }

        protected virtual async UniTask OnShowAsync()
        {
            if ((_flags & ViewFlags.UseSetActive) != 0)
            {
                gameObject.SetActive(true);
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                if ((_flags & ViewFlags.InstantShow) == 0)
                {
                    await _canvasGroup.DOFade(1f, 0.2f).ToUniTask();
                }
            }
        }
        
        protected virtual async UniTask OnHideAsync()
        {
            if (_canvasGroup != null)
            {
                if ((_flags & ViewFlags.InstantHide) == 0)
                {
                    await _canvasGroup.DOFade(0, 0.2f).ToUniTask();
                }
                _canvasGroup.blocksRaycasts = false;
            }
            
            if (_destroyOnHide)
            {
                Destroy(gameObject);
            }
            else if ((_flags & ViewFlags.UseSetActive) != 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}