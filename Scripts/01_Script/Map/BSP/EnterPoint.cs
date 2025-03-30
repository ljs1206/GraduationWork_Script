using UnityEngine;

namespace LJS.Rooms
{
    public class EnterPoint : MonoBehaviour
    {
        public Room LinkedRoom;
        public EnterPoint LinkedPoint;
        public Dir _pointDir;

        public bool isActivePoint = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag("Player")){
                Debug.Log("Enter");
            }
        }
    }
}
