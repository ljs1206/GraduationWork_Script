using System;
using System.Collections.Generic;
using UnityEngine;
namespace LJS.Map
{
[Serializable]
public struct EnterPoint
{
    public enum DIR
    {
        Up = 0, Down, Left, Right, None
    }
    public DIR dir;
    public Transform transform;
}

public class RoomNode : MonoBehaviour
{
    public List<EnterPoint> EnterPointList = new();
    public BoxCollider Width;
    public BoxCollider Height;

    [SerializeField] private LayerMask _pathLayer;
    [SerializeField] private Material _miniMapMaterial;

    private GameObject _wallPrefab;
    private GameObject _roofPrefab;
    private Material _pathMat;
    private RoomNode[,] _roomArray;
    private int _gridX;
    private int _gridY;
    private float _pathWidth;
    private List<EnterPoints> _enterPointsList;
    
    private int[] _dx = { 0, 0, -1, 1 };
    private int[] _dy = { 1, -1, 0, 0 };
    public bool CreateWall { get; private set; }

    /// <summary>
    /// 기본 정보 설정 하는 함수
    /// </summary>
    /// <param name="wallPrefab"> Path의 벽</param>
    /// <param name="pathMat">Path의 Material</param>
    /// <param name="roomArray"> 생성된 방 이차원 배열(x,y)</param>
    /// <param name="gridX"> 최대 크키 : x</param>
    /// <param name="gridY"> 최대 크키 : y</param>
    /// <param name="pathWidth"> Path의 넓이</param>
    /// <param name="roofPrefab"> 천장 Prefab</param>
    public void SetInfo(GameObject wallPrefab, Material pathMat
        , RoomNode[,] roomArray, int gridX, int gridY, float pathWidth, GameObject roofPrefab)
    {
        _wallPrefab = wallPrefab;
        _pathMat = pathMat;
        _roomArray = roomArray;
        _gridX = gridX;
        _gridY = gridY;
        _pathWidth = pathWidth;
        _roofPrefab = roofPrefab;
        CreateWall = false;
    }
    
    /// <summary>
    /// 실제 방과 방을 연결해주는 함수
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void LinkRoom(int x, int y)
    {
        // Debug.Log("Link Start");
        for (int i = 0; i < 4; ++i)
        {
            // Debug.Log(_gridX);
            // Debug.Log(_gridY);
            if (y + _dy[i] < 0 || y + _dy[i] >= _gridY) continue;
            if (x + _dx[i] < 0 || x + _dx[i] >= _gridX) continue;
            if (_roomArray[x + _dx[i], y + _dy[i]] != null)
            {
                // 미리 만들어 둔 EnterPoint 중 어디 방향인지 찾음.
                // 그리고 맞보는 방향과 이어줌
                EnterPoint.DIR dir = FindDir(_dx[i], _dy[i]);
                EnterPoint.DIR reverseDir = EnterPoint.DIR.None;

                switch (dir)
                {
                    case EnterPoint.DIR.Up:
                        reverseDir = EnterPoint.DIR.Down;
                        break;
                    case EnterPoint.DIR.Down:
                        reverseDir = EnterPoint.DIR.Up;
                        break;
                    case EnterPoint.DIR.Left:
                        reverseDir = EnterPoint.DIR.Right;
                        break;
                    case EnterPoint.DIR.Right:
                        reverseDir = EnterPoint.DIR.Left;
                        break;
                }

                Transform startTrm = EnterPointList.Find(a => a.dir == reverseDir).transform;
                Transform endTrm = _roomArray[x + _dx[i], y + _dy[i]]
                    .EnterPointList.Find(a => a.dir == dir).transform;
                
                startTrm.GetComponent<EnterPoints>().ChangeStatePoint(true);
                endTrm.GetComponent<EnterPoints>().ChangeStatePoint(true);
                
                CreatePath(startTrm, endTrm,
                    reverseDir, _roomArray[x + _dx[i], y + _dy[i]]);
            }
        }

        _roomArray[x, y].CreateWall = true;
    }

    /// <summary>
    /// 방향 찾기용 함수
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private EnterPoint.DIR FindDir(int x, int y)
    {
        for (int i = 0; i < 4; ++i)
        {
            if (x == 0)
            {
                if (y == -1)
                    return EnterPoint.DIR.Down;
                else
                    return EnterPoint.DIR.Up;
            }
            else if(y == 0)
            {
                if (x == -1)
                    return EnterPoint.DIR.Left;
                else
                    return EnterPoint.DIR.Right;
            }
        }
        return EnterPoint.DIR.None;
    }

    /// <summary>
    /// Path 동적 생성 함수
    /// Mesh를 방과 방의 EnterPoint의 위치에 따라서 start, end로 이어줌
    /// 그리고 Vertice, Triangle, uv를 고려하여 생성해서 배치함
    /// Start를 기준으로 End가 제대로 맞물리지 않는다면 End에 속하는 Room를 맞물리게 이동함
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="dir"></param>
    /// <param name="targetRoom"></param>
    private void CreatePath(Transform start, Transform end, EnterPoint.DIR dir, RoomNode targetRoom)
    {
        if (targetRoom.CreateWall) return;
        targetRoom.CreateWall = true;
        
        Mesh pathMesh = new Mesh();
        Vector3 topLeftV = Vector3.zero,
            topRightV = Vector3.zero,
            bottomLeftV = Vector3.zero,
            bottomRightV = Vector3.zero;
        
        if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
        {
            float moveZ = 0;
            if (start.position.z > end.position.z)
            {
                moveZ = start.transform.position.z - end.position.z;
                targetRoom.transform.position = new Vector3(targetRoom.transform.position.x, 
                    targetRoom.transform.position.y, targetRoom.transform.position.z + moveZ);
            }
            else if (end.position.z > start.position.z)
            {
                moveZ = end.transform.position.z - start.position.z;
                targetRoom.transform.position = new Vector3(targetRoom.transform.position.x, 
                    targetRoom.transform.position.y, targetRoom.transform.position.z - moveZ);
            }
        }
        else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
        {
            float moveX = 0;
            if (start.position.x > end.position.x)
            {
                moveX = start.transform.position.x - end.position.x;
                targetRoom.transform.position = new Vector3(targetRoom.transform.position.x + moveX, 
                    targetRoom.transform.position.y, targetRoom.transform.position.z);
            }
            else if (end.position.x > start.position.x)
            {
                moveX = end.transform.position.x - start.position.x;
                targetRoom.transform.position = new Vector3(targetRoom.transform.position.x - moveX, 
                    targetRoom.transform.position.y, targetRoom.transform.position.z);
            }
        }
        
        switch (dir)
        {
            case EnterPoint.DIR.Left:
            {
                topLeftV = new Vector3(end.position.x, start.position.y, start.position.z - _pathWidth / 2);
                topRightV = new Vector3(start.position.x, start.position.y, start.position.z - _pathWidth / 2);
                bottomLeftV = new Vector3(end.position.x, start.position.y, start.position.z + _pathWidth / 2);
                bottomRightV = new Vector3(start.position.x, start.position.y, start.position.z + _pathWidth / 2);
            }
                break;
            case EnterPoint.DIR.Right:
            {
                topLeftV = new Vector3(start.position.x, start.position.y, start.position.z - _pathWidth / 2);
                topRightV = new Vector3(end.position.x, start.position.y, start.position.z - _pathWidth / 2);
                bottomLeftV = new Vector3(start.position.x, start.position.y, start.position.z + _pathWidth / 2);
                bottomRightV = new Vector3(end.position.x, start.position.y, start.position.z + _pathWidth / 2);
            }
                break;
            case EnterPoint.DIR.Up:
            {
                topLeftV = new Vector3(end.position.x + _pathWidth / 2, start.position.y, end.position.z);
                topRightV = new Vector3(end.position.x - _pathWidth / 2, start.position.y, end.position.z);
                bottomLeftV = new Vector3(start.position.x + _pathWidth / 2, start.position.y, start.position.z);
                bottomRightV = new Vector3(start.position.x - _pathWidth / 2, start.position.y, start.position.z);
            }
                break;
            case EnterPoint.DIR.Down:
            {
                topLeftV = new Vector3(start.position.x + _pathWidth / 2, start.position.y, start.position.z);
                topRightV = new Vector3(start.position.x - _pathWidth / 2, start.position.y, start.position.z);
                bottomLeftV = new Vector3(end.position.x + _pathWidth / 2, start.position.y, end.position.z);
                bottomRightV = new Vector3(end.position.x - _pathWidth / 2, start.position.y, end.position.z);
            }
                break;
        }

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };
        
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        
        pathMesh.vertices = vertices;
        pathMesh.uv = uvs;
        pathMesh.triangles = triangles;
        
        GameObject floor = new GameObject("Floor", 
            typeof(MeshFilter), typeof(MeshRenderer), typeof(RoomPath));
        floor.GetComponent<MeshFilter>().mesh = pathMesh;
        floor.GetComponent<MeshRenderer>().material = _pathMat;
        
        Vector3 roofPos =  start.position;
        Vector3 roofSize = _roofPrefab.transform.Find("Visual").GetComponent<MeshRenderer>().bounds.size;
        if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
        {
            floor.GetComponent<RoomPath>().
                SetInfo(topLeftV, bottomLeftV, _wallPrefab, dir);
            roofPos = new Vector3(roofPos.x, roofPos.y, roofPos.z + roofSize.x);
            float moveZ = 0;
            if (start.position.z > end.position.z)
            {
                moveZ = start.transform.position.z - end.position.z;
            }
            else if (end.position.z > start.position.z)
            {
                moveZ = end.transform.position.z - start.position.z;
            }
            
            targetRoom.transform.position = new Vector3(targetRoom.transform.position.x, 
                targetRoom.transform.position.y, targetRoom.transform.position.z + moveZ);
        }
        else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
        {
            floor.GetComponent<RoomPath>().
                SetInfo(topLeftV, topRightV, _wallPrefab, dir);
            roofPos = new Vector3(roofPos.x + roofSize.x, roofPos.y, roofPos.z);
            
            float moveX = 0;
            if (start.position.x > end.position.x)
            {
                moveX = start.transform.position.x - end.position.x;
            }
            else if (end.position.x > start.position.x)
            {
                moveX = end.transform.position.x - start.position.x;
            }
            
            targetRoom.transform.position = new Vector3(targetRoom.transform.position.x + moveX, 
                targetRoom.transform.position.y, targetRoom.transform.position.z);
        }
        
        floor.transform.SetParent(transform.root);
        floor.transform.localPosition = Vector3.zero;
        floor.transform.rotation = Quaternion.identity;
        roofPos.y = -2.35f;
        // Debug.Log($"roof pos : {roofPos}, dir : {dir}");
        floor.AddComponent<MeshCollider>();
        floor.GetComponent<RoomPath>().CreateRoof(roofPos, _roofPrefab , dir, roofSize);

        GameObject miniMapFloor = Instantiate(floor, transform.root, true);
        miniMapFloor.transform.localPosition = Vector3.zero;
        miniMapFloor.transform.rotation = Quaternion.identity;
        
        miniMapFloor.GetComponent<MeshRenderer>().material = _miniMapMaterial;
        miniMapFloor.layer = 17;
    }
    
    /// <summary>
    /// 방이 전투 상태에 돌입했다면
    /// </summary>
    /// <param name="state"></param>
    public void ChangeBattleState(bool state)
    {
        if (_enterPointsList == null)
        {
            _enterPointsList = new(EnterPointList.Count);
            foreach (var point in EnterPointList)
            {
                EnterPoints compo = point.transform.GetComponent<EnterPoints>();
                _enterPointsList.Add(compo);
                compo.ChangeStatePoint(state);
            }

            return;
        }

        foreach (var point in _enterPointsList)
        {
            point.ChangeStatePoint(state);
        }
    }
}

}
