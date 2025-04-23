using System;
using System.Collections.Generic;
using System.Threading;
using BIS;
using LJS.Map;
using UnityEngine;

public enum SpecialRoomType
{
    Shop = 0, Bouns, Healing, Boss, None
}

public struct MapInfo
{
    public int x;
    public int y;
}

public class RoomGenrator : MonoBehaviour
{
    [Header("Map Grid Setting")]
    [SerializeField] private int _gridX = 9;
    [SerializeField] private int _gridY = 9;
    [SerializeField] private int _roomPaddingX;
    [SerializeField] private int _roomPaddingZ;
    
    [Header("Room Spawn Setting")]
    [SerializeField] private int _roomSpawnCount = 0;
    [SerializeField] private float _pathWidth = 5f;
    [SerializeField] private int _maxTryCount;
    [SerializeField] private int _spcialRoomCount;

    [SerializeField] private GameObject _startRoom;
    [SerializeField] private RoomTableSO _roomTable;
    [SerializeField] private Material _pathMat;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _roofPrefab;

    private int[,] _map;
    private RoomNode[,] _roomArray;
    private int[] _dx = { 1, 0, -1, 0 };
    private int[] _dy = { 0, -1, 0, 1 };
    private bool _isSpawnNow;

    private int _currentX;
    private int _currentY;
    private int _tryCount;

    private Stack<MapInfo> _mapStack;
    private List<MapInfo> _failureList;
    private List<MapInfo> _spawnedRoomList;
    
    /// <summary>
    /// 8 * 8 이하로 맵의 최대 크기가 감소하지 않도록 Clamp하고
    /// -1 이하로 방을 생성할 갯수가 감소하지 않도록 Clamp 해줍니다.
    /// </summary>
    private void OnValidate()
    {
        _roomSpawnCount = Mathf.Clamp(_roomSpawnCount, 0, _gridX * _gridY);
        _gridX = Mathf.Clamp(_gridX, 8, _gridX + 1);
        _gridY = Mathf.Clamp(_gridY, 8, _gridY + 1);
    }
    
    private void Start()
    {
        _roomArray = new RoomNode[_gridX, _gridY];
    }
    
    /// <summary>
    /// 기본 설정 및 방 생성
    /// </summary>
    public void DefaultSetting()
    {
        DestroyAll();
        Debug.Log("RoomGenrator Default Setting");
        _tryCount++;
        _isSpawnNow = true;
        
        int rand = 0;
        int currentSpawnCount = 0;
        int failureCount = 0;
        int mainRoomSpawnCount = 1;
        MapInfo info = new MapInfo();
        
        _map = new int[_gridX, _gridY];
        _mapStack = new Stack<MapInfo>(_roomSpawnCount);
        _failureList = new(_roomSpawnCount);
        _spawnedRoomList = new(_roomSpawnCount);
        
        // 4,4 -> 시작 위치부터 생성할 위치로 정해줌
        _map[4, 4] = 1;
        _currentX = 4;
        _currentY = 4;
        info = new MapInfo { x = 4, y = 4 };
        _mapStack.Push(info);
        _spawnedRoomList.Add(info);
        
        // 그다음 4,4에서 랜덤 4방향 중 하나를 선택해서 생성할 위치로 정해줌
        rand = UnityEngine.Random.Range(0, 4);
        _currentX += _dx[rand];
        _currentY += _dy[rand];
        _map[_currentX, _currentY] = 1;
        info = new MapInfo{x = _currentX, y = _currentY};
        _mapStack.Push(info);
        _spawnedRoomList.Add(info);
        
        ++currentSpawnCount;
        int tryCount = 0;
        for (;;)
        {
            if(tryCount >= _maxTryCount) break;
            bool createSuccess = false;
            
            // 만약 이미 최대 생성 갯수만큼 생성했다면
            if(currentSpawnCount >= _roomSpawnCount)
            {
                Debug.Log(_tryCount);
                _tryCount = 0;
                CreateRoom();
                return;
            }
            
            if (currentSpawnCount / (_roomSpawnCount / 2) == mainRoomSpawnCount)
                // 4방향으로 생성 하기 위해서 주기적으로 확인함.
            {
                if (mainRoomSpawnCount == 4)
                {
                    mainRoomSpawnCount = 0;
                }
                while (_mapStack.Count > 1)
                {
                    _mapStack.Pop();
                }

                _currentX = _mapStack.Peek().x;
                _currentY = _mapStack.Peek().y;
                mainRoomSpawnCount++;
            }
            
            for (int i = 0; i < 4; ++i)
            {
                // 4가지 조건을 체크하며 생성 가능한지 확인함.
                // **1. 현재 원하는 만큼 방을 생성하였나?**
                // **2. 생성하고자 하는 위치에 이미 방이 생성되어 있나?**
                // **3. 생성하고자 하는 위치 주위에 이웃하는 방이 2개 이상 있나?**
                // **4. 50퍼센트 확률로 포기한다.
                if (CanGenerateRoom(_dx[i], _dy[i]) == true)
                {
                    createSuccess = true;
                    _currentX += _dx[i];
                    _currentY += _dy[i];
                    Debug.Log("Success : " + _currentX + " " + _currentY);
                    
                    _map[_currentX, _currentY] = 1;
                    info = new MapInfo { x = _currentX, y = _currentY };
                    _mapStack.Push(info);
                    _spawnedRoomList.Add(info);
                    currentSpawnCount++;
                    break;
                }
            }

            // 만드는 것에 실패했다면?
            // 4방향 전부 생성이 불가능한 상황
            if (!createSuccess)
            {
                // failureList에 넣어주고 Pop으로 Stack에 제외
                if (_mapStack.Count > 1)
                {
                    MapInfo currentMap = _mapStack.Pop();
                    _failureList.Add(currentMap);
                    failureCount++;
                    _currentX = _mapStack.Peek().x;
                    _currentY = _mapStack.Peek().y;
                }
                // 만약 Stack에 4,4만 남게 된다면 4,4로 초기화 해줌 
                else
                {
                    failureCount += (_roomSpawnCount / 4) - mainRoomSpawnCount;
                    mainRoomSpawnCount = 0;
                    _currentX = 4;
                    _currentY = 4;
                }
            }

            tryCount++;
        }
        
        // 아직도 원하는 갯수만큼 생성을 못했다면 failureList에 있는 후보들을 골라서 생성해줌
        int failureSpawnCount = 0;
        for (int j = 0; j < _failureList.Count; ++j)
        {
            if(failureSpawnCount > failureCount) break;
            
            _currentX =  _failureList[j].x;
            _currentY =  _failureList[j].y;
            
            for (int i = 0; i < 4; ++i)
            {
                if (CanGenerateRoom(_dx[i], _dy[i], false) == true)
                {
                    _currentX += _dx[i];
                    _currentY += _dy[i];
                    _map[_currentX, _currentY] = 1;
                    _spawnedRoomList.Add(new MapInfo{ x = _currentX, y = _currentY });
                    Debug.Log("Success : " + _currentX + " " + _currentY);
                    failureSpawnCount++;
                    currentSpawnCount++;
                    break;
                }
            }
        }
        
        // 만약 모든 경우의 수를 뚫고 생성을 전부 못했다면 다시 생성 시작함.
        if(currentSpawnCount < _roomSpawnCount) DefaultSetting();
        else
        {
            _tryCount = 0;
            Debug.Log(_tryCount);
            CreateRoom();
        }
    }

    /// <summary>
    /// 현재 생성된 Map를 전부 삭제 해주는 함수
    /// </summary>
    public void DestroyAll()
    {
        var roomCompoArray = transform.GetComponentsInChildren<RoomNode>();
        var pathCompoArray = transform.GetComponentsInChildren<RoomPath>();
        var roofCompoArray = transform.GetComponentsInChildren<RoomRoof>();
        
        for (int i = 0; i < roomCompoArray.Length; i++)
        {
            Debug.Log("DestoryRoom");
            DestroyImmediate(roomCompoArray[i].gameObject);
        }
        for (int i = 0; i < pathCompoArray.Length; i++)
        {
            Debug.Log("DestoryPath");
            DestroyImmediate(pathCompoArray[i].gameObject);
        }
        for (int i = 0; i < roofCompoArray.Length; i++)
        {
            Debug.Log("DestoryRoof");
            DestroyImmediate(roofCompoArray[i].gameObject);
        }
    }

    /// <summary>
    /// 4가지 조건을 체크하며 생성 가능한지 확인함.
    /// **1. 현재 원하는 만큼 방을 생성하였나?**
    /// **2. 생성하고자 하는 위치에 이미 방이 생성되어 있나?**
    /// **3. 생성하고자 하는 위치 주위에 이웃하는 방이 2개 이상 있나?**
    /// **4. 50퍼센트 확률로 포기한다.**
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="randomFailure"> 50퍼센트 확률로 실패 할지? </param>
    /// <returns></returns>
    public bool CanGenerateRoom(int x, int y, bool randomFailure = true)
    {
        if (_currentX + x >= _gridX || _currentX + x < 0 
            || _currentY + y >= _gridY || _currentY + y < 0)
            return false;
        int sumX = _currentX + x;
        int sumY = _currentY + y;
        
        #region TestCase_1
        if (_map[sumX, sumY] == 1)
            return false;
        #endregion
        
        #region TestCase_2
        int count = 0;
        for (int i = 0; i < 4; ++i)
        {
            int allX = sumX + _dx[i];
            int allY = sumY + _dy[i];
            if(allX >= _gridX || allX < 0) continue;
            if(allY >= _gridY || allY < 0) continue;
            
            if (_map[allX, allY] == 1)
            {
                count++;
            }
            
            if (count >= 2) return false;
        }
        #endregion
        
        #region TestCase_3
        if(_mapStack.Count > 1 && randomFailure)
            if (UnityEngine.Random.Range(0, 2) == 0) return false;
        #endregion
        
        return true;
    }

    /// <summary>
    /// 실제 방을 동적으로 생성해주는 함수 이 때 Padding에 따라서 방의 거리가 결정됨
    /// </summary>
    public void CreateRoom()
    {
        _roomArray = new RoomNode[_gridX, _gridY];
        GameObject obj = Instantiate(_startRoom, transform.position, Quaternion.identity);
        obj.name = "start Room";
        obj.transform.SetParent(transform);
        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        RoomNode room = obj.GetComponent<RoomNode>();
        obj.transform.position 
            = new Vector3(_roomPaddingX * 4, 0, _roomPaddingZ * 4);
        room.SetInfo(_wallPrefab, _pathMat, _roomArray, _gridX, _gridY, _pathWidth, _roofPrefab);
        _roomArray[4,4] = room;
        
        // Special Room Spawn
        // SpecialRoomSelector roomSelector = 
        //     new SpecialRoomSelector(_map, _gridX, _gridY, _spcialRoomCount);
        // SpecialRoom[] specialRoomArray = roomSelector.FindNomineePoint();
        // for (int i = 0; i < specialRoomArray.Length; ++i)
        // {
        //     SpecialRoom info = specialRoomArray[i];
        //     // todo : Change to Pool
        //     obj = Instantiate(_roomTable.RandomReturn().gameObject, transform.position, Quaternion.identity);
        //     obj.name = $"{specialRoomArray[i].type}";
        //     obj.transform.SetParent(transform);
        //     obj.transform.rotation = Quaternion.Euler(0, 0, 0);
        //             
        //     room = obj.GetComponent<RoomNode>();
        //     _roomArray[info.pos.x, info.pos.y] = room;
        //     obj.transform.position
        //         = new Vector3(_roomPaddingX * info.pos.x, 0, _roomPaddingZ * info.pos.y);
        //     Debug.Log($"{room.Width.size.x * 2}, {room.Height.size.x * 2}");
        //
        //     room.SetInfo(_wallPrefab, _pathMat, _roomArray, _gridX, _gridY, _pathWidth, _roofPrefab);
        // }
        
        // Spawn
        for (int i = 0; i < _gridX; ++i)
        {
            for (int j = 0; j < _gridY; ++j)
            {
                if(i == 4 && j == 4) continue;
                if (_map[i, j] == 1 && _roomArray[i, j] == null)
                {
                    // todo : Change to Pool
                    obj = Instantiate(_roomTable.RandomReturn().gameObject, transform.position, Quaternion.identity);
                    obj.name = $"{i}, {j}";
                    obj.transform.SetParent(transform);
                    obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                    
                    room = obj.GetComponent<RoomNode>();
                    _roomArray[i, j] = room;
                    obj.transform.position
                        = new Vector3(_roomPaddingX * i, 0, _roomPaddingZ * j);
                    Debug.Log($"{room.Width.size.x * 2}, {room.Height.size.x * 2}");
                    room.SetInfo(_wallPrefab, _pathMat, _roomArray, _gridX, _gridY, _pathWidth, _roofPrefab);
                }
            }
        }

        // 마지막으로 방을 끼리 이어줌
        Linking();
    }
    
    /// <summary>
    /// 방과 방을 연결하는 함수
    /// </summary>
    public void Linking()
    {
        for (int i = 0; i < _spawnedRoomList.Count; ++i)
        {
            MapInfo info = _spawnedRoomList[i];
            _roomArray[info.x, info.y].LinkRoom(info.x, info.y);
        }
    }
}
