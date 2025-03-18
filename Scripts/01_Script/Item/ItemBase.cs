using UnityEngine;
using Main.Shared;

namespace LJS.Item
{
    public class ItemBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemSOBase _itemInfo;

        #region Interaction

        public Transform UIDisplayTrm => transform;
        public Vector3 AdditionalUIDisplayPos => Vector3.up * 1;
        [field: SerializeField] public string Description { get; set; }

        public void Interact(Transform Interactor)
        {
            _itemInfo.UseItem();
            Destroy(gameObject);
        }
        #endregion
    }
}
