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
    private IFormRepository _mockFormRepository;
    private IGroupRepository _mockGroupRepository;
    private IFormService _formService;

    public FormServiceTest()
    {
        _mockFormRepository = Substitute.For<IFormRepository>();
        _mockGroupRepository = Substitute.For<IGroupRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        ILogger<FormService> logger = Substitute.For<ILogger<FormService>>();
        _formService = new FormService(_mockFormRepository, _mockGroupRepository, uow, logger);
    }

    [Fact]
    public async Task GetFormsAsync_Success()
    {
        IReadOnlyCollection<Form> testForms = Util.GetTestForms();
        _mockFormRepository.GetFormsAsync(TestContext.Current.CancellationToken, Arg.Is<HashSet<long>>(set => set.Count == 0)).Returns(testForms);

        IReadOnlyCollection<Form> result = await _formService.GetFormsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(testForms.Count, "count should be same as from the repo");
        result.Should().BeEquivalentTo(testForms, "forms should be same as from the repo");
    }

    [Fact]
    public async Task GetFormByIdAsync_Success()
    {
        Form testForm = new()
        {
            Id = 0,
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
        _mockFormRepository.GetFormByIdAsync(0, false, TestContext.Current.CancellationToken).Returns((Form?) null);

        OneOf<Form, NotFound> result = await _formService.GetFormByIdAsync(0, TestContext.Current.CancellationToken);

        result.Switch(form => result.Should().NotBeOfType<Form>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null, "Space Form")]
    [InlineData(0, "Small Business Form")]
    public async Task CreateFormAsync_Success(long? groupId, string name)
    {
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

        OneOf<Success<Form>, IFormService.GroupNotFound> result
            = await _formService.CreateFormAsync(groupId, name, TestContext.Current.CancellationToken);

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
        _mockGroupRepository.GetGroupByIdAsync(0, false, TestContext.Current.CancellationToken).Returns((Group?) null);

        OneOf<Success<Form>, IFormService.GroupNotFound> result
            = await _formService.CreateFormAsync(0, "Space Form", TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<Form>>("group should not be found"),
                      groupNotFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null, "House Building Form")]
    [InlineData(null, "Space Form")]
    [InlineData(1, "House Building Form")]
    [InlineData(1, "Space Form")]
    public async Task UpdateFormAsync_Success(long? newGroupId, string newName)
    {
        Form testForm = new()
        {
            Id = 0,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        };

        _mockFormRepository.GetFormByIdAsync(testForm.Id, true, TestContext.Current.CancellationToken)
                           .Returns(testForm);

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
            = await _formService.UpdateFormAsync(testForm.Id, newGroupId, newName,
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
            Id = 0,
            Name = "Test Group",
            Forms = [],
            SubGroups = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, false, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockFormRepository.GetFormByIdAsync(0, true, TestContext.Current.CancellationToken).Returns((Form?) null);

        OneOf<Success, NotFound, IFormService.GroupNotFound> result
            = await _formService.UpdateFormAsync(0, testGroup.Id, "New Name",
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
        long testGroupId = 0;
        Form testForm = new()
        {
            Id = 0,
            GroupId = null,
            Name = "House Building Form",
            Group = null,
            FormFieldGroups = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroupId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockFormRepository.GetFormByIdAsync(testForm.Id, true, TestContext.Current.CancellationToken)
                           .Returns(testForm);

        OneOf<Success, NotFound, IFormService.GroupNotFound> result
            = await _formService.UpdateFormAsync(testForm.Id, testGroupId, "New Name",
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
            Id = 0,
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
        long testFormId = 0;

        _mockFormRepository.GetFormByIdAsync(testFormId, true, TestContext.Current.CancellationToken)
                           .Returns((Form?) null);

        OneOf<Success, NotFound> result
            = await _formService.DeleteFormAsync(testFormId, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
