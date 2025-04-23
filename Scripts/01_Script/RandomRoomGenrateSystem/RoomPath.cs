using System;
using UnityEngine;

namespace LJS.Map
{
    public class RoomPath : MonoBehaviour
    {
        private MeshRenderer _wallBound;
        private GameObject _wallPrefab;
        
        private MeshRenderer _renderCompo;
        private Vector3 _startTopWallPoint;
        private Vector3 _startBottomWallPoint;

        /// <summary>
        /// 기본 정보를 설정 함수.
        /// </summary>
        /// <param name="startTopWallPoint"> 벽을 처음 설치할 위치 : Top</param>
        /// <param name="startBottomWallPoint"> 벽을 처음 설치할 위치 : Bottom</param>
        /// <param name="wallPrefab"></param>
        /// <param name="dir"> 방향 </param>
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
        
        /// <summary>
        /// 벽 동적 생성
        /// 생성하는 EnterPoint의 방향에 따라서 회전을 해줌
        /// 만약 벽이 짤린다면 벽의 Scale를 줄여준다.
        /// </summary>
        /// <param name="dir"></param>
        private void CreateWall(EnterPoint.DIR dir)
        {
            float pathRenderSize = _renderCompo.bounds.size.x > _renderCompo.bounds.size.z
                ? _renderCompo.bounds.size.x : _renderCompo.bounds.size.z;
            int wallCount = Mathf.CeilToInt(pathRenderSize / _wallBound.bounds.size.x);
            float wallOverSize =  (_wallBound.bounds.size.x * wallCount) - pathRenderSize;
            // 벽 생성
            // Debug.Log($"Wall Spawn Start, RenderSize : {pathRenderSize}, " +
            //           $"Wall Size : {_wallBound.bounds.size.x}, Wall Count {wallCount}");
            
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
                    // Debug.Log(wallOverSize);
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

        /// <summary>
        /// 천장을 생성해준다.
        /// 벽과 비슷한 방식으로 생성하되 Scale를 늘리는 방식을 채택했다.
        /// </summary>
        /// <param name="pos"> 위치 </param>
        /// <param name="roofPrefab"></param>
        /// <param name="dir"> 방향 </param>
        /// <param name="roofSize"></param>
        public void CreateRoof(Vector3 pos, GameObject roofPrefab, EnterPoint.DIR dir, Vector3 roofSize)
        {
            float pathRenderSize = _renderCompo.bounds.size.x > _renderCompo.bounds.size.z
                ? _renderCompo.bounds.size.x : _renderCompo.bounds.size.z;
            GameObject roof =  Instantiate(roofPrefab, pos, Quaternion.identity);
            roof.transform.SetParent(transform.root);
            if (dir == EnterPoint.DIR.Down || dir == EnterPoint.DIR.Up)
            {
                roof.transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            Vector3 scale = roof.transform.localScale;
            if(dir == EnterPoint.DIR.Right || dir == EnterPoint.DIR.Down)
                scale.x = -(pathRenderSize / roofSize.x);
            else
                scale.x = pathRenderSize / roofSize.x;
            roof.transform.localScale = scale;
        }
    }
}

