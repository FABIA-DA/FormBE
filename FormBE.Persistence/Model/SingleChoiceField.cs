namespace FormBE.Persistence.Model;

public class SingleChoiceField
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required List<FieldGroupSingleChoiceField> FieldGroupSingleChoiceFields { get; set; }
    public required List<Option> Options { get; set; }
}
