namespace FormBE.Persistence.Model;

public class Group
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public required string Name { get; set; }
    public Group? Parent { get; set; } = null;
    public required List<Group> SubGroups { get; set; }
    public required List<Form> Forms { get; set; }
}
