namespace FormBE.Persistence.Model;

public class OptionField
{
    public long Id { get; set; }
    public long OptionId { get; set; }
    public long FieldId { get; set; }
    public required Option Option { get; set; }
    public required Field Field { get; set; }
}
