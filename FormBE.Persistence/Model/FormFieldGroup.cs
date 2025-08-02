namespace FormBE.Persistence.Model;

public class FormFieldGroup
{
    public long Id { get; set; }
    public long FormId { get; set; }
    public long FieldGroupId { get; set; }
    public required Form Form { get; set; }
    public required FieldGroup FieldGroup { get; set; }
}
