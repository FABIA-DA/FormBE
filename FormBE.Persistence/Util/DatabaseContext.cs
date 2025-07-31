using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace FormBE.Persistence.Util;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public const string SchemaName = "FormBE";
    
    public DbSet<Group> Groups { get; set; }
    public DbSet<Form> Forms { get; set; }
    public DbSet<FieldGroup> FieldGroups { get; set; }
    public DbSet<SingleChoiceField> SingleChoiceFields { get; set; }
    public DbSet<OptionResponse> OptionResponses { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<FieldType> FieldTypes { get; set; }
    public DbSet<FieldResponse> FieldResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        ConfigureGroup(modelBuilder);
        ConfigureForm(modelBuilder);
        ConfigureFieldGroup(modelBuilder);
        ConfigureSingleChoiceField(modelBuilder);
        ConfigureOptionResponse(modelBuilder);
        ConfigureOption(modelBuilder);
        ConfigureField(modelBuilder);
        ConfigureFieldType(modelBuilder);
        ConfigureFieldResponse(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }

    private static void ConfigureGroup(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Group> groupBuilder = modelBuilder.Entity<Group>();

        groupBuilder.HasKey(g => g.Id);
        groupBuilder.Property(g => g.Id).ValueGeneratedOnAdd();
        
        groupBuilder.HasOne(g => g.Parent)
                    .WithMany(g => g.SubGroups)
                    .HasForeignKey(g => g.ParentId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

        groupBuilder.HasMany(g => g.Forms)
                    .WithOne(f => f.Group)
                    .HasForeignKey(f => f.GroupId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureForm(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Form> formBuilder = modelBuilder.Entity<Form>();
        
        formBuilder.HasKey(f => f.Id);
        formBuilder.Property(f => f.Id).ValueGeneratedOnAdd();

        formBuilder.HasMany(f => f.FormFieldGroups)
                   .WithOne(ffg => ffg.Form)
                   .HasForeignKey(ffg => ffg.FormId)
                   .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureFieldGroup(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<FieldGroup> fieldGroupBuilder = modelBuilder.Entity<FieldGroup>();

        fieldGroupBuilder.HasKey(fg => fg.Id);
        fieldGroupBuilder.Property(fg => fg.Id).ValueGeneratedOnAdd();

        fieldGroupBuilder.HasMany(fg => fg.FormFieldGroups)
                         .WithOne(ffg => ffg.FieldGroup)
                         .HasForeignKey(ffg => ffg.FieldGroupId)
                         .OnDelete(DeleteBehavior.Cascade);
        
        fieldGroupBuilder.HasMany(fg => fg.FieldGroupSingleChoiceFields)
                         .WithOne(fgscf => fgscf.FieldGroup)
                         .HasForeignKey(fgscf => fgscf.FieldGroupId)
                         .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSingleChoiceField(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<SingleChoiceField> singleChoiceFieldBuilder = modelBuilder.Entity<SingleChoiceField>();
        
        singleChoiceFieldBuilder.HasKey(scf => scf.Id);
        singleChoiceFieldBuilder.Property(scf => scf.Id).ValueGeneratedOnAdd();
        
        singleChoiceFieldBuilder.HasMany(scf => scf.FieldGroupSingleChoiceFields)
                                .WithOne(fgscf => fgscf.SingleChoiceField)
                                .HasForeignKey(fgscf => fgscf.SingleChoiceFieldId)
                                .OnDelete(DeleteBehavior.Cascade);
        
        singleChoiceFieldBuilder.HasMany(scf => scf.Options)
                                .WithOne(o => o.SingleChoiceField)
                                .HasForeignKey(o => o.SingleChoiceFieldId)
                                .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureOptionResponse(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<OptionResponse> optionResponseBuilder = modelBuilder.Entity<OptionResponse>();
        
        optionResponseBuilder.HasKey(or => or.Id);
        optionResponseBuilder.Property(or => or.Id).ValueGeneratedOnAdd();

        optionResponseBuilder.HasOne(or => or.Option)
                             .WithMany(o => o.OptionResponses)
                             .HasForeignKey(or => or.OptionId)
                             .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureOption(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Option> optionBuilder = modelBuilder.Entity<Option>();
        
        optionBuilder.HasKey(o => o.Id);
        optionBuilder.Property(o => o.Id).ValueGeneratedOnAdd();

        optionBuilder.HasMany(o => o.OptionFields)
                     .WithOne(of => of.Option)
                     .HasForeignKey(of => of.OptionId)
                     .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureField(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Field> fieldBuilder = modelBuilder.Entity<Field>();
        
        fieldBuilder.HasKey(f => f.Id);
        fieldBuilder.Property(f => f.Id).ValueGeneratedOnAdd();

        fieldBuilder.Property(f => f.Description)
                    .IsRequired(false);
        
        fieldBuilder.HasMany(f => f.FieldGroupFields)
                    .WithOne(fgf => fgf.Field)
                    .HasForeignKey(fgf => fgf.FieldId)
                    .OnDelete(DeleteBehavior.Cascade);
        
        fieldBuilder.HasMany(f => f.OptionFields)
                    .WithOne(of => of.Field)
                    .HasForeignKey(of => of.FieldId)
                    .OnDelete(DeleteBehavior.Cascade);
        
        fieldBuilder.HasOne(f => f.FieldType)
                    .WithMany(ft => ft.Fields)
                    .HasForeignKey(f => f.FieldTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
        
        fieldBuilder.HasMany(f => f.FieldResponses)
                    .WithOne(fr => fr.Field)
                    .HasForeignKey(fr => fr.FieldId)
                    .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureFieldType(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<FieldType> fieldTypeBuilder = modelBuilder.Entity<FieldType>();
        
        fieldTypeBuilder.HasKey(ft => ft.Id);
        fieldTypeBuilder.Property(ft => ft.Id).ValueGeneratedOnAdd();

        fieldTypeBuilder.Property(ft => ft.Description)
                        .IsRequired(false);
    }

    private static void ConfigureFieldResponse(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<FieldResponse> fieldResponseBuilder = modelBuilder.Entity<FieldResponse>();
        
        fieldResponseBuilder.HasKey(r => r.Id);
        fieldResponseBuilder.Property(r => r.FieldId).ValueGeneratedOnAdd();
    }
}
