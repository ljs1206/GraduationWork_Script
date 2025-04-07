using System;
using System.Collections.Generic;
using UnityEngine;

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

    private GameObject _wallPrefab;
    private GameObject _roofPrefab;
    private Material _pathMat;
    private RoomNode[,] _roomArray;
    private int _gridX;
    private int _gridY;
    private float _pathWidth;
    
    private int[] _dx = { 1, 0, -1, 0 };
    private int[] _dy = { 0, -1, 0, 1 };
    public bool CreateWall { get; private set; }

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
    
    public void LinkRoom(int x, int y)
    {
        for (int i = 0; i < 4; ++i)
        {
            if (y + _dy[i] < 0 || y + _dy[i] >= _gridY) break;
            if (x + _dx[i] < 0 || x + _dx[i] >= _gridX) break;
            if (_roomArray[x + _dx[i], y + _dy[i]] != null)
            {
                EnterPoint.DIR dir = FindDir(_dx[i], _dy[i]);
                CreatePath(EnterPointList.Find(a => a.dir == EnterPoint.DIR.Up).transform,
                    _roomArray[x + _dx[i], y + _dy[i]].EnterPointList.Find(a => a.dir == EnterPoint.DIR.Down).transform,
                    (EnterPoint.DIR)i, _roomArray[x + _dx[i], y + _dy[i]]);
            }
        }

        CreateWall = true;
    }

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
            else
            {
                if (x == -1)
                    return EnterPoint.DIR.Left;
                else
                    return EnterPoint.DIR.Right;
            }
        }
        return EnterPoint.DIR.None;
    }

    private void CreatePath(Transform start, Transform end, EnterPoint.DIR dir, RoomNode targetRoom)
    {
        if (targetRoom.CreateWall) return;
        Mesh pathMesh = new Mesh();
        Vector3 topLeftV = Vector3.zero,
            topRightV = Vector3.zero,
            bottomLeftV = Vector3.zero,
            bottomRightV = Vector3.zero;
        
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
        if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
        {
            floor.GetComponent<RoomPath>().
                SetInfo(topLeftV, bottomLeftV, _wallPrefab, dir);
        }
        else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
        {
            floor.GetComponent<RoomPath>().
                SetInfo(topLeftV, topRightV, _wallPrefab, dir);
        }
        
        floor.transform.SetParent(transform.root);
        floor.transform.localPosition = Vector3.zero;
        floor.transform.rotation = Quaternion.identity;
        Vector3 middle = new Vector3((start.position.x + end.position.x) / 2, 2.65f,
            end.position.z);
        floor.GetComponent<RoomPath>().CreateRoof(middle, _roofPrefab , dir);
    }
}
