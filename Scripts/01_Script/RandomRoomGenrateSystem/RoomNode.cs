using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnterPoint
{
    public enum DIR
    {
        Up = 0, Down, Left, Right
    }
    public DIR dir;
    public Transform transform;
}

public class RoomNode : MonoBehaviour
{
    public List<EnterPoint> EnterPointList = new();
    public BoxCollider Width;
    public BoxCollider Height;
}
