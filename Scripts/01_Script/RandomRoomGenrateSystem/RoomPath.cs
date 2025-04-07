using System;
using UnityEngine;
using VInspector.Libs;

public class RoomPath : MonoBehaviour
{
    private MeshRenderer _wallBound;
    private GameObject _wallPrefab;
    
    private MeshRenderer _renderCompo;
    private Vector3 _startTopWallPoint;
    private Vector3 _startBottomWallPoint;

    public void SetInfo(Vector3 startTopWallPoint, Vector3 startBottomWallPoint,
         GameObject wallPrefab, EnterPoint.DIR dir)
    {
        _startTopWallPoint = startTopWallPoint;
        _startBottomWallPoint = startBottomWallPoint;
        _renderCompo = GetComponent<MeshRenderer>();

        _wallPrefab = wallPrefab;
        Transform visualTrm = wallPrefab.transform.Find("Visual");
        _wallBound = visualTrm.GetComponent<MeshRenderer>();
        CreateWall(dir);
    }

    private void CreateWall(EnterPoint.DIR dir)
    {
        float pathRenderSize = _renderCompo.bounds.size.x > _renderCompo.bounds.size.z
            ? _renderCompo.bounds.size.x : _renderCompo.bounds.size.z;
        int wallCount = Mathf.CeilToInt(pathRenderSize / _wallBound.bounds.size.x);
        float wallOverSize =  (_wallBound.bounds.size.x * wallCount) - pathRenderSize;
        // 벽 생성
        Debug.Log($"Wall Spawn Start, RenderSize : {pathRenderSize}, " +
                  $"Wall Size : {_wallBound.bounds.size.x}, Wall Count {wallCount}");
        
        int i = 0;
        for (i = 0; i < wallCount; i++)
        {
            float x = 0, z = 0;
            GameObject wall = null;
            if (i == wallCount - 1)
            {
                // todo : Mesh나 Scale 줄여서 짤리는 부분 없애기
                wall = Instantiate(_wallPrefab, transform.root);
                if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
                {
                    x = _startTopWallPoint.x - _wallBound.bounds.size.x / 2 - i * 3;
                    wall.transform.position = new Vector3(x, _startTopWallPoint.y, _startTopWallPoint.z);
                }
                else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
                {
                    z = (_startTopWallPoint.z -
                        _wallBound.bounds.size.x + (i + 1) * 3) + _wallBound.bounds.size.x / 2;
                    wall.transform.position = new Vector3(_startTopWallPoint.x, _startTopWallPoint.y, z);
                    wall.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                Transform visualTrm = wall.transform.Find("Visual");
                Debug.Log(wallOverSize);
                float scaleX = visualTrm.transform.localScale.x - wallOverSize / _wallBound.bounds.size.x;
                visualTrm.localScale = new Vector3(scaleX, 1, 1);
                break;
            }
            
            wall = Instantiate(_wallPrefab, transform.root);
            x = _startTopWallPoint.x - _wallBound.bounds.size.x / 2 - i * 3;
            if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
            {
                x = _startTopWallPoint.x - _wallBound.bounds.size.x / 2 - i * 3;
                wall.transform.position = new Vector3(x, _startTopWallPoint.y, _startTopWallPoint.z);
            }
            else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
            {
                z = (_startTopWallPoint.z - 
                    _wallBound.bounds.size.x + (i + 1) * 3) + _wallBound.bounds.size.x / 2;
                wall.transform.position = new Vector3(_startTopWallPoint.x, _startTopWallPoint.y, z);
                wall.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
        
        for (i = 0; i < wallCount; i++)
        {
            float x = 0, z = 0;
            GameObject wall = null;
            if (i == wallCount - 1)
            {
                // todo : Mesh나 Scale 줄여서 짤리는 부분 없애기
                wall = Instantiate(_wallPrefab, transform.root);
                if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
                {
                    x = _startBottomWallPoint.x - _wallBound.bounds.size.x / 2 - i * 3;
                    wall.transform.position = new Vector3(x, _startBottomWallPoint.y, _startBottomWallPoint.z);
                }
                else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
                {
                    z = (_startBottomWallPoint.z -
                        _wallBound.bounds.size.x + (i + 1) * 3) + _wallBound.bounds.size.x / 2;
                    wall.transform.position = new Vector3(_startBottomWallPoint.x, _startBottomWallPoint.y, z);
                    wall.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                Transform visualTrm = wall.transform.Find("Visual");
                Debug.Log(wallOverSize);
                float scaleX = visualTrm.transform.localScale.x - wallOverSize / _wallBound.bounds.size.x;
                visualTrm.localScale = new Vector3(scaleX, 1, 1);
                break;
            }
            
            wall = Instantiate(_wallPrefab, transform.root);
            x = _startBottomWallPoint.x - _wallBound.bounds.size.x / 2 - i * 3;
            if (dir == EnterPoint.DIR.Left || dir == EnterPoint.DIR.Right)
            {
                x = _startBottomWallPoint.x - _wallBound.bounds.size.x / 2 - i * 3;
                wall.transform.position = new Vector3(x, _startBottomWallPoint.y, _startBottomWallPoint.z);
            }
            else if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
            {
                z = (_startBottomWallPoint.z - 
                    _wallBound.bounds.size.x + (i + 1) * 3) + _wallBound.bounds.size.x / 2;
                wall.transform.position = new Vector3(_startBottomWallPoint.x, _startBottomWallPoint.y, z);
                wall.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
    }

    public void CreateRoof(Vector3 pos, GameObject roofPrefab, EnterPoint.DIR dir)
    {
        GameObject roof =  Instantiate(roofPrefab, pos, Quaternion.identity);
        roof.transform.SetParent(transform);
        if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
        {
            roof.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }
}
