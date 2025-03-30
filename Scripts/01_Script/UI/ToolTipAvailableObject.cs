using System.Collections.Generic;
using KHJ.SO;
using LJS.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KHJ.SO;

namespace LJS.UI
{
    public class ToolTipAvailableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ItemSOBase _itemInfo;
        private SynergySO _synergyInfo;

        private GraphicRaycaster _caster;
        private PointerEventData _pointerData;
        private List<RaycastResult> _raycastResult;
        private ToolTip _toolTipCompo;

        private RewardType _rewardType;

        public void SettingInfo(ScriptableObject item, ToolTip toolTipCompo, Canvas canvas)
        {
            if (item is ItemSOBase)
            {
                _itemInfo = item as ItemSOBase;
                _rewardType = RewardType.Item;
            }
            else
            {
                _synergyInfo = item as SynergySO;
                _rewardType = RewardType.Synergy;
            }

            _toolTipCompo = toolTipCompo;
            _caster = canvas.GetComponent<GraphicRaycaster>();
            _raycastResult = new();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipOpen();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _toolTipCompo.ToolTipClose();
        }

        private void ToolTipOpen()
        {
            _pointerData = new PointerEventData(EventSystem.current);
            _pointerData.position = Input.mousePosition;
            _caster.Raycast(_pointerData, _raycastResult);

            if (_raycastResult.Count <= 0) return;

            foreach (var hit in _raycastResult)
            {
                if (hit.gameObject == gameObject)
                {
                    // 툴팁 띄우기
                    if (_rewardType == RewardType.Item)
                        _toolTipCompo.ToolTipOpen(_itemInfo.itemName, _itemInfo.description);
                    else
                        _toolTipCompo.ToolTipOpen(_synergyInfo.ItemName, _synergyInfo.ItemDescription);
                }
            }
        }
    }
}