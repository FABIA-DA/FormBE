namespace FormBE.Persistence.Model;

public class Option
{
    public long Id { get; set; }
    public long SingleChoiceFieldId { get; set; }
    public required string Name { get; set; }
    public required SingleChoiceField SingleChoiceField { get; set; }
    public required List<OptionField> OptionFields { get; set; } = [];
    public required List<OptionResponse> OptionResponses { get; set; } = [];
}
