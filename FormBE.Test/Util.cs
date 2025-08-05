using FormBE.Persistence.Model;

namespace FormBE.Test;

public static class Util
{
    public static List<Group> GetTestGroups() =>
    [
        new()
        {
            Id = 0,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        },
        new()
        {
            Id = 1,
            Name = "Art",
            SubGroups = [],
            Forms = []
        },
        new()
        {
            Id = 2,
            Name = "Business",
            SubGroups = [],
            Forms = []
        },
        new()
        {
            Id = 3,
            Name = "Other",
            SubGroups = [],
            Forms = []
        }
    ];

    public static List<Form> GetTestForms() =>
    [
        new()
        {
            Id = 0,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        },
        new()
        {
            Id = 1,
            GroupId = null,
            Name = "Forest Work Form",
            Group = null,
            FormFieldGroups = []
        },
        new()
        {
            Id = 2,
            GroupId = null,
            Name = "Fishing Form",
            Group = null,
            FormFieldGroups = []
        },
        new()
        {
            Id = 3,
            GroupId = null,
            Name = "Business Form",
            Group = null,
            FormFieldGroups = []
        }
    ];

    public static List<FieldGroup> GetTestFieldGroups() =>
    [
        new()
        {
            Id = 0,
            Name = "Contact",
            FormFieldGroups = [],
            FieldGroupSingleChoiceFields = [],
            FieldGroupFields = []
        },
        new()
        {
            Id = 1,
            Name = "Details",
            FormFieldGroups = [],
            FieldGroupSingleChoiceFields = [],
            FieldGroupFields = []
        },
        new()
        {
            Id = 2,
            Name = "Person",
            FormFieldGroups = [],
            FieldGroupSingleChoiceFields = [],
            FieldGroupFields = []
        },
        new()
        {
            Id = 3,
            Name = "Work-Related",
            FormFieldGroups = [],
            FieldGroupSingleChoiceFields = [],
            FieldGroupFields = []
        }
    ];

    public static List<SingleChoiceField> GetTestSingleChoiceFields() =>
    [
        new()
        {
            Id = 0,
            Name = "Gender",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        },
        new()
        {
            Id = 1,
            Name = "Organization Status",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        },
        new()
        {
            Id = 2,
            Name = "Personal Status",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        },
        new()
        {
            Id = 3,
            Name = "Business Status",
            Options = [],
            FieldGroupSingleChoiceFields = [],
        }
    ];

    public static List<Field> GetTestFields() => [
        new()
        {
            Id = 0,
            FieldTypeId = 0,
            Name = "Postal Code",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0,
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
            Id = 1,
            FieldTypeId = 0,
            Name = "Street Number",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0,
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
            Id = 2,
            FieldTypeId = 0,
            Name = "How old are you",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0,
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
            Id = 3,
            FieldTypeId = 0,
            Name = "Height",
            Description = null,
            IsOptional = false,
            FieldResponses = [],
            FieldType = new()
            {
                Id = 0,
                Name = "Number",
                Description = null,
                Regex = "",
                Fields = []
            },
            FieldGroupFields = [],
            OptionFields = []
        }
    ];

    public static List<(long Id, string Name, List<long> FieldIds)> GetTestOptions() => [
        (0, "Option 1", [0, 1]),
        (1, "Option 2", [0]),
        (2, "Option 3", []),
        (3, "Option 4", [2, 4])
    ];

    public static List<OptionResponse> GetTestOptionResponses() => [
        new()
        {
            Id = 0,
            TelephoneNumber = "0000",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Option = new()
            {
                Id = 0,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0,
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
            Id = 1,
            TelephoneNumber = "0001",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 1, 0),
            Option = new()
            {
                Id = 0,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0,
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
            Id = 2,
            TelephoneNumber = "0002",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 2, 0),
            Option = new()
            {
                Id = 0,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0,
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
            Id = 3,
            TelephoneNumber = "0003",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 3, 0),
            Option = new()
            {
                Id = 0,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        },
    ];

    public static List<FieldType> GetTestFieldTypes() => [
        new()
        {
            Id = 0,
            Name = "Number",
            Description = null,
            Regex = "\\d+",
            Fields = []
        },
        new()
        {
            Id = 1,
            Name = "String",
            Description = null,
            Regex = "\\w+",
            Fields = []
        },
        new()
        {
            Id = 2,
            Name = "Postal Code",
            Description = null,
            Regex = "\\d{4}",
            Fields = []
        },
        new()
        {
            Id = 3,
            Name = "EAN-13",
            Description = null,
            Regex = "\\d{13}",
            Fields = []
        }
    ];

    public static List<FieldResponse> GetTestFieldResponses() => [
        new()
        {
            Id = 0,
            TelephoneNumber = "0000",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0,
                Name = "Input",
                Description =null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
        new()
        {
            Id = 1,
            TelephoneNumber = "0001",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0,
                Name = "Input",
                Description =null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
        new()
        {
            Id = 2,
            TelephoneNumber = "0002",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0,
                Name = "Input",
                Description =null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        },
        new()
        {
            Id = 3,
            TelephoneNumber = "0003",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0,
                Name = "Input",
                Description =null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0,
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
    public static List<long> GetIds(this IEnumerable<FieldGroup> coll) => coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<SingleChoiceField> coll) => coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<Field> coll) => coll.Select(i => i.Id).ToList();
}
