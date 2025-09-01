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
    private readonly IGroupRepository _mockGroupRepository;
    private readonly IFormRepository _mockFormRepository;
    private readonly GroupService _groupService;

    public GroupServiceTest()
    {
        _mockGroupRepository = Substitute.For<IGroupRepository>();
        _mockFormRepository = Substitute.For<IFormRepository>();
        ILogger<GroupService> logger = Substitute.For<ILogger<GroupService>>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        uow.GroupRepository.Returns(_mockGroupRepository);
        uow.FormRepository.Returns(_mockFormRepository);
        _groupService = new GroupService(uow, logger);
    }

    [Fact]
    public async Task GetGroupsAsync_Success()
    {
        IReadOnlyCollection<Group> testGroups = Util.GetTestGroups();
        _mockGroupRepository
            .GetGroupsAsync(TestContext.Current.CancellationToken)
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
            Id = 0L,
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
        const long TestGroupId = 0L;

        _mockGroupRepository.GetGroupByIdAsync(TestGroupId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);

        OneOf<Group, NotFound> result
            = await _groupService.GetGroupByIdAsync(TestGroupId, TestContext.Current.CancellationToken);

        result.Switch(group => group.Should().NotBeOfType<Group>("should be not found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    public async Task CreateGroupAsync_Success(long? parentGroupId)
    {
        List<Group> subgroups = Util.GetTestGroups();
        List<long> subgroupIds = subgroups.GetIds();
        List<Form> forms = Util.GetTestForms();
        List<long> formIds = forms.GetIds();
        
        Group parentTestGroup = new()
        {
            Id = 0L,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };
        if (parentGroupId.HasValue)
        {
            _mockGroupRepository.GetGroupByIdAsync(parentGroupId.Value, false, TestContext.Current.CancellationToken)
                                .Returns(parentTestGroup);
        }
        
        _mockGroupRepository.GetGroupsByIdsAsync(TestContext.Current.CancellationToken, subgroupIds).Returns(subgroups);
        _mockFormRepository.GetFormsByIdsAsync(TestContext.Current.CancellationToken, formIds).Returns(forms);

        OneOf<Success<Group>, IGroupService.ParentNotFound> result
            = await _groupService.CreateGroupAsync(parentGroupId, "Sports", subgroupIds, formIds, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          success.Value.Name.Should().Be("Sports");
                          success.Value.SubGroups.Should().BeEquivalentTo(subgroups, "should have subgroups set");
                          success.Value.Forms.Should().BeEquivalentTo(forms, "should have forms set");
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
        const long ParentGroupId = 0L;

        _mockGroupRepository.GetGroupByIdAsync(ParentGroupId, true, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockGroupRepository.GetGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);
        _mockFormRepository.GetFormsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

        OneOf<Success<Group>, IGroupService.ParentNotFound> result
            = await _groupService.CreateGroupAsync(ParentGroupId, "Sports", [], [], TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<Group>>("parent cannot be found"),
                      parentNotFound =>
                      {
                          // expected
                      });
    }

    [Theory]
    [InlineData(null, "Anything")]
    [InlineData(1L, "Forms")]
    [InlineData(1L, "Anything")]
    [InlineData(null, "Forms")]
    public async Task UpdateGroupAsync_Success(long? newParentGroupId, string newName)
    {
        IReadOnlyCollection<Group> subGroups = Util.GetTestGroups();
        IReadOnlyCollection<Form> forms = Util.GetTestForms();
        List<long> subgroupIds = subGroups.GetIds();
        List<long> formIds = forms.GetIds();

        Group testGroup = new()
        {
            Id = 5L,
            Name = "Forms",
            SubGroups = subGroups.Take(2).ToList(),
            Forms = forms.Take(2).ToList()
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockGroupRepository.GetGroupsByIdsAsync(TestContext.Current.CancellationToken, subgroupIds)
                            .Returns(subGroups);
        _mockFormRepository.GetFormsByIdsAsync(TestContext.Current.CancellationToken, formIds).Returns(forms);

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
            = await _groupService.UpdateGroupAsync(testGroup.Id, newParentGroupId, newName, subgroupIds,
                                                   formIds, TestContext.Current.CancellationToken);

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
        const long TestGroupId = 0L;
        Group parent = new()
        {
            Id = 1L,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };

        _mockGroupRepository.GetGroupByIdAsync(TestGroupId, true, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockGroupRepository.GetGroupByIdAsync(parent.Id, false, TestContext.Current.CancellationToken).Returns(parent);
        _mockGroupRepository.GetGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);
        _mockFormRepository.GetFormsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(TestGroupId, parent.Id, "", [], [],
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
            Id = 0L,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };
        const long ParentId = 1L;

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockGroupRepository.GetGroupByIdAsync(ParentId, false, TestContext.Current.CancellationToken)
                            .Returns((Group?) null);
        _mockGroupRepository.GetGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);
        _mockFormRepository.GetFormsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

        OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf> result
            = await _groupService.UpdateGroupAsync(testGroup.Id, ParentId, "", [], [],
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
            Id = 0L,
            Name = "Forms",
            SubGroups = [],
            Forms = []
        };

        _mockGroupRepository.GetGroupByIdAsync(testGroup.Id, true, TestContext.Current.CancellationToken)
                            .Returns(testGroup);
        _mockGroupRepository.GetGroupsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);
        _mockFormRepository.GetFormsByIdsAsync(TestContext.Current.CancellationToken, []).Returns([]);

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
            Id = 0L,
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
        const long TestGroupId = 0L;
        
        _mockGroupRepository.GetGroupByIdAsync(TestGroupId, true, TestContext.Current.CancellationToken).Returns((Group?) null);

        OneOf<Success, NotFound>
            result = await _groupService.DeleteGroupAsync(TestGroupId, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
