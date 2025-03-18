using System.Collections.Generic;
using LJS.Item;
using UnityEngine;

namespace LJS
{
    [CreateAssetMenu(fileName = "ItemTableSO", menuName = "SO/LJS/ItemTableSO")]
    public class ItemTableSO : ScriptableObject
    {
        public List<GameObject> itemList;
        
        public GameObject RandomItemOutput()
        {
            return itemList.Count <= 0 ? null : itemList[UnityEngine.Random.Range(0, itemList.Count)];
        }
    }
}
