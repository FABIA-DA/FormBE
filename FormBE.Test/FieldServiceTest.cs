using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class FieldServiceTest
{
    private readonly IFieldRepository _mockFieldRepository;
    private readonly IFieldTypeRepository _mockFieldTypeRepository;
    private readonly IFieldService _fieldService;

    public FieldServiceTest()
    {
        _mockFieldRepository = Substitute.For<IFieldRepository>();
        _mockFieldTypeRepository = Substitute.For<IFieldTypeRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        ILogger<FieldService> logger = Substitute.For<ILogger<FieldService>>();
        _fieldService = new FieldService(_mockFieldRepository, _mockFieldTypeRepository, uow, logger);
    }

    [Fact]
    public async Task GetFieldsAsync()
    {
        List<Field> fields = Util.GetTestFields();

        _mockFieldRepository.GetFieldsAsync(TestContext.Current.CancellationToken).Returns(fields);

        IReadOnlyCollection<Field> result = await _fieldService.GetFieldsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(fields.Count);
        result.Should().BeEquivalentTo(fields);
    }

    [Fact]
    public async Task GetFieldByIdAsync()
    {
        Field testField = new()
        {
            Id = 0,
            Name = "Field 1",
            Description = null,
            IsOptional = false,
            FieldType = new()
            {
                Id = 0,
                Name = "Type 1",
                Description = null,
                Regex = String.Empty,
                Fields = []
            },
            FieldGroupFields = [],
            FieldResponses = [],
            OptionFields = []
        };

        _mockFieldRepository.GetFieldByIdAsync(testField.Id, false, TestContext.Current.CancellationToken)
                            .Returns(testField);

        OneOf<Field, NotFound> result
            = await _fieldService.GetFieldByIdAsync(testField.Id, TestContext.Current.CancellationToken);

        result.Switch(field =>
                      {
                          field.Name.Should().Be(field.Name);
                          field.Description.Should().Be(field.Description);
                          field.IsOptional.Should().Be(field.IsOptional);
                          field.FieldType.Should().Be(field.FieldType);
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetFieldByIdAsync_NotFound()
    {
        const long FieldId = 0;

        _mockFieldRepository.GetFieldByIdAsync(FieldId, false, TestContext.Current.CancellationToken)
                            .Returns((Field?) null);

        OneOf<Field, NotFound> result
            = await _fieldService.GetFieldByIdAsync(FieldId, TestContext.Current.CancellationToken);

        result.Switch(field => result.Should().NotBeOfType<Field>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task CreateFieldAsync_Success()
    {
        const string FieldName = "Field 1";
        const string? Description = null;
        const bool IsOptional = false;
        FieldType type = new()
        {
            Id = 0,
            Name = "Type 1",
            Description = null,
            Regex = String.Empty,
            Fields = []
        };

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(type.Id, true, TestContext.Current.CancellationToken)
                                .Returns(type);

        OneOf<Success<Field>, IFieldService.FieldTypeNotFound> result
            = await _fieldService.CreateFieldAsync(type.Id, FieldName, Description, IsOptional,
                                                   TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          success.Value.Name.Should().Be(FieldName);
                          success.Value.Description.Should().Be(Description);
                          success.Value.IsOptional.Should().Be(IsOptional);
                          success.Value.FieldType.Should().Be(type);
                      },
                      fieldTypeNotFound =>
                          result.Should().NotBeOfType<IFieldService.FieldTypeNotFound>("field type should be found"));
    }

    [Fact]
    public async Task CreateFieldAsync_FieldTypeNotFound()
    {
        const string FieldName = "Field 1";
        const string? Description = null;
        const bool IsOptional = false;
        const long FieldTypeId = 0;

        _mockFieldTypeRepository.GetFieldTypeByIdAsync(FieldTypeId, true, TestContext.Current.CancellationToken)
                                .Returns((FieldType?) null);

        OneOf<Success<Field>, IFieldService.FieldTypeNotFound> result
            = await _fieldService.CreateFieldAsync(FieldTypeId, FieldName, Description, IsOptional,
                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<Field>>("field type should not be found"),
                      fieldTypeNotFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task UpdateFieldAsync_Success()
    {
        const string NewName = "1.Field";
        const string? NewDescription = "Description";
        const bool NewIsOptional = true;

        Field testField = new()
        {
            Id = 0,
            FieldTypeId = 0,
            Name = "Field 1",
            Description = null,
            IsOptional = false,
            FieldType = new()
            {
                Id = 0,
                Name = "Type 1",
                Description = null,
                Regex = String.Empty,
                Fields = []
            },
            FieldGroupFields = [],
            FieldResponses = [],
            OptionFields = []
        };

        FieldType testType = new()
        {
            Id = 1,
            Name = "Type 2",
            Description = null,
            Regex = String.Empty,
            Fields = []
        };

        _mockFieldRepository.GetFieldByIdAsync(testField.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testField);
        _mockFieldTypeRepository.GetFieldTypeByIdAsync(testType.Id, true, TestContext.Current.CancellationToken)
                                .Returns(testType);

        OneOf<Success, NotFound, IFieldService.FieldTypeNotFound> result
            = await _fieldService.UpdateFieldAsync(testField.Id, testType.Id, NewName, NewDescription, NewIsOptional,
                                                   TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => notFound.Should().NotBeOfType<NotFound>("field should be found"),
                      fieldTypeNotFound =>
                          fieldTypeNotFound.Should().NotBeOfType<IFieldService.FieldTypeNotFound>("field type should be found"));
    }

    [Fact]
    public async Task UpdateFieldAsync_NotFound()
    {
        const string NewName = "1.Field";
        const string? NewDescription = "Description";
        const bool NewIsOptional = true;
        const long TestFieldId = 0;

        FieldType testType = new()
        {
            Id = 1,
            Name = "Type 2",
            Description = null,
            Regex = String.Empty,
            Fields = []
        };

        _mockFieldRepository.GetFieldByIdAsync(TestFieldId, true, TestContext.Current.CancellationToken)
                            .Returns((Field?) null);
        _mockFieldTypeRepository.GetFieldTypeByIdAsync(testType.Id, true, TestContext.Current.CancellationToken)
                                .Returns(testType);

        OneOf<Success, NotFound, IFieldService.FieldTypeNotFound> result
            = await _fieldService.UpdateFieldAsync(TestFieldId, testType.Id, NewName, NewDescription, NewIsOptional,
                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("field should not be found"),
                      notFound =>
                      {
                          // expected
                      },
                      fieldTypeNotFound =>
                          result.Should().NotBeOfType<IFieldService.FieldTypeNotFound>("field type should be found"));
    }

    [Fact]
    public async Task UpdateFieldAsync_FieldTypeNotFound()
    {
        const string NewName = "1.Field";
        const string? NewDescription = "Description";
        const bool NewIsOptional = true;
        const long TestFieldTypeId = 1;

        Field testField = new()
        {
            Id = 0,
            Name = "Field 1",
            Description = null,
            IsOptional = false,
            FieldType = new()
            {
                Id = 0,
                Name = "Type 1",
                Description = null,
                Regex = String.Empty,
                Fields = []
            },
            FieldGroupFields = [],
            FieldResponses = [],
            OptionFields = []
        };

        _mockFieldRepository.GetFieldByIdAsync(testField.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testField);
        _mockFieldTypeRepository.GetFieldTypeByIdAsync(TestFieldTypeId, true, TestContext.Current.CancellationToken)
                                .Returns((FieldType?) null);

        OneOf<Success, NotFound, IFieldService.FieldTypeNotFound> result
            = await _fieldService.UpdateFieldAsync(testField.Id, TestFieldTypeId, NewName, NewDescription,
                                                   NewIsOptional,
                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("field type should not be found"),
                      notFound => result.Should().NotBeOfType<NotFound>("field should be found"),
                      fieldTypeNotFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteFieldAsync_Success()
    {
        Field testField = new()
        {
            Id = 0,
            Name = "Field 1",
            Description = null,
            IsOptional = false,
            FieldType = new()
            {
                Id = 0,
                Name = "Type 1",
                Description = null,
                Regex = String.Empty,
                Fields = []
            },
            FieldGroupFields = [],
            FieldResponses = [],
            OptionFields = []
        };

        _mockFieldRepository.GetFieldByIdAsync(testField.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testField);

        OneOf<Success, NotFound> result
            = await _fieldService.DeleteFieldAsync(testField.Id, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteFieldAsync_NotFound()
    {
        const long TestFieldId = 0;

        _mockFieldRepository.GetFieldByIdAsync(TestFieldId, true, TestContext.Current.CancellationToken)
                            .Returns((Field?) null);

        OneOf<Success, NotFound> result
            = await _fieldService.DeleteFieldAsync(TestFieldId, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
