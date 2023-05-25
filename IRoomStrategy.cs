namespace bbsurvivor;
interface IRoomStrategy
{
    string Name { get; }
    bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom);
}

class NoobStrat : IRoomStrategy
{
    public string Name => "Take First Room";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom) => false;
}

class BailOnLower : IRoomStrategy
{
    public string Name => "Leave on lower floor";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom > nextRoom;
    }
}

class CoastTilExtremeOrBetter : IRoomStrategy
{
    public string Name => "Extreme is pretty good.";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom < RoomLevel.Extreme;
    }
}

class CoastTilUltimate : IRoomStrategy
{
    public string Name => "ULTIMATE BEANZ ONLY";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom < RoomLevel.Ultimate;
    }
}
