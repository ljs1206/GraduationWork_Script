using System.Collections.Generic;
using UnityEngine;

namespace LJS.Item
{
    [CreateAssetMenu(fileName = "ItemTableSO", menuName = "SO/LJS/ItemDataTableSO")]
    public class ItemDataTableSO : ScriptableObject
    {
        public List<ItemSOBase> ItemList;

        public ItemSOBase RandomItemOutput()
        {
           return ItemList.Count <= 0 ? null : ItemList[UnityEngine.Random.Range(0, ItemList.Count)];
        }
    }
}
