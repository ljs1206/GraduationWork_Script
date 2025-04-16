using System;
using System.Collections.Generic;
using System.Threading;
using BIS;
using Cysharp.Threading.Tasks;
using LJS.Map;
using UnityEngine;
using VInspector.Libs;

public enum SpecialRoomType
{
    Shop = 0, Bouns, Healing, Boss 
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
    private MeshRenderer _renderCompo;
    private CancellationTokenSource _destroyCancellation;

    private void OnValidate()
    {
        _roomSpawnCount = Mathf.Clamp(_roomSpawnCount, 0, _gridX * _gridY);
    }
    
    private void Start()
    {
        _roomArray = new RoomNode[_gridX, _gridY];
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_isSpawnNow) return;
            DefaultSetting();
        }
        #endif
    }
    public void DefaultSetting()
    {
        DestroyAll();
        _destroyCancellation = new CancellationTokenSource();
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
        
        _map[4, 4] = 1;
        _currentX = 4;
        _currentY = 4;
        info = new MapInfo { x = 4, y = 4 };
        _mapStack.Push(info);
        _spawnedRoomList.Add(info);
        
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
            if(currentSpawnCount >= _roomSpawnCount)
            {
                Debug.Log(_tryCount);
                _tryCount = 0;
                CreateRoom();
                return;
            }
            
            if (currentSpawnCount / (_roomSpawnCount / 2) == mainRoomSpawnCount)
                // 4방향으로 생성 하기 위해서 
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

            if (!createSuccess)
            {
                if (_mapStack.Count > 1)
                {
                    MapInfo currentMap = _mapStack.Pop();
                    _failureList.Add(currentMap);
                    failureCount++;
                    _currentX = _mapStack.Peek().x;
                    _currentY = _mapStack.Peek().y;
                }
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
        
        if(currentSpawnCount < _roomSpawnCount) DefaultSetting();
        else
        {
            _tryCount = 0;
            Debug.Log(_tryCount);
            CreateRoom();
        }
    }

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

    public void CreateRoom()
    {
        _roomArray = new RoomNode[_gridX, _gridY];
        GameObject obj = Instantiate(_startRoom, transform.position, Quaternion.identity);
        obj.name = "start Room";
        obj.transform.SetParent(transform);
        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        RoomNode room = obj.GetComponent<RoomNode>();
        obj.transform.position 
            = new Vector3(40 * 4, 0, 40 * 4);
        room.SetInfo(_wallPrefab, _pathMat, _roomArray, _gridX, _gridY, _pathWidth, _roofPrefab);
        _roomArray[4,4] = room;
        
        // Spawn
        for (int i = 0; i < _gridX; ++i)
        {
            for (int j = 0; j < _gridY; ++j)
            {
                if(i == 4 && j == 4) continue;
                if (_map[i, j] == 1)
                {
                    // todo : Change to Pool
                    obj = Instantiate(_roomTable.RandomReturn().gameObject, transform.position, Quaternion.identity);
                    obj.name = $"{i}, {j}";
                    obj.transform.SetParent(transform);
                    obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                    
                    room = obj.GetComponent<RoomNode>();
                    _roomArray[i, j] = room;
                    obj.transform.position
                        = new Vector3(40 * i, 0, 40 * j);
                    Debug.Log($"{room.Width.size.x * 2}, {room.Height.size.x * 2}");
                    // Debug.Log($"{40 }, {room.Height.size.x * j}");
                    room.SetInfo(_wallPrefab, _pathMat, _roomArray, _gridX, _gridY, _pathWidth, _roofPrefab);
                }
            }
        }

        SpecialRoomSelector roomSelector = 
            new SpecialRoomSelector(_map, _gridX, _gridY, _spcialRoomCount);
        roomSelector.FindNomineePoint();

        Linking();
    }
    
    public async UniTask Linking()
    {
        for (int i = 0; i < _spawnedRoomList.Count; ++i)
        {
            MapInfo info = _spawnedRoomList[i];
            await UniTask.WaitUntil(_roomArray[info.x, info.y].LinkRoom(info.x, info.y)
            ,PlayerLoopTiming.Update, _destroyCancellation.Token, true);
        }
        
        // _roomArray[4, 4].LinkRoom(4, 4);
        // //Link
        // for (int i = 0; i < _gridX; ++i)
        // {
        //     for (int j = 0; j < _gridY; ++j)
        //     {
        //         if(i == 4 && j == 4) continue;
        //         if (_roomArray[i, j] != null)
        //         {
        //             _testCnt++;
        //              await UniTask.WaitUntil(_roomArray[i, j].LinkRoom(i, j));
        //         }
        //     }
        // }
    }

    public void OnDestroy()
    {
        _destroyCancellation.Cancel();
        _destroyCancellation.Dispose();
    }
}
