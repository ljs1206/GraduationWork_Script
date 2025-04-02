using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using KHJ.SO;
using LJS.Item;
using Main.Runtime.Core.Events;
using Main.Runtime.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BIS.Core.Utility;
using Main.Core;

namespace LJS.UI
{
    public enum RewardType
    {
        Item,
        Synergy
    }

    public class RewardPanel : MonoBehaviour
    {
        [Header("Base Setting")]
        [SerializeField]
        private List<Transform> _lineList = new();

        [SerializeField] private RectTransform _itemCartalog;
        [SerializeField] private ToolTip _toolTipCompo;
        [SerializeField] private Canvas _canvas;

        [Header("Icon Setting")]
        [SerializeField]
        private float _iconWidth;

        [SerializeField] private float _iconHeight;

        [Header("Text Setting")]
        [SerializeField]
        private TMP_FontAsset _font;

        [SerializeField] private float _nameTagWidth;
        [SerializeField] private float _nameTagHeight;
        [SerializeField] private float _fontSize;
        [SerializeField] [ColorUsage(true)] private Color _textColor;

        [SerializeField] private float _nameTagPosY;

        private float _panelWidth;
        private bool isOpen;
        private bool closeNow;

        private GameEventChannelSO _gameEventChannel;
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _gameEventChannel = AddressableManager.Load<GameEventChannelSO>("GameEventChannel");
            _tokenSource = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        private void Start()
        {
            isOpen = false;
            closeNow = false;
            _panelWidth = _itemCartalog.rect.width;
        }

        public void OpenPanel(List<ScriptableObject> list)
        {
            if (isOpen) return;

            isOpen = true;
            //Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            WindowIn(() => OpenPanelCoro(list));
        }

        public void ClosePanel()
        {
            if (closeNow) return;

            closeNow = true;
            isOpen = false;
            WindowOut();
            _toolTipCompo.ToolTipClose();
        }

        public void ResetPanel()
        {
            if (_lineList[0].childCount <= 0) return;

            for (int i = _lineList.Count - 1; i >= 0; i--)
            {
                Destroy(_lineList[i].gameObject);
            }

            closeNow = false;
            transform.position = new Vector3(0, 1080, 0);
        }

        private async Task OpenPanelCoro(List<ScriptableObject> list = null)
        {
            float iconWidthSum = 0;
            int lineNumber = 0;
            // 등장 효과 ex) 위에서 아래로 중아에서 크기 늘리기
            // 아이템 나올 때 효과.. 결과 창에 하나씩 딱딱 등장하는 느낌? 만들기
            foreach (var element in list)
            {
                ItemSOBase item = null;
                SynergySO synergy = null;
                if (element is ItemSOBase)
                    item = element as ItemSOBase;
                else
                    synergy = element as SynergySO;

                iconWidthSum += _iconWidth;

                if (iconWidthSum >= _panelWidth - 220)
                // horizontal layOut Group의 spacing과 vertical // 의 padding 고려 수치. 대략적임 수정 필요
                {
                    iconWidthSum = 0;
                    lineNumber++;
                }

                #region Generate

                GameObject icon = new GameObject();
                ToolTipAvailableObject tooltipCompo = icon.AddComponent<ToolTipAvailableObject>();
                RectTransform rectTrm = icon.AddComponent<RectTransform>();
                Image imageCompo = icon.AddComponent<Image>();

                imageCompo.sprite = element is ItemSOBase ? item.icon : synergy.ItemIcon;
                imageCompo.raycastPadding = new Vector4(0, 0, 0, 10);
                rectTrm.sizeDelta = new Vector2(_iconWidth, _iconHeight);

                icon.transform.SetParent(_lineList[lineNumber]);

                GameObject nameTag = new GameObject();

                nameTag.transform.SetParent(icon.transform);
                tooltipCompo.SettingInfo(element, _toolTipCompo, _canvas); // 기물도 추가해야 됨...

                RectTransform textTrm = nameTag.AddComponent<RectTransform>();
                TextMeshProUGUI textCompo = nameTag.AddComponent<TextMeshProUGUI>();

                textTrm.sizeDelta = new Vector2(_nameTagWidth, _nameTagHeight);
                textTrm.position = new Vector3(0, _nameTagPosY, 0);

                textCompo.font = _font;
                textCompo.text = element is ItemSOBase ? item.name : synergy.ItemName;
                textCompo.fontSize = _fontSize;
                textCompo.color = _textColor;

                #endregion

                await Task.Delay(300, _tokenSource.Token);

                // yield return new WaitForSecondsRealtime(0.3f);
            }
        }

        private void WindowIn(Action endEvent)
        {
            // Lerp 이용해서 만들기 클릭 방지도 만들어야 할듯??
            Time.timeScale = 0;
            transform.DOLocalMoveY(0, 0).SetUpdate(true);
            Util.UIFadeOut(gameObject, false, onCompleteCallBack: () => endEvent?.Invoke());
            //transform.DOLocalMoveY(0, 1f)
            //    .SetEase(Ease.OutCubic).OnComplete(() => endEvent?.Invoke()).SetUpdate(true);
        }

        public void WindowOut() // 선택지 선택후 불러올 예정?
        {
            // transform.DOMoveY(transform.position.y + Screen.height, 1f)
            //     .SetEase(Ease.OutCubic).SetUpdate(true);
            var evt = GameEvents.EndBattle;
            _gameEventChannel.RaiseEvent(evt);
        }
    }
}