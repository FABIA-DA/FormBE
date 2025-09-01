namespace FormBE.Persistence.Model;

public class Form
{
    public long Id { get; set; }
    public long? GroupId { get; set; }
    public required string Name { get; set; }
    public Group? Group { get; set; }
    public required List<FormFieldGroup> FormFieldGroups { get; set; } = [];
}
