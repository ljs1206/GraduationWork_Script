using System;
using UnityEngine;

namespace BIS
{
    public class EnterPoints : MonoBehaviour
    {
        [SerializeField] private GameObject _gate;

        public void ChangeStatePoint(bool state)
        {
            if(_gate == null) _gate = transform.GetComponentInChildren<MeshRenderer>().gameObject;
            _gate.SetActive(!state);
        }
    }
}
