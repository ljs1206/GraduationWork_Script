using System.Collections.Generic;
using UnityEngine;

namespace BIS
{
    public class SpecialRoomSelector
    {
        private int[,] _integerMap;
        private int[,] _visited;
        private int[,] _spawned;
        private int _currentDepth;
        private int _currentRoomSpawnCount = 0;
        private int _specialRoomSpawnCount;
        private bool _spawn;
        private bool _increaseNow;
        private List<Vector2Int> _specialRoomList;
        private List<Vector2Int> _specialNomineeRoomList;
        
        public SpecialRoomSelector(int[,] map, int maxX, int maxY, int maxSpawnCount)
        {
            _integerMap = map;
            _visited = new int[maxX, maxY];
            _spawned = new int[maxX, maxY];
            _specialRoomSpawnCount = maxSpawnCount;
            _specialRoomList = new();
            _specialNomineeRoomList = new();
            FindNomineePoint();
        }

        public void FindNomineePoint()
        {
            _currentRoomSpawnCount = 0;
            _specialRoomList.Clear();
            _specialNomineeRoomList.Clear();
            BFS();
            if (_currentRoomSpawnCount < _specialRoomSpawnCount)
            {
                for (int i = 0; i < _specialNomineeRoomList.Count; i++)
                {
                    if (_currentRoomSpawnCount >= _specialRoomSpawnCount) break;
                    Vector2Int room = _specialNomineeRoomList[i];
                    if (_spawned[room.x, room.y] == 0)
                    {
                        _specialRoomList.Add(new Vector2Int(room.x, room.y));
                        _currentRoomSpawnCount++;
                        // Debug.Log($"SpecialRoom Pos : " + room.x + ", " + room.y);
                        // Debug.Log(_currentRoomSpawnCount);
                    }
                }
            }
        }

        private void BFS()
        {
            int cnt = 0;
            int[] dx =  { 0, 0, -1, 1 };
            int[] dy =  { 1, -1, 0, 0 };
            
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(4, 4));
            _visited[4, 4] = 1;
            
            while(queue.Count > 0)
            {
                _spawn = false;
                cnt++;
                Vector2Int current = queue.Dequeue();

                int currentX = current.x;
                int currentY = current.y;
                
                for (int i = 0; i < 4; i++)
                {
                    int nextX = currentX + dx[i];
                    int nextY = currentY + dy[i];

                    if (nextX >= 0 && nextX < _integerMap.Length && nextY >= 0 && nextY < _integerMap.Length)
                    {
                        if (_integerMap[nextX, nextY] == 1 && _visited[nextX, nextY] == 0)
                        {
                            if(!_spawn) _currentDepth++;

                            _increaseNow = true;
                            _spawn = true;
                            queue.Enqueue(new Vector2Int(nextX, nextY));
                            _visited[nextX, nextY] = 1;
                            if (_currentDepth > 3)
                            {
                                SelectSpecialRoom(nextX, nextY);
                            }
                        }
                    }
                }
                
                if (!_spawn)
                {
                    _currentDepth--;
                    if (_increaseNow)
                    {
                        _specialNomineeRoomList.Add(new Vector2Int(currentX, currentY));
                    }
                    _increaseNow = false;
                }
            }
        }

        private void SelectSpecialRoom(int nextX, int nextY)
        {
            if (_currentRoomSpawnCount >= _specialRoomSpawnCount) return;
                
            _currentRoomSpawnCount++;
            _specialRoomList.Add(new Vector2Int(nextX, nextY));
            _spawned[nextX, nextY] = 1;
            // Debug.Log($"SpecialRoom Pos : " + nextX + ", " + nextY);
        }
    }
}
