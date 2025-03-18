using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LJS.Item.UI
{
    public class SelectUI : MonoBehaviour
    {
        [SerializeField] private ItemDataTableSO _itemTableSO;
        
        private List<ChestSelect> _selectViewList;
        private List<ItemSOBase> _selectedList;
        
        private GraphicRaycaster _caster;
        private PointerEventData _pointerData;
        private List<RaycastResult> _raycastResult;

        public bool CanCastNow { get; private set; }
        public bool SelectingNow { get; private set; }

        private void Awake()
        {
            _caster = transform.root.GetComponent<GraphicRaycaster>();
            
            _selectViewList = new();
            _selectedList = new();
            _raycastResult = new();
        }

        private void Start()
        {
            _selectViewList.Clear();
            _selectViewList = transform.GetComponentsInChildren<ChestSelect>().ToList();

            SelectingNow = false;
        }
        
        private void Update()
        {
            if (CanCastNow && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _pointerData = new PointerEventData(EventSystem.current);
                _pointerData.position = Input.mousePosition;
                _caster.Raycast(_pointerData, _raycastResult);

                if (_raycastResult.Count > 0)
                {
                    // 선택 완료 시....
                    _raycastResult[0].
                        gameObject.GetComponent<ChestSelect>().ItemInfo.UseItem();
                    
                    Debug.Log(_raycastResult[0].
                        gameObject.GetComponent<ChestSelect>().ItemInfo.itemName);
                    CanCastNow = false;
                    WindowOut();
                }
            }
        }

        public void SelectStart()
        {
            if (SelectingNow) return;
            SelectingNow = true;
            
            ItemSOBase item;
            _selectedList.Clear();
            
            foreach (ChestSelect select in _selectViewList)
            {
                while (true)
                {
                    item = _itemTableSO.RandomItemOutput();
                    if (!_selectedList.Contains(item))
                    {
                        select.ChangeItem(item);
                        _selectedList.Add(item);
                        break;
                    }
                }
            }
            
            // 이동 연출 필요
            WindowIn();
        }

        private void WindowIn()
        {
            // Lerp 이용해서 만들기 클릭 방지도 만들어야 할듯??
            transform.DOMoveY(transform.position.y - Screen.height, 1f)
                .SetEase(Ease.OutCubic).OnComplete(() => CanCastNow = true);
            
        }

        public void WindowOut() // 선택지 선택후 불러올 예정?
        {
            transform.DOMoveY(transform.position.y + Screen.height, 1f)
                .SetEase(Ease.OutCubic).OnComplete(() => SelectingNow = false);
        }
    }
}
