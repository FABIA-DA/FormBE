using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class FieldTypeServiceTest
{
    private readonly IFieldTypeRepository _mockFieldTypeRepository;
    private readonly IFieldTypeService _fieldTypeService;

    public FieldTypeServiceTest()
    {
        _mockFieldTypeRepository = Substitute.For<IFieldTypeRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        ILogger<FieldTypeService> logger = Substitute.For<ILogger<FieldTypeService>>();
        _fieldTypeService = new FieldTypeService(_mockFieldTypeRepository, uow, logger);
    }

    [Fact]
    public async Task GetFieldTypesAsync()
    {
        List<FieldType> fieldTypes = Util.GetTestFieldTypes();

        _mockFieldTypeRepository.GetAllFieldTypes(TestContext.Current.CancellationToken).Returns(fieldTypes);

        IReadOnlyCollection<FieldType> result
            = await _fieldTypeService.GetFieldTypesAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(fieldTypes.Count);
        result.Should().BeEquivalentTo(fieldTypes);
    }

    [Fact]
    public async Task GetFieldTypeByIdAsync_Success()
    {
        FieldType type = new()
        {
            Id = 0,
            Name = "test",
            Description = "test",
            Regex = "test",
            Fields = []
        };

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(type.Id, false, TestContext.Current.CancellationToken)
                                .Returns(type);

        OneOf<FieldType, NotFound> result
            = await _fieldTypeService.GetFieldTypeByIdAsync(type.Id, TestContext.Current.CancellationToken);

        result.Switch(fieldType => { fieldType.Should().BeEquivalentTo(type); },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetFieldTypeByIdAsync_NotFound()
    {
        const long FieldTypeId = 0;

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(FieldTypeId, false, TestContext.Current.CancellationToken)
                                .Returns((FieldType?) null);

        OneOf<FieldType, NotFound> result
            = await _fieldTypeService.GetFieldTypeByIdAsync(FieldTypeId, TestContext.Current.CancellationToken);

        result.Switch(fieldType => result.Should().NotBeOfType<FieldType>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task CreateFieldTypeAsync_Success()
    {
        const string Name = "test";
        const string? Description = null;
        const string Regex = "test";

        FieldType result
            = await _fieldTypeService.CreateFieldTypeAsync(Name, Description, Regex,
                                                           TestContext.Current.CancellationToken);

        result.Name.Should().Be(Name);
        result.Description.Should().Be(Description);
        result.Regex.Should().Be(Regex);
    }

    [Fact]
    public async Task UpdateFieldTypeAsync_Success()
    {
        const string Name = "1.test";
        const string? Description = "Desc";
        const string Regex = "\\d.test";

        FieldType type = new()
        {
            Id = 0,
            Name = "test",
            Description = "test",
            Regex = "test",
            Fields = []
        };

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(type.Id, true, TestContext.Current.CancellationToken)
                                .Returns(type);

        OneOf<Success, NotFound> result
            = await _fieldTypeService.UpdateFieldTypeAsync(type.Id, Name, Description, Regex,
                                                           TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task UpdateFieldTypeAsync_NotFound()
    {
        const long FieldTypeId = 0;
        const string Name = "1.test";
        const string? Description = "Desc";
        const string Regex = "\\d.test";

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(FieldTypeId, true, TestContext.Current.CancellationToken)
                                .Returns((FieldType?) null);

        OneOf<Success, NotFound> result
            = await _fieldTypeService.UpdateFieldTypeAsync(FieldTypeId, Name, Description, Regex,
                                                           TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteFieldTypeAsync_Success()
    {
        FieldType type = new()
        {
            Id = 0,
            Name = "test",
            Description = "test",
            Regex = "test",
            Fields = []
        };

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(type.Id, true, TestContext.Current.CancellationToken)
                                .Returns(type);

        OneOf<Success, NotFound> result
            = await _fieldTypeService.DeleteFieldTypeAsync(type.Id, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteFieldTypeAsync_NotFound()
    {
        const long FieldTypeId = 0;

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(FieldTypeId, true, TestContext.Current.CancellationToken)
                                .Returns((FieldType?) null);

        OneOf<Success, NotFound> result
            = await _fieldTypeService.DeleteFieldTypeAsync(FieldTypeId, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
