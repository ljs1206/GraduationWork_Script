using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LJS.Item.UI
{
    public class ChestSelect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _description;
        
        [SerializeField] private Image _icon;
        public ItemSOBase ItemInfo { get; private set; }

        public void ChangeItem(ItemSOBase itemInfo)
        {
            // 효과 추가 (이건 나중에)
            ItemInfo = itemInfo;

            _name.text = ItemInfo.itemName;
            _description.text = ItemInfo.description;
            // 아이콘 추가 필요
        }
    }
}
