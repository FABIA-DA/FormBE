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
    private GroupService _groupService;

    public GroupServiceTest()
    {
        _mockGroupRepository = Substitute.For<IGroupRepository>();
        ILogger<GroupService> logger = Substitute.For<ILogger<GroupService>>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        _groupService = new GroupService(_mockGroupRepository, uow, logger);
    }

    [Fact]
    public async Task GetGroupsAsync_Success()
    {
        IReadOnlyCollection<Group> testGroups = GetTestGroups();
        _mockGroupRepository.GetGroupsAsync(TestContext.Current.CancellationToken).Returns(testGroups);

        IReadOnlyCollection<Group> result = await _groupService.GetGroupsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(testGroups.Count, "same amount of groups");
        result.Should().BeEquivalentTo(testGroups, "same groups");
    }

    [Fact]
    public async Task GetGroupByIdAsync_Success()
    {
        Group testGroup = GetTestGroups().First();
        _mockGroupRepository.GetGroupByIdAsync(0, false, TestContext.Current.CancellationToken).Returns(testGroup);

        OneOf<Group, NotFound> result = await _groupService.GetGroupByIdAsync(0, TestContext.Current.CancellationToken);

        result.Switch(group => group.Should().BeEquivalentTo(testGroup, "same group"),
                      notFround => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetGroupByIdAsync_NotFound()
    {
        _mockGroupRepository.GetGroupByIdAsync(0, false, TestContext.Current.CancellationToken).Returns((Group?) null);

        OneOf<Group, NotFound> result = await _groupService.GetGroupByIdAsync(0, TestContext.Current.CancellationToken);

        result.Switch(group => group.Should().NotBeOfType<Group>("should be not found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task CreateGroupAsync_Success(int? parentGroupId)
    {
        Group parentTestGroup = GetTestGroups().First();

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
        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns((Group?) null);

        OneOf<Success<Group>, IGroupService.ParentNotFound> result
            = await _groupService.CreateGroupAsync(0, "Sports", TestContext.Current.CancellationToken);

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
    public async Task UpdateGroupAsync_Success(int? newParentGroupId, string newName)
    {
        Group testGroup = GetTestGroups().First();

        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns(testGroup);

        if (newParentGroupId.HasValue)
        {
            Group parent = GetTestGroups().Skip(1).First();
            _mockGroupRepository.GetGroupByIdAsync(newParentGroupId.Value, false, TestContext.Current.CancellationToken)
                                .Returns(parent);
        }

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(0, newParentGroupId, newName, TestContext.Current.CancellationToken);

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
        Group testGroup = GetTestGroups().First();

        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns((Group?) null);
        _mockGroupRepository.GetGroupByIdAsync(1, false, TestContext.Current.CancellationToken).Returns(testGroup);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(0, 1, "", TestContext.Current.CancellationToken);

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
        Group testGroup = GetTestGroups().First();

        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns(testGroup);
        _mockGroupRepository.GetGroupByIdAsync(1, false, TestContext.Current.CancellationToken).Returns((Group?) null);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(0, 1, "", TestContext.Current.CancellationToken);

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
        Group testGroup = GetTestGroups().First();

        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns(testGroup);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(0, 0, "", TestContext.Current.CancellationToken);

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
        Group testGroup = GetTestGroups().First();

        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns(testGroup);

        OneOf<Success, NotFound>
            result = await _groupService.DeleteGroupAsync(0, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }
    
    [Fact]
    public async Task DeleteGroupAsync_NotFound()
    {
        _mockGroupRepository.GetGroupByIdAsync(0, true, TestContext.Current.CancellationToken).Returns((Group?)null);

        OneOf<Success, NotFound>
            result = await _groupService.DeleteGroupAsync(0, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    private static IReadOnlyCollection<Group> GetTestGroups() =>
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
}
