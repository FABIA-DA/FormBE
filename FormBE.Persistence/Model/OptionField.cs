namespace FormBE.Persistence.Model;

public class OptionField
{
    public int Id { get; set; }
    public int OptionId { get; set; }
    public int FieldId { get; set; }
    public required Option Option { get; set; }
    public required Field Field { get; set; }
}
