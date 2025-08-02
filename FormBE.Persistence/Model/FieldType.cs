namespace FormBE.Persistence.Model;

public class FieldType
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; } = null;
    public required string Regex { get; set; }
    public required List<Field> Fields { get; set; }
}
