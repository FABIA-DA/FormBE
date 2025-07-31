namespace FormBE.Persistence.Model;

public class FieldGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required List<FormFieldGroup> FormFieldGroups { get; set; }
    public required List<FieldGroupSingleChoiceField> FieldGroupSingleChoiceFields { get; set; }
    public required List<FieldGroupField> FieldGroupFields { get; set; }
}
