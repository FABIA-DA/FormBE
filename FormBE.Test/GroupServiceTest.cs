using FormBE.Persistence.Model;
using FormBE.Core.Services;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class GroupServiceTest
{
    private IGroupRepository _mockGroupRepository;
    private IFormRepository _mockFormRepository;
    private GroupService _groupService;

    public GroupServiceTest()
    {
        _mockGroupRepository = Substitute.For<IGroupRepository>();
        _mockFormRepository = Substitute.For<IFormRepository>();
        ILogger<GroupService> logger = Substitute.For<ILogger<GroupService>>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        _groupService = new GroupService(_mockGroupRepository, _mockFormRepository, uow, logger);
    }

    [Fact]
    public async Task GetGroupsAsync_Success()
    {
        IReadOnlyCollection<Group> testGroups = Util.GetTestGroups();
        _mockGroupRepository
            .GetGroupsAsync(TestContext.Current.CancellationToken, Arg.Is<HashSet<long>>(set => set.Count == 0))
            .Returns(testGroups);

        IReadOnlyCollection<Group> result = await _groupService.GetGroupsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(testGroups.Count, "same amount of groups");
        result.Should().BeEquivalentTo(testGroups, "same groups");
    }

    [Fact]
    public async Task GetGroupByIdAsync_Success()
    {
        Group testGroup = new()
        {
            Id = 0,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };
        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, false, TestContext.Current.CancellationToken)
                            .Returns(testGroup);

        OneOf<Group, NotFound> result
            = await _groupService.GetGroupByIdAsync(testGroup.Id, TestContext.Current.CancellationToken);

        result.Switch(group => group.Should().BeEquivalentTo(testGroup, "same group"),
                      notFround => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetGroupByIdAsync_NotFound()
    {
        long testGroupId = 0;

        _mockGroupRepository.GetGroupByIdAsync(testGroupId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);

        OneOf<Group, NotFound> result
            = await _groupService.GetGroupByIdAsync(testGroupId, TestContext.Current.CancellationToken);

        result.Switch(group => group.Should().NotBeOfType<Group>("should be not found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task CreateGroupAsync_Success(long? parentGroupId)
    {
        Group parentTestGroup = new()
        {
            Id = 0,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };
        if (parentGroupId.HasValue)
        {
            _mockGroupRepository.GetGroupByIdAsync(parentGroupId.Value, false, TestContext.Current.CancellationToken)
                                .Returns(parentTestGroup);
        }

        OneOf<Success<Group>, IGroupService.ParentNotFound> result
            = await _groupService.CreateGroupAsync(parentGroupId, "Sports", TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          success.Value.Name.Should().Be("Sports");
                          if (parentGroupId.HasValue)
                          {
                              success.Value.ParentId.Should().Be(parentGroupId.Value);
                          }
                      },
                      parentNotFound =>
                          result.Should().NotBeOfType<IGroupService.ParentNotFound>("parent should be found if set"));
    }

    [Fact]
    public async Task CreateGroupAsync_ParentNotFound()
    {
        long parentGroupId = 0;

        _mockGroupRepository.GetGroupByIdAsync(parentGroupId, true, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);

        OneOf<Success<Group>, IGroupService.ParentNotFound> result
            = await _groupService.CreateGroupAsync(parentGroupId, "Sports", TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<Group>>("parent cannot be found"),
                      parentNotFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null, "Anything")]
    [InlineData(1, "Forms")]
    [InlineData(1, "Anything")]
    [InlineData(null, "Forms")]
    public async Task UpdateGroupAsync_Success(long? newParentGroupId, string newName)
    {
        IReadOnlyCollection<Group> subGroups = Util.GetTestGroups();
        IReadOnlyCollection<Form> forms = Util.GetTestForms();

        Group testGroup = new()
        {
            Id = 5,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockGroupRepository.GetGroupsAsync(TestContext.Current.CancellationToken, subGroups.GetIds())
                            .Returns(subGroups);
        _mockFormRepository.GetFormsAsync(TestContext.Current.CancellationToken, forms.GetIds()).Returns(forms);

        if (newParentGroupId.HasValue)
        {
            Group parent = new()
            {
                Id = newParentGroupId.Value,
                Name = "Everything",
                SubGroups = [],
                Forms = []
            };
            _mockGroupRepository.GetGroupByIdAsync(parent.Id, false, TestContext.Current.CancellationToken)
                                .Returns(parent);
        }

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(testGroup.Id, newParentGroupId, newName, subGroups.GetIds(),
                                                   forms.GetIds(), TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"),
                      parentNotFound =>
                          result.Should().NotBeOfType<IGroupService.ParentNotFound>("parent should be found if set"),
                      parentIsSelf => result.Should().NotBeOfType<IGroupService.ParentIsSelf>("parent is not self"));
    }

    [Fact]
    public async Task UpdateGroupAsync_NotFound()
    {
        long testGroupId = 0;
        Group parent = new()
        {
            Id = 1,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroupId, true, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockGroupRepository.GetGroupByIdAsync(parent.Id, false, TestContext.Current.CancellationToken).Returns(parent);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(testGroupId, parent.Id, "", [], [],
                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      },
                      parentNotFound =>
                          result.Should().NotBeOfType<IGroupService.ParentNotFound>("parent should be found if set"),
                      parentIsSelf => result.Should().NotBeOfType<IGroupService.ParentIsSelf>("parent is not self"));
    }

    [Fact]
    public async Task UpdateGroupAsync_ParentNotFound()
    {
        Group testGroup = new()
        {
            Id = 0,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };
        long parentId = 1;

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockGroupRepository.GetGroupByIdAsync(parentId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(testGroup.Id, parentId, "", [], [],
                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("parent should not be found"),
                      notFound => result.Should().NotBeOfType<NotFound>("the group should be found"),
                      parentNotFound =>
                      {
                          // expected
                      },
                      parentIsSelf => result.Should().NotBeOfType<IGroupService.ParentIsSelf>("parent is not self"));
    }

    [Fact]
    public async Task UpdateGroupAsync_ParentIsSelf()
    {
        Group testGroup = new()
        {
            Id = 0,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(testGroup.Id, testGroup.Id, "", [], [],
                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("parent should be self"),
                      notFound => result.Should().NotBeOfType<NotFound>("the group should be found"),
                      parentNotFound =>
                          result.Should().NotBeOfType<IGroupService.ParentNotFound>("parent should be found if set"),
                      parentIsSelf =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteGroupAsync_Success()
    {
        Group testGroup = new()
        {
            Id = 0,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);

        OneOf<Success, NotFound>
            result = await _groupService.DeleteGroupAsync(testGroup.Id, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteGroupAsync_NotFound()
    {
        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns((Group?) null);

        OneOf<Success, NotFound>
            result = await _groupService.DeleteGroupAsync(0, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
