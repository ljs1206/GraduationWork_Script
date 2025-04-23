using BIS.Data;
using BIS.Manager;
using BIS.UI.Popup;
using Main.Shared;
using UnityEngine;

namespace LJS.Map
{
    public class HealingObject : MonoBehaviour, IInteractable
    {
        public Transform UIDisplayTrm => transform;
        public Vector3 AdditionalUIDisplayPos => Vector3Int.up;
        [field:SerializeField] public string Description { get; set; }
    
        [SerializeField] private DialogueSO[] _currentTextDatas;
        private bool _isSpeaking;
        private DialoguePopupUI _dialogueUi;
        
        public void Interact(Transform Interactor)
        {
            if (_isSpeaking == true)
                return;

            _isSpeaking = true;
            
            if(_dialogueUi == null)
                _dialogueUi = Managers.UI.ShowPopup<DialoguePopupUI>();
            
            _dialogueUi.ShowText(_currentTextDatas[Random.Range(0, _currentTextDatas.Length)], isFinishMove: true);
            _dialogueUi.DialogueFinishEvent += () => _isSpeaking = false;
        }
    }
}
