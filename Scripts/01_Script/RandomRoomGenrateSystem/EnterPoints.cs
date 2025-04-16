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
            if(!_doorCompo) _doorCompo = _gate.GetComponent<RoomDoor>();
            Debug.Log(_doorCompo);
            _doorCompo?.ChangeDoorAnimation(state);
        }
    }
}
