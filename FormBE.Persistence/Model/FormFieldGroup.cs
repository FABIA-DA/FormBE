namespace FormBE.Persistence.Model;

public class FormFieldGroup
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public int FieldGroupId { get; set; }
    public required Form Form { get; set; }
    public required FieldGroup FieldGroup { get; set; }
}
