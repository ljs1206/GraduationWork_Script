using KHJ.SO;
using System.Collections.Generic;
using KHJ.SO;
using Sirenix.Utilities;
using UnityEngine;


namespace LJS.Item
{
    [CreateAssetMenu(fileName = "SynergyBlockTableSO", menuName = "SO/Synergy/SynergyBlockTableSO")]
    public class SynergyBlockTableSO : ScriptableObject
    {
        public List<SynergySO> synergyList = new();
        // [SerializeField] private RandomValueSO PercentageList;
        //
        // public void OnValidate()
        // {
        //     PercentageList.percentageList.SetLength(synergyList.Count);
        // }

        public SynergySO RandomSynergyOutput()
        {
            int randomGrade = UnityEngine.Random.Range(1, 3);

            switch (randomGrade)
            {
                case 1:
                    break;
                case 2:
                    break;
            }
            
            return synergyList.Count <= 0 ? null : synergyList[UnityEngine.Random.Range(0, synergyList.Count)];
        }
    }
}