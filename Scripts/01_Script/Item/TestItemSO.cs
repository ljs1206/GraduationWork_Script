using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LJS.Item
{
[CreateAssetMenu(menuName = "SO/LJS/Item", fileName = "NewItem")]
    public class TestItemSO : ItemSOBase
    {
        public override void EndItemEffect()
        {
            base.EndItemEffect();
        }

        public override void UseItem()
        {
            Debug.Log("use item");
            base.UseItem();
        }
    }
}
