namespace Janthus.Model.Entities;

public class WorldMap
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Seed { get; set; }
    public int ChunkSize { get; set; } = 32;
    public int ChunkCountX { get; set; }
    public int ChunkCountY { get; set; }
}
