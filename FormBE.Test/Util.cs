using FormBE.Persistence.Model;

namespace FormBE.Test;

public static class Util
{
    public static List<Group> GetTestGroups() =>
    [
        new()
        {
            Id = 0L,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        },
        new()
        {
            Id = 1L,
            Name = "Art",
            SubGroups = [],
            Forms = []
        },
        new()
        {
            Id = 2L,
            Name = "Business",
            SubGroups = [],
            Forms = []
        },
        new()
        {
            Id = 3L,
            Name = "Other",
            SubGroups = [],
            Forms = []
        }
    ];

    public static List<Form> GetTestForms() =>
    [
        new()
        {
            Id = 0L,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        },
        new()
        {
            Id = 1L,
            GroupId = null,
            Name = "Forest Work Form",
            Group = null,
            FormFieldGroups = []
        },
        new()
        {
            Id = 2L,
            GroupId = null,
            Name = "Fishing Form",
            Group = null,
            FormFieldGroups = []
        },
        new()
        {
            Id = 3L,
            GroupId = null,
            Name = "Business Form",
            Group = null,
            FormFieldGroups = []
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

    public static List<SingleChoiceField> GetTestSingleChoiceFields() =>
    [
        new()
        {
            Id = 0L,
            Name = "Gender",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        },
        new()
        {
            Id = 1L,
            Name = "Organization Status",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        },
        new()
        {
            Id = 2L,
            Name = "Personal Status",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        },
        new()
        {
            Id = 3L,
            Name = "Business Status",
            Options = [],
            FieldGroupSingleChoiceFields = [],
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

    public static List<long> GetIds(this IEnumerable<Group> coll) => coll.Select(i => i.Id).ToList();
    public static List<long> GetIds(this IEnumerable<Form> coll) => coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(
        this IEnumerable<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> coll) =>
        coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<SingleChoiceField> coll) => coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<Field> coll) => coll.Select(i => i.Id).ToList();
}
