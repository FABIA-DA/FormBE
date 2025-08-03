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

    public static List<long> GetIds(this IEnumerable<Group> coll) => coll.Select(i => i.Id).ToList();
    public static List<long> GetIds(this IEnumerable<Form> coll) => coll.Select(i => i.Id).ToList();
    public static List<long> GetIds(this IEnumerable<FieldGroup> coll) => coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<SingleChoiceField> coll) =>
        coll.Select(i => i.Id).ToList();

    public static List<long> GetIds(this IEnumerable<Field> coll) => coll.Select(i => i.Id).ToList();
}
