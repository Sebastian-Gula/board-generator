public class Room
{
    public int RoomNumber { get; set; }
    public int Size { get; set; }

    public Room(int roomNumber, int size)
    {
        RoomNumber = roomNumber;
        Size = size;
    }
}
