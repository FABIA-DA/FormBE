namespace FormBE.Persistence.Model;

public class Group
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public required string Name { get; set; }
    public Group? Parent { get; set; }
    public required List<Group> SubGroups { get; set; }
    public required List<Form> Forms { get; set; }
}
