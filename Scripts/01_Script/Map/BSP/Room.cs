using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LJS.Rooms
{
        
    
    public class Room : MonoBehaviour, IPoolable
    {
        // private Dictionary<string, int> dict = new();
        [SerializeField] private BoxCollider WidthCol;
        [SerializeField] private BoxCollider HeightCol;
        
        // [SerializeField] private PoolTypeSO _enterPointType;
        // [SerializeField] private PoolManagerSO _poolManager;
        // [SerializeField] private Transform _spawnTrm;

        public List<EnterPoint> EnterPointList = new();
        public List<EnterPoint> LinkedPointList {get; private set; } = new();

        private List<EnterPoint> _spawnPointList = new();
        public int MaxEnterPoint;

        public int xSize { get; private set; }
        public int ySize { get; private set; }

        #region Pooling
        [SerializeField] private PoolTypeSO _poolType;
        public PoolTypeSO PoolType => _poolType;
        public GameObject GameObject => gameObject;
        
        public void SetUpPool(Pool pool)
        {
            // 부모를 제외하고 List에 Add
            // List<EnterPoint> list = transform.GetComponentsInChildren<EnterPoint>().ToList();
            // for (int i = 1; i < list.Count; ++i)
            // {
            //     EnterPointList.Add(list[i]);
            // }

            xSize = (int)WidthCol.size.x;
            ySize = (int)HeightCol.size.z;
        }

        public void ResetItem()
        {

        }
        #endregion

        public EnterPoint GetShorterPoints(Transform targetTrm){
            EnterPoint _shorter = null;
            float distance = 0;
            for(int i = 0; i < EnterPointList.Count; ++i){
                if(_shorter == null)
                {
                    _shorter = EnterPointList[i];
                    distance = Vector3.Distance(targetTrm.position, EnterPointList[0].transform.position);
                }
                else if(Vector3.Distance(targetTrm.position, EnterPointList[i].transform.position) < 
                distance)
                {
                    _shorter = EnterPointList[i];
                    distance = Vector3.Distance(targetTrm.position, EnterPointList[i].transform.position);
                }
            }
            _shorter.isActivePoint = true;
            EnterPointList.Remove(_shorter);
            return _shorter;
        }

    //    private int spawnCount = 0;
    //    public EnterPoint GenerateEnterPoint(){
    //         IPoolable poolable = _poolManager.Pop(_enterPointType);
    //         poolable.GameObject.transform.SetParent(_spawnTrm);
    //         poolable.GameObject.transform.position = EnterPointList[spawnCount].position;

    //         spawnCount++;
    //         _spawnPointList.Add(poolable.GameObject.GetComponent<EnterPoint>());
    //         return poolable.GameObject.GetComponent<EnterPoint>();
    //    }

        // private void Awake()
        // {
        //     // 부모를 제외하고 List에 Add
        //     List<EnterPoint> list = transform.GetComponentsInChildren<EnterPoint>().ToList();
        //     for (int i = 0; i < list.Count; ++i)
        //     {
        //         EnterPointList.Add(list[i]);
        //     }

        //     xSize = (int)WidthCol.size.x;
        //     ySize = (int)WidthCol.size.y;
        // }
    }
}
