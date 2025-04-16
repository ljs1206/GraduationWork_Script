using System.Collections.Generic;
using LJS.Map;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomTableSO", menuName = "SO/RoomTableSO")]
public class RoomTableSO : ScriptableObject
{
    public List<RoomNode> RoomList = new();

    public RoomNode RandomReturn()
    {
        return RoomList[Random.Range(0, RoomList.Count)];
    }
}
