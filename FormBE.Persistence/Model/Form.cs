namespace FormBE.Persistence.Model;

public class Form
{
    public int Id { get; set; }
    public int? GroupId { get; set; }
    public required string? Name { get; set; }
    public Group? Group { get; set; }
    public required List<FormFieldGroup> FormFieldGroups { get; set; }
}
