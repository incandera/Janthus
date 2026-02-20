namespace Janthus.Model.Entities;

public class MapObject
{
    public int Id { get; set; }
    public int MapChunkId { get; set; }
    public int LocalX { get; set; }
    public int LocalY { get; set; }
    public int ObjectDefinitionId { get; set; }
}
