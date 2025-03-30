using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LJS.Rooms
{
    public enum Dir{
        Left, Right, Up, Down
    }

    public class BSPSystem : MonoBehaviour
    {
        [SerializeField] private PoolManagerSO _poolManager = null;
        
        [Header("BSP Setting")]
        [SerializeField] private Room _startRoom;
        [SerializeField] private Room _endRoom;
        [SerializeField] private RoomTableSO _roomTable;
        [SerializeField] private int _spawnRoomCount;
        [SerializeField] private bool _duplicationSetting;

        private void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartBSP();
            }
        }

        private int spawnCount = 0;
        private bool spawnEnd = false;
        private Room currentRoom;
        public void StartBSP()
        {
            List<Room> _spawnedRoomList = new();
            
            EnterPoint startPoint;
            startPoint = _startRoom.EnterPointList[0].GetComponent<EnterPoint>();

            currentRoom = SpawnRoom(startPoint._pointDir, _startRoom);

            startPoint.LinkedRoom = currentRoom;
            startPoint.isActivePoint = true;

            EnterPoint point;
            point = currentRoom.GetShorterPoints(startPoint.transform);
            point.LinkedRoom = _startRoom;

            LinkPoint(startPoint, point);
            currentRoom.LinkedPointList.Add(point);
            _startRoom.LinkedPointList.Add(startPoint);

            while(currentRoom.EnterPointList.Count > 0){
                if(_spawnRoomCount <= spawnCount) return;

                EnterPoint currentPoint = currentRoom.EnterPointList[0];
                Room spawnedRoom = SpawnRoom(currentPoint._pointDir, currentRoom);
                currentPoint.isActivePoint = true;
                currentPoint.LinkedRoom = spawnedRoom;
                
                point = spawnedRoom.GetShorterPoints(currentPoint.transform);
                point.LinkedRoom = currentRoom;

                LinkPoint(currentPoint, point);
                
                currentRoom.LinkedPointList.Add(currentPoint);
                spawnedRoom.LinkedPointList.Add(point);

                currentRoom.EnterPointList.Remove(currentPoint);

                _spawnedRoomList.Add(spawnedRoom);
            }

            for(;;){
                for(int i = 0; i < _spawnedRoomList.Count; ++i){
                    if(_spawnedRoomList.Count == 0) continue;

                    while(_spawnedRoomList[i].EnterPointList.Count > 0){
                    {
                        if(_spawnRoomCount <= spawnCount){
                            _endRoom.EnterPointList[0].LinkedRoom = _spawnedRoomList[i];
                            LinkPoint(_endRoom.EnterPointList[0],
                                _spawnedRoomList[i].GetShorterPoints(_endRoom.EnterPointList[0].transform));
                            return;
                        }

                        EnterPoint currentPoint = _spawnedRoomList[i].EnterPointList[0];
                        Room spawnedRoom = SpawnRoom(currentPoint._pointDir, _spawnedRoomList[i]);
                        currentPoint.isActivePoint = true;
                        currentPoint.LinkedRoom = spawnedRoom;

                        spawnedRoom.EnterPointList[0].LinkedRoom = _spawnedRoomList[i];

                        LinkPoint(currentPoint,
                            spawnedRoom.GetShorterPoints(spawnedRoom.EnterPointList[0].transform));
                        
                        _spawnedRoomList[i].EnterPointList.Remove(currentPoint);

                        _spawnedRoomList.Add(spawnedRoom);
                    }

                    if(_spawnRoomCount <= spawnCount){
                        return;
                    }
                }
            }
        }
        }

        private IPoolable _poolable = null;
        public Room SpawnRoom(Dir dir, Room _room)
        {
            if (spawnEnd) return null;
            _poolable = _poolManager.Pop(_roomTable.RandomRoomDraw(_duplicationSetting));
            Room room = _poolable.GameObject.GetComponent<Room>();

            switch(dir){
                case Dir.Left:
                {
                    _poolable.GameObject.transform.position = 
                _room.transform.position + new Vector3(-room.xSize * 2, 0);
                }
                break;
                case Dir.Right:
                {
                    _poolable.GameObject.transform.localPosition = 
                _room.transform.position + new Vector3(room.xSize * 2, 0);
                }
                break;
                case Dir.Up:
                {
                    _poolable.GameObject.transform.position = 
                _room.transform.position + new Vector3(0, 0, room.ySize * 2);
                }
                break;
                case Dir.Down:
                {
                    _poolable.GameObject.transform.position = 
                _room.transform.position + new Vector3(0, 0, -room.ySize * 2);
                }
                break;
            }
            
            ++spawnCount;

            if(_spawnRoomCount <= spawnCount) spawnEnd = true;
            return room;
        }

        private void LinkPoint(EnterPoint point_1, EnterPoint point_2){
            point_1.LinkedPoint = point_2;
            point_2.LinkedPoint = point_1;
        }
    }
}
