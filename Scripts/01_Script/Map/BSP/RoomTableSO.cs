using System.Collections.Generic;
using UnityEngine;

namespace LJS.Rooms
{
    [CreateAssetMenu(fileName = "RoomTableSO", menuName = "So/LJS/RoomTableSO")]
    public class RoomTableSO : ScriptableObject
    {
        public List<PoolTypeSO> RoomList = new();
        
        private List<PoolTypeSO> _drawRoomList = new();
        private int count = 0;
        public PoolTypeSO RandomRoomDraw(bool notDuplication = false)
        {
            int rand = Random.Range(0, RoomList.Count);
            if (notDuplication && count < RoomList.Count)
            {
                while (_drawRoomList.Contains(RoomList[rand]))
                {
                    rand = Random.Range(0, RoomList.Count);
                }
                count++;
            }
            _drawRoomList.Add(RoomList[rand]);
            return RoomList[rand];
        }

        public void ResetSetting()
        {
            _drawRoomList.Clear();
        }
    }
}
