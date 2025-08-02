namespace FormBE.Persistence.Model;

public class FieldGroupSingleChoiceField
{
    public long Id { get; set; }
    public long FieldGroupId { get; set; }
    public long SingleChoiceFieldId { get; set; }
    public required FieldGroup FieldGroup { get; set; }
    public required SingleChoiceField SingleChoiceField { get; set; }
}
