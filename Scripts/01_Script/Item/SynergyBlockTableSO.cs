using KHJ.SO;
using System.Collections.Generic;
using KHJ.SO;
using UnityEngine;


namespace LJS.Item
{
    [CreateAssetMenu(fileName = "SynergyBlockTableSO", menuName = "SO/Synergy/SynergyBlockTableSO")]
    public class SynergyBlockTableSO : ScriptableObject
    {
        public List<SynergySO> synergyList = new();

        public SynergySO RandomSynergyOutput()
        {
            return synergyList.Count <= 0 ? null : synergyList[UnityEngine.Random.Range(0, synergyList.Count)];
        }
    }
}