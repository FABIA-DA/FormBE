namespace FormBE.Persistence.Model;

public class FieldGroupField
{
    public long Id { get; set; }
    public long FieldGroupId { get; set; }
    public long FieldId { get; set; }
    public required FieldGroup FieldGroup { get; set; }
    public required Field Field { get; set; }
}
