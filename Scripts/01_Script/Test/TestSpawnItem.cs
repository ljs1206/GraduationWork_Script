using UnityEngine;
using UnityEngine.InputSystem;

namespace LJS.Test
{
    public class TestSpawnItem : MonoBehaviour, IPoolable
    {
        public PoolTypeSO PoolType => _poolType;
        public GameObject GameObject => gameObject;
        
        [SerializeField] private PoolTypeSO _poolType;
        
        public void SetUpPool(Pool pool)
        {
            
        }

        public void ResetItem()
        {
            
        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                ManagementObject.Instance.SpawnManagent.DeleteEnemy(gameObject);
            }
        }
    }
}
