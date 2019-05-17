using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int RoomNumber { get; }
    public List<Vector2> RoomFields { get; }

    public Room(int number, List<Vector2> roomFields)
    {
        RoomNumber = number;
        RoomFields = roomFields;
    }
}
