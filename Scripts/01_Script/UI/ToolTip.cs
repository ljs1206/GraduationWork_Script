using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

namespace LJS.UI
{
    public class ToolTip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameTextField;
        [SerializeField] private TextMeshProUGUI _descriptionTextField;

        private RectTransform _rectTrm;

        private void Start()
        {
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ToolTipClose();
            }
        }

        public void ToolTipOpen(string name, string description)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.2f).SetUpdate(true);
            _rectTrm = GetComponent<RectTransform>();
            Vector3 mousePos = Input.mousePosition;
            
            transform.position = 
                new Vector3(mousePos.x + _rectTrm.rect.width / 2, mousePos.y + _rectTrm.rect.height / 2, 0);
            _nameTextField.text = name;
            _descriptionTextField.text = description;
        }

        public void ToolTipClose()
        {
            transform.DOScale(Vector3.zero, 0.2f)
                .SetUpdate(true);
        }
    }
}
