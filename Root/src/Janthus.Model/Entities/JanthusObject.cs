namespace Janthus.Model.Entities;

public class JanthusObject
{
    public int Id { get; set; }
    public Guid InternalId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
