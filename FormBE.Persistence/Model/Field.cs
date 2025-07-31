namespace FormBE.Persistence.Model;

public class Field
{
    public int Id { get; set; }
    public int FieldTypeId { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; } = null;
    public bool IsOptional { get; set; }
    public required FieldType FieldType { get; set; }
    public required List<OptionField> OptionFields { get; set; }
    public required List<FieldGroupField> FieldGroupFields { get; set; }
    public required List<FieldResponse> FieldResponses { get; set; }
}
