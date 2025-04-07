using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum SpecialRoomType
{
    Shop = 0, Bouns, Healing, Boss 
}

[Serializable]
public struct SpawnRoomInfo
{
    public int shopSpawnCount;
    public int bounsSpawnCount;
    public int healingSpawnCount;
    public int bossSpawnCount;
}

public struct MapInfo
{
    public int x;
    public int y;
}

public class RoomGenrator : MonoBehaviour
{
    [SerializeField] private int _gridX = 9;
    [SerializeField] private int _gridY = 9;
    
    [SerializeField] 
    private int _roomSpawnCount = 0;
    [SerializeField] private float _pathWidth = 5f;
    // [SerializeField] private SpawnRoomInfo _spawnInfo;

    [SerializeField] private GameObject _startRoom;
    [SerializeField] private RoomTableSO _roomTable;
    [SerializeField] private Material _pathMat;
    [SerializeField] private int _maxTryCount;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _roofPrefab;

    private int[,] _map;
    private RoomNode[,] _roomArray;
    private int[] _dx = { 1, 0, -1, 0 };
    private int[] _dy = { 0, -1, 0, 1 };
    private bool _isSpawnNow;

    private int currentX = 0;
    private int currentY = 0;
    private int _tryCount = 0;

    private Stack<MapInfo> _mapStack;
    private List<MapInfo> _failureList;
    private MeshRenderer _renderCompo;

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
        Debug.Log("RoomGenrator Default Setting");
        _tryCount++;
        _isSpawnNow = true;
        int rand = 0;
        int currentSpawnCount = 0;
        int failureCount = 0;
        int mainRoomSpawnCount = 1;
        
        _map = new int[_gridX, _gridY];
        _mapStack = new Stack<MapInfo>(_roomSpawnCount);
        _failureList = new(_roomSpawnCount);
        
        _map[4, 4] = 1;
        currentX = 4;
        currentY = 4;
        _mapStack.Push(new MapInfo{ x = 4, y = 4 });
        rand = UnityEngine.Random.Range(0, 4);
        currentX += _dx[rand];
        currentY += _dy[rand];
        _map[currentX, currentY] = 1;
        _mapStack.Push(new MapInfo{x = currentX, y = currentY});

        Debug.Log("Start : " + currentX + " " + currentY);
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
                Debug.Log("Reset");
                currentX = _mapStack.Peek().x;
                currentY = _mapStack.Peek().y;
                mainRoomSpawnCount++;
            }
            
            for (int i = 0; i < 4; ++i)
            {

                if (CanGenerateRoom(_dx[i], _dy[i]) == true)
                {
                    createSuccess = true;
                    currentX += _dx[i];
                    currentY += _dy[i];
                    Debug.Log("Success : " + currentX + " " + currentY);
                    _map[currentX, currentY] = 1;
                    _mapStack.Push(new MapInfo{x = currentX, y = currentY});
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
                    Debug.Log("Pop : " + currentMap.x + " " + currentMap.y);
                    currentX = _mapStack.Peek().x;
                    currentY = _mapStack.Peek().y;
                }
                else
                {
                    failureCount += (_roomSpawnCount / 4) - mainRoomSpawnCount;
                    mainRoomSpawnCount = 0;
                    currentX = 4;
                    currentY = 4;
                }
            }

            tryCount++;
        }
        
        int failureSpawnCount = 0;
        for (int j = 0; j < _failureList.Count; ++j)
        {
            if(failureSpawnCount > failureCount) break;
            
            currentX =  _failureList[j].x;
            currentY =  _failureList[j].y;
            
            for (int i = 0; i < 4; ++i)
            {
                if (CanGenerateRoom(_dx[i], _dy[i], false) == true)
                {
                    currentX += _dx[i];
                    currentY += _dy[i];
                    _map[currentX, currentY] = 1;
                    Debug.Log("Success : " + currentX + " " + currentY);
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
    }

    public bool CanGenerateRoom(int x, int y, bool randomFailure = true)
    {
        if (currentX + x >= _gridX || currentX + x < 0 
            || currentY + y >= _gridY || currentY + y < 0)
            return false;
        int sumX = currentX + x;
        int sumY = currentY + y;
        
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
        Debug.Log("CreateRoom");
        _roomArray = new RoomNode[_gridX, _gridY];
        GameObject obj = Instantiate(_startRoom, transform.position, Quaternion.identity);
        RoomNode room = obj.GetComponent<RoomNode>();
        obj.transform.SetParent(transform);
        obj.transform.localPosition 
            = new Vector3(40 * 4, 0, 40 * 4);
        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
        _roomArray[4,4] = room;
        obj.transform.Find("Tile").GetComponent<TextMeshPro>().text = "4, 4";
        
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
                    room = obj.GetComponent<RoomNode>();
                    obj.transform.SetParent(transform);
                    obj.transform.localPosition 
                        = new Vector3(40 * i, 0, 40 * j);
                    obj.transform.rotation = Quaternion.Euler(0, 0, 0);
                    obj.transform.Find("Tile").GetComponent<TextMeshPro>().text = $"{i}, {j}";
                    room.SetInfo(_wallPrefab, _pathMat, _roomArray, _gridX, _gridY, _pathWidth, _roofPrefab);
                    _roomArray[i, j] = room;
                }
            }
        }
        
        // Link
        for (int i = 0; i < _gridX; ++i)
        {
            for (int j = 0; j < _gridY; ++j)
            {
                if(i == 4 && j == 4) continue;
                if (_roomArray[i, j] != null)
                {
                    _roomArray[i, j].LinkRoom(i, j);
                }
            }
        }
    }
}
