using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class GroupExtension
{
    public static GroupDto ToDto(this Group self) =>
        new()
        {
            Id = self.Id,
            ParentId = self.ParentId,
            Name = self.Name,
            ParentName = self.Parent?.Name,
            SubGroups = self.SubGroups.Select(g => SubGroupToDto(g, self.Id, self.Name)).ToList(),
            Forms = self.Forms.Select(f => f.ToFormDto(self.Id)).ToList()
        };

    private static GroupDto SubGroupToDto(this Group self, long parentId, string parentName) =>
        new GroupDto()
        {
            Id = self.Id,
            ParentId = parentId,
            Name = self.Name,
            ParentName = parentName,
            SubGroups = [],
            Forms = []
        };

    private static FormDto ToFormDto(this Form self, long groupId) =>
        new FormDto()
        {
            Id = self.Id,
            GroupId = groupId,
            Name = self.Name,
            Group = null,
            FieldGroups = self.FormFieldGroups.Select(ffg => ffg.FieldGroup.ToDto()).ToList()
        };
}

public sealed class GroupDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public required string Name { get; set; }
    public string? ParentName { get; set; }
    public required List<GroupDto> SubGroups { get; set; }
    public required List<FormDto> Forms { get; set; }
}
