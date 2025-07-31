namespace FormBE.Persistence.Model;

public class FieldGroupSingleChoiceField
{
    public int Id { get; set; }
    public int FieldGroupId { get; set; }
    public int SingleChoiceFieldId { get; set; }
    public required FieldGroup FieldGroup { get; set; }
    public required SingleChoiceField SingleChoiceField { get; set; }
}
