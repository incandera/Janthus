namespace Janthus.Model.Entities;

public class MapChunk
{
    public int Id { get; set; }
    public int WorldMapId { get; set; }
    public int ChunkX { get; set; }
    public int ChunkY { get; set; }
    public byte[] GroundData { get; set; }
    public byte[] HeightData { get; set; }
}
