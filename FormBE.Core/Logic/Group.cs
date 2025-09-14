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
            Subgroups = self.SubGroups.Select(SubGroupToDto).ToList(),
            Forms = self.Forms.Select(f => f.ToFormDto(self.Name)).ToList()
        };

    public static GroupListDto ToListDto(this (long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount, int FormCount) self) =>
        new GroupListDto()
        {
            Id = self.Id,
            ParentId = self.ParentId,
            Name = self.Name,
            ParentName = self.ParentName,
            SubgroupCount = self.SubgroupCount,
            FormCount = self.FormCount,
        };

    private static SubgroupDto SubGroupToDto(this Group self) =>
        new SubgroupDto()
        {
            Id = self.Id,
            Name = self.Name
        };

    private static FormDto ToFormDto(this Form self, string groupName) =>
        new FormDto()
        {
            Id = self.Id,
            GroupId = null,
            Name = self.Name,
            GroupName = groupName,
            FieldGroups = self.FormFieldGroups.Select(ffg => ffg.FieldGroup.ToDto()).ToList()
        };
}

public sealed class GroupDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public required string Name { get; set; }
    public string? ParentName { get; set; }
    public required List<SubgroupDto> Subgroups { get; set; }
    public required List<FormDto> Forms { get; set; }
}

public sealed class SubgroupDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
}

public sealed class GroupListDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public required string Name { get; set; }
    public string? ParentName { get; set; }
    public int SubgroupCount { get; set; }
    public int FormCount { get; set; }
}