using RayFire;
using UnityEngine;

namespace LJS.Map
{
    public class BreakableObject : MonoBehaviour
    {
        [SerializeField] private ItemTableSO _itemTableSO;
        
        private RayfireRigidRoot _rigid;
        private bool _breakNow;

        private void Awake()
        {
            _rigid = GetComponent<RayfireRigidRoot>();

            _rigid.activationEvent.LocalEventRoot += (x, y) =>
            {
                _breakNow = true;
            };
        }

        private void Update()
        {
            if (_breakNow)
            {
                Instantiate(_itemTableSO.itemList[UnityEngine.Random.Range(0, _itemTableSO.itemList.Count)], transform.position + new Vector3(0, 3, 0), Quaternion.identity);
                _breakNow = false;
            }
        }
    }
}
