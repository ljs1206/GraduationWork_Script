using System;
using UnityEngine;

namespace LJS.Map
{
    public class EnterPoints : MonoBehaviour
    {
        [SerializeField] private GameObject _gate;
        private RoomDoor _doorCompo;
        
        public void ChangeStatePoint(bool state)
        {
            if (!_doorCompo) _doorCompo = _gate.GetComponent<RoomDoor>();
            _doorCompo?.ChangeDoorAnimation(state);
        }
    }
}
