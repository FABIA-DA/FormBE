using FormBE.Persistence.Model;

namespace FormBE.Test;

public static class Util
{
    public static List<(long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount, int FormCount)>
        GetTestGroups() =>
    [
        new()
        {
            Id = 0L,
            Name = "Forms",
            ParentId = null,
            ParentName = null,
            SubgroupCount = 0,
            FormCount = 0
        },
        new()
        {
            Id = 1L,
            Name = "Art",
            ParentId = null,
            ParentName = null,
            SubgroupCount = 0,
            FormCount = 0
        },
        new()
        {
            Id = 2L,
            Name = "Business",
            ParentId = null,
            ParentName = null,
            SubgroupCount = 0,
            FormCount = 0
        },
        new()
        {
            Id = 3L,
            Name = "Other",
            ParentId = null,
            ParentName = null,
            SubgroupCount = 0,
            FormCount = 0
        }
    ];

    public static List<(long Id, string Name, long? GroupId, string? GroupName, int FieldGroupCount)> GetTestForms() =>
    [
        new()
        {
            Id = 0L,
            GroupId = null,
            Name = "House Building Form",
            GroupName = null,
            FieldGroupCount = 0,
        },
        new()
        {
            Id = 1L,
            GroupId = null,
            Name = "Forest Work Form",
            GroupName = null,
            FieldGroupCount = 0,
        },
        new()
        {
            Id = 2L,
            GroupId = null,
            Name = "Fishing Form",
            GroupName = null,
            FieldGroupCount = 0,
        },
        new()
        {
            Id = 3L,
            GroupId = null,
            Name = "Business Form",
            GroupName = null,
            FieldGroupCount = 0,
        }
    ];

    public static List<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> GetTestFieldGroups() =>
    [
        new()
        {
            Id = 0L,
            Name = "Contact",
            SingleChoiceFieldCount = 0,
            FieldCount = 0
        },
        new()
        {
            Id = 1L,
            Name = "Details",
            SingleChoiceFieldCount = 0,
            FieldCount = 0
        },
        new()
        {
            Id = 2L,
            Name = "Person",
            SingleChoiceFieldCount = 0,
            FieldCount = 0
        },
        new()
        {
            Id = 3L,
            Name = "Work-Related",
            SingleChoiceFieldCount = 0,
            FieldCount = 0
        }
    ];

    public static List<(long Id, string Name, int OptionCount)> GetTestSingleChoiceFields() =>
    [
        new()
        {
            Id = 0L,
            Name = "Gender",
            OptionCount = 0
        },
        new()
        {
            Id = 1L,
            Name = "Organization Status",
            OptionCount = 0
        },
        new()
        {
            Id = 2L,
            Name = "Personal Status",
            OptionCount = 0
        },
        new()
        {
            Id = 3L,
            Name = "Business Status",
            OptionCount = 0
        }
    ];

    public static List<Field> GetTestFields() =>
    [
        new()
        {
            Id = 0L,
            FieldTypeId = 0L,
            Name = "Postal Code",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0L,
                Name = "Number",
                Description = null,
                Regex = "",
                Fields = []
            },
            FieldGroupFields = [],
            OptionFields = []
        },
        new()
        {
            Id = 1L,
            FieldTypeId = 0L,
            Name = "Street Number",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0L,
                Name = "Number",
                Description = null,
                Regex = "",
                Fields = []
            },
            FieldGroupFields = [],
            OptionFields = []
        },
        new()
        {
            Id = 2L,
            FieldTypeId = 0L,
            Name = "How old are you",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0L,
                Name = "Number",
                Description = null,
                Regex = "",
                Fields = []
            },
            FieldGroupFields = [],
            OptionFields = []
        },
        new()
        {
            Id = 3L,
            FieldTypeId = 0L,
            Name = "Height",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0L,
                Name = "Number",
                Description = null,
                Regex = "",
                Fields = []
            },
            FieldGroupFields = [],
            OptionFields = []
        }
    ];

    public static List<(long Id, string Name, List<long> FieldIds)> GetTestOptions() =>
    [
        (0L, "Option 1", [0L, 1L]),
        (1L, "Option 2", [0L]),
        (2L, "Option 3", []),
        (3L, "Option 4", [2L, 4L])
    ];

    public static List<OptionResponse> GetTestOptionResponses() =>
    [
        new()
        {
            Id = 0L,
            TelephoneNumber = "0000",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Option = new()
            {
                Id = 0L,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0L,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        },
        new()
        {
            Id = 1L,
            TelephoneNumber = "0001",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 1, 0),
            Option = new()
            {
                Id = 0L,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0L,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        },
        new()
        {
            Id = 2L,
            TelephoneNumber = "0002",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 2, 0),
            Option = new()
            {
                Id = 0L,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0L,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        },
        new()
        {
            Id = 3L,
            TelephoneNumber = "0003",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 3, 0),
            Option = new()
            {
                Id = 0L,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0L,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        },
    ];

    public static List<FieldType> GetTestFieldTypes() =>
    [
        new()
        {
            Id = 0L,
            Name = "Number",
            Description = null,
            Regex = "\\d+",
            Fields = []
        },
        new()
        {
            Id = 1L,
            Name = "String",
            Description = null,
            Regex = "\\w+",
            Fields = []
        },
        new()
        {
            Id = 2L,
            Name = "Postal Code",
            Description = null,
            Regex = "\\d{4}",
            Fields = []
        },
        new()
        {
            Id = 3L,
            Name = "EAN-13",
            Description = null,
            Regex = "\\d{13}",
            Fields = []
        }
    ];

    public static List<FieldResponse> GetTestFieldResponses() =>
    [
        new()
        {
            Id = 0L,
            TelephoneNumber = "0000",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0L,
                Name = "Input",
                Description = null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0L,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
        new()
        {
            Id = 1L,
            TelephoneNumber = "0001",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0L,
                Name = "Input",
                Description = null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0L,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
        new()
        {
            Id = 2L,
            TelephoneNumber = "0002",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0L,
                Name = "Input",
                Description = null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0L,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
        new()
        {
            Id = 3L,
            TelephoneNumber = "0003",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0L,
                Name = "Input",
                Description = null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0L,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
    ];

    public static List<long> GetIds(
        this IEnumerable<(long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount, int FormCount)>
            coll) =>
        coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(
        this IEnumerable<(long Id, string Name, long? GroupId, string? GroupName, int FieldGroupCount)> coll) =>
        coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(
        this IEnumerable<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> coll) =>
        coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<(long Id, string Name, int OptionCount)> coll) =>
        coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<Field> coll) => coll.Select(i => i.Id).ToList();

    public static List<Group> GetGroups(
        this IEnumerable<(long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount, int FormCount)>
            coll) =>
        coll.Select(g => new Group()
        {
            Id = g.Id,
            Name = g.Name,
            ParentId = g.ParentId,
            Parent = null,
            SubGroups = [],
            Forms = []
        }).ToList();
    
    public static List<Form> GetForms(
        this IEnumerable<(long Id, string Name, long? GroupId, string? GroupName, int FieldGroupCount)> coll) =>
        coll.Select(f => new Form()
        {
            Id = f.Id,
            Name = f.Name,
            GroupId = f.GroupId,
            Group = null,
            FormFieldGroups = []
        }).ToList();

    public static List<FieldGroup> GetFieldGroups(
        this IEnumerable<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> coll) =>
        coll.Select(fg => new FieldGroup()
        {
            Id = fg.Id,
            Name = fg.Name,
            FormFieldGroups = [],
            FieldGroupSingleChoiceFields = [],
            FieldGroupFields = []
        }).ToList();

    public static List<SingleChoiceField> GetSingleChoiceFields(
        this IEnumerable<(long Id, string Name, int OptionCount)> coll) =>
        coll.Select(scf => new SingleChoiceField()
        {
            Id = scf.Id,
            Name = scf.Name,
            FieldGroupSingleChoiceFields = [],
            Options = []
        }).ToList();
}
