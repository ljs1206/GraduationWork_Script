using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RandomValue
{
    public int percentage;
    public string name;
}

[CreateAssetMenu(fileName = "RandomValueSO", menuName = "SO/LJS/Item/RandomValueSO")]
public class RandomValueSO : ScriptableObject
{
    public List<RandomValue> percentageList = new();

    public string ReturnValue(int value)
    {
        int sum = 0;
        for (int i = 0; i < percentageList.Count; i++)
        {
            if (i == 0)
            {
                if (0 <= value && value <= percentageList[i].percentage)
                {
                    return percentageList[i].name;
                }
                else
                    sum += percentageList[i].percentage;
            }
            else
            {
                if (sum <= value && value <= percentageList[i].percentage)
                {
                    return percentageList[i].name;
                }
                else
                    sum += percentageList[i].percentage;
            }
        }

        return "";
    }
}
