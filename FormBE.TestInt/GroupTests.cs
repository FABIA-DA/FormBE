using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class GroupTests(WebApiTestFixture webApiTestFixture) :
    WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllGroups_ExistingGroups_Success()
    {
        List<string> groupNames = ["Sports", "Economy", "Business Events"];

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Groups.AddRange(new Group()
                                {
                                    Name = groupNames[0],
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                },
                                new Group()
                                {
                                    Name = groupNames[1],
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                },
                                new Group()
                                {
                                    Name = groupNames[2],
                                    ParentId = 2L,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var groupResponse = await ApiClient.GetAsync("/api/groups", TestCancellationToken);

        groupResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var groupContent
            = await groupResponse.Content.ReadFromJsonAsync<GroupListResponse>(JsonOptions, TestCancellationToken);

        groupContent.Should().NotBeNull();
        groupContent.Groups.Should().NotBeEmpty()
                    .And.HaveCount(3);
        groupContent.Groups.Select(g => g.Name)
                    .Should().BeEquivalentTo(groupNames);
        groupContent.Groups.Should().ContainSingle(g => g.ParentId == 2L);
    }

    [Fact]
    public async Task CreateGroup_CheckExistence_Success()
    {
        GroupCreationRequest request = new GroupCreationRequest()
        {
            Name = "Sport Events",
            ParentId = 1L,
            GroupIds = [2L],
            FormIds = [1L]
        };

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Groups.AddRange(new Group()
                                {
                                    Name = "Sports",
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                },
                                new Group()
                                {
                                    Name = "Mass Sport Events",
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                });

            ctx.Forms.Add(new Form()
            {
                Name = "Financial Assistance",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response
            = await ApiClient.PostAsJsonAsync("/api/groups", request, JsonOptions,
                                              TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<GroupDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().Be("/api/groups/3");

        var getResponse = await ApiClient.GetAsync("/api/groups/3", TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<GroupDto>(JsonOptions, TestCancellationToken);

        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);

        return;

        static void ShouldBeSameProperties(GroupDto response, GroupCreationRequest request)
        {
            response.Name.Should().Be(request.Name);
            response.ParentId.Should().Be(request.ParentId);
            response.SubGroups.Should().ContainSingle(g => g.Id == request.GroupIds[0]);
            response.Forms.Should().ContainSingle(g => g.Id == request.FormIds[0]);
        }
    }

    [Fact]
    public async Task UpdateGroup_CheckChange_Success()
    {
        const long Id = 2L;
        GroupUpdateRequest request = new GroupUpdateRequest()
        {
            Name = "Sport Events",
            ParentId = 1L,
            SubGroupIds = [3L],
            FormIds = [1L]
        };
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Groups.AddRange(new Group()
                                {
                                    Name = "Sports",
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                },
                                new Group()
                                {
                                    Name = "Business Events",
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                },
                                new Group()
                                {
                                    Name = "Mass Sport Events",
                                    ParentId = null,
                                    Parent = null,
                                    SubGroups = [],
                                    Forms = []
                                });

            ctx.Forms.Add(new Form()
            {
                Name = "Inquiry",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.PutAsJsonAsync($"/api/groups/{Id}", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/groups/{Id}", TestCancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadFromJsonAsync<GroupDto>(JsonOptions, TestCancellationToken);
        getContent.Should().NotBeNull();
        getContent.Id.Should().Be(Id);
        getContent.Name.Should().Be(request.Name);
        getContent.ParentId.Should().Be(request.ParentId);
        getContent.SubGroups.Should().ContainSingle(g => g.Id == 3L);
        getContent.Forms.Should().ContainSingle(f => f.Id == 1L);
    }

    [Fact]
    public async Task DeleteGroup_CheckExistence_Success()
    {
        const long Id = 1L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Groups.Add(new Group()
            {
                Name = "Sports",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.DeleteAsync($"/api/groups/{Id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/groups/{Id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
