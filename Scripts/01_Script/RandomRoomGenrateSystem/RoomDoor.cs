using System;
using UnityEngine;

namespace LJS.Map
{
    public class RoomDoor : MonoBehaviour
    {
        private Animator _animatorCompo;
        [SerializeField] private string _boolParameterName = ""; 

        private void Awake()
        {
            _animatorCompo = GetComponent<Animator>();
        }
        
        public void ChangeDoorAnimation(bool state)
        { 
            _animatorCompo = GetComponent<Animator>();
            _animatorCompo.SetBool(_boolParameterName, state);
        } 
        public void OpenAnimation() => _animatorCompo.SetBool(_boolParameterName, true);
        public void CloseAnimation() => _animatorCompo.SetBool(_boolParameterName, false);
    }
}
