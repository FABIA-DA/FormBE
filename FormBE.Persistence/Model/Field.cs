namespace FormBE.Persistence.Model;

public class Field
{
    public long Id { get; set; }
    public long FieldTypeId { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public bool IsOptional { get; set; }
    public required FieldType FieldType { get; set; }
    public required List<OptionField> OptionFields { get; set; } = [];
    public required List<FieldGroupField> FieldGroupFields { get; set; } = [];
    public required List<FieldResponse> FieldResponses { get; set; } = [];
}
