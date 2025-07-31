namespace FormBE.Persistence.Model;

public class FieldGroupField
{
    public int Id { get; set; }
    public int FieldGroupId { get; set; }
    public int FieldId { get; set; }
    public required FieldGroup FieldGroup { get; set; }
    public required Field Field { get; set; }
}
