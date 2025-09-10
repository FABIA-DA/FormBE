using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class FormServiceTest
{
    private readonly IFormRepository _mockFormRepository;
    private readonly IGroupRepository _mockGroupRepository;
    private readonly IFieldGroupRepository _mockFieldGroupRepository;
    private readonly IFormService _formService;

    public FormServiceTest()
    {
        _mockFormRepository = Substitute.For<IFormRepository>();
        _mockGroupRepository = Substitute.For<IGroupRepository>();
        _mockFieldGroupRepository = Substitute.For<IFieldGroupRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        uow.FormRepository.Returns(_mockFormRepository);
        uow.GroupRepository.Returns(_mockGroupRepository);
        uow.FieldGroupRepository.Returns(_mockFieldGroupRepository);
        ILogger<FormService> logger = Substitute.For<ILogger<FormService>>();
        _formService = new FormService(uow,
                                       logger);
    }

    [Fact]
    public async Task GetFormsAsync_Success()
    {
        List<(long Id, string Name, long? GroupId, string? GroupName, int FieldGroupCount)> testForms
            = Util.GetTestForms();
        _mockFormRepository
            .GetFormsAsync(TestContext.Current.CancellationToken)
            .Returns(testForms);

        IReadOnlyCollection<(long Id, string Name, long? GroupId, string? GroupName, int FieldGroupCount)> result
            = await _formService.GetFormsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(testForms.Count, "count should be same as from the repo");
        result.Should().BeEquivalentTo(testForms, "forms should be same as from the repo");
    }

    [Fact]
    public async Task GetFormByIdAsync_Success()
    {
        Form testForm = new()
        {
            Id = 0L,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        };
        _mockFormRepository.GetFormByIdAsync(testForm.Id, false, TestContext.Current.CancellationToken)
                           .Returns(testForm);

        OneOf<Form, NotFound> result
            = await _formService.GetFormByIdAsync(testForm.Id, TestContext.Current.CancellationToken);

        result.Switch(form => form.Should().BeEquivalentTo(testForm, "should same as from the repo"),
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetFormByIdAsync_NotFound()
    {
        const long FormId = 0L;

        _mockFormRepository.GetFormByIdAsync(FormId, false, TestContext.Current.CancellationToken)
                           .Returns((Form?) null);

        OneOf<Form, NotFound> result
            = await _formService.GetFormByIdAsync(FormId, TestContext.Current.CancellationToken);

        result.Switch(form => result.Should().NotBeOfType<Form>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null, "Space Form")]
    [InlineData(0L, "Small Business Form")]
    public async Task CreateFormAsync_Success(long? groupId, string name)
    {
        List<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)>
            fieldGroups = Util.GetTestFieldGroups();
        List<FieldGroup> testFieldGroups = fieldGroups.GetFieldGroups();
        List<long> fieldGroupIds = fieldGroups.GetIds();

        if (groupId.HasValue)
        {
            Group testGroup = new()
            {
                Id = groupId.Value,
                Name = "Test Group",
                Forms = [],
                SubGroups = []
            };
            _mockGroupRepository.GetGroupByIdAsync(groupId.Value, false, TestContext.Current.CancellationToken)
                                .Returns(testGroup);
        }

        _mockFieldGroupRepository.GetFieldGroupsByIdsAsync(TestContext.Current.CancellationToken, fieldGroupIds)
                                 .Returns(testFieldGroups);

        OneOf<Success<Form>, IFormService.GroupNotFound> result
            = await _formService.CreateFormAsync(groupId, name, fieldGroupIds, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          success.Value.GroupId.Should().Be(groupId, "groupId should be successfully set");
                          success.Value.Name.Should().Be(name, "name should be successfully set");
                      },
                      groupNotFound =>
                          result.Should().NotBeOfType<IFormService.GroupNotFound>("should be found if set"));
    }

    [Fact]
    public async Task CreateFormAsync_GroupNotFound()
    {
        const long FormId = 0L;

        _mockGroupRepository.GetGroupByIdAsync(FormId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockFieldGroupRepository.GetFieldGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

        OneOf<Success<Form>, IFormService.GroupNotFound> result
            = await _formService.CreateFormAsync(FormId, "Space Form", [], TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<Form>>("group should not be found"),
                      groupNotFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null, "House Building Form")]
    [InlineData(null, "Space Form")]
    [InlineData(1L, "House Building Form")]
    [InlineData(1L, "Space Form")]
    public async Task UpdateFormAsync_Success(long? newGroupId, string newName)
    {
        List<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)>
            fieldGroups = Util.GetTestFieldGroups();
        List<FieldGroup> testFieldGroups = fieldGroups.GetFieldGroups();
        List<long> fieldGroupIds = fieldGroups.GetIds();

        Form testForm = new()
        {
            Id = 0L,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        };

        testForm.FormFieldGroups = testFieldGroups.Take(2).Select(g => new FormFieldGroup()
        {
            FormId = testForm.Id,
            FieldGroupId = g.Id,
            Form = testForm,
            FieldGroup = g
        }).ToList();

        fieldGroupIds = fieldGroupIds.Skip(2).ToList();

        _mockFormRepository.GetFormByIdAsync(testForm.Id, true, TestContext.Current.CancellationToken)
                           .Returns(testForm);
        _mockFieldGroupRepository
            .GetFieldGroupsByIdsAsync(TestContext.Current.CancellationToken,
                                      Arg.Is<List<long>>(ids => ids.SequenceEqual(fieldGroupIds)))
            .Returns(testFieldGroups);

        if (newGroupId.HasValue)
        {
            Group testGroup = new()
            {
                Id = newGroupId.Value,
                Name = "Test Group",
                Forms = [],
                SubGroups = []
            };

            _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, false, TestContext.Current.CancellationToken)
                                .Returns(testGroup);
        }

        OneOf<Success, NotFound, IFormService.GroupNotFound> result
            = await _formService.UpdateFormAsync(testForm.Id, newGroupId, newName, fieldGroupIds,
                                                 TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"),
                      groupNotFound =>
                          result.Should().NotBeOfType<IFormService.GroupNotFound>("group should be found if set"));
    }

    [Fact]
    public async Task UpdateFormAsync_NotFound()
    {
        Group testGroup = new()
        {
            Id = 0L,
            Name = "Test Group",
            Forms = [],
            SubGroups = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, false, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockFormRepository.GetFormByIdAsync(0, true, TestContext.Current.CancellationToken).Returns((Form?) null);
        _mockFieldGroupRepository.GetFieldGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

        OneOf<Success, NotFound, IFormService.GroupNotFound> result
            = await _formService.UpdateFormAsync(0, testGroup.Id, "New Name", [],
                                                 TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      },
                      groupNotFound =>
                          result.Should().NotBeOfType<IFormService.GroupNotFound>("group should be found if set"));
    }

    [Fact]
    public async Task UpdateFormAsync_GroupNotFound()
    {
        const long TestGroupId = 0;
        Form testForm = new()
        {
            Id = 0,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        };

        _mockGroupRepository.GetGroupByIdAsync(TestGroupId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockFormRepository.GetFormByIdAsync(testForm.Id, true, TestContext.Current.CancellationToken)
                           .Returns(testForm);
        _mockFieldGroupRepository.GetFieldGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

        OneOf<Success, NotFound, IFormService.GroupNotFound> result
            = await _formService.UpdateFormAsync(testForm.Id, TestGroupId, "New Name", [],
                                                 TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("group should not be found"),
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"),
                      groupNotFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteFormAsync_Success()
    {
        Form testForm = new()
        {
            Id = 0L,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        };

        _mockFormRepository.GetFormByIdAsync(testForm.Id, true, TestContext.Current.CancellationToken)
                           .Returns(testForm);

        OneOf<Success, NotFound> result
            = await _formService.DeleteFormAsync(testForm.Id, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should not be found"));
    }

    [Fact]
    public async Task DeleteFormAsync_NotFound()
    {
        const long TestFormId = 0L;

        _mockFormRepository.GetFormByIdAsync(TestFormId, true, TestContext.Current.CancellationToken)
                           .Returns((Form?) null);

        OneOf<Success, NotFound> result
            = await _formService.DeleteFormAsync(TestFormId, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
