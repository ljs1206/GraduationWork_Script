using System;
using System.Collections.Generic;
using UnityEngine;

namespace Editors.SO
{
    [Serializable]
    public struct SOinfo
    {
        public ScriptableObject so;
        public string filePath;
    }
    
    [CreateAssetMenu(fileName = "SOTypeTable", menuName = "SO/LJS/Editor/SOTypeTable")]
    public class SOTypeTable : ScriptableObject
    {
        [field:SerializeField] public List<SOinfo> SOTypeList = new();
        [HideInInspector] public readonly List<Type> _typeList = new();

        public void OnValidate()
        {
            _typeList.Clear();
            foreach (var type in SOTypeList)
            {
                if (!_typeList.Contains(type.so.GetType()))
                {
                    _typeList.Add(type.so.GetType());
                }
            }
        }

        public string ReturnPath(Type type)
        {
            foreach (var item in SOTypeList)
            {
                if(item.so.GetType() == type) return item.filePath;
            }

            return "Unknown";
        }
    }
}
