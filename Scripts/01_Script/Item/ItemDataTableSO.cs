using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace LJS.Item
{
    [CreateAssetMenu(fileName = "ItemTableSO", menuName = "SO/LJS/ItemDataTableSO")]
    public class ItemDataTableSO : ScriptableObject
    {
        public List<ItemSOBase> ItemList;
        // [SerializeField] private RandomValueSO PercentageList;
        //
        // public void OnValidate()
        // {
        //     PercentageList.percentageList.SetLength(ItemList.Count);
        // }

        public ItemSOBase RandomItemOutput()
        {
           return ItemList.Count <= 0 ? null : ItemList[UnityEngine.Random.Range(0, ItemList.Count)];
        }
    }
}
