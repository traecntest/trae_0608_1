namespace BioCAD.Domain.Entities;

public abstract class EntityBase
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsDeleted { get; set; } = false;
}
