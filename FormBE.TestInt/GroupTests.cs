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
        long parentId = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var g1 = new Group()
            {
                Name = groupNames[0],
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var g2 = new Group()
            {
                Name = groupNames[1],
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var g3 = new Group()
            {
                Name = groupNames[2],
                Parent = g1,
                SubGroups = [],
                Forms = []
            };
            
            ctx.Groups.AddRange(g1, g2, g3);

            await ctx.SaveChangesAsync(TestCancellationToken);

            parentId = g1.Id;
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
        groupContent.Groups.Should().ContainSingle(g => g.ParentId == parentId);
    }

    [Fact]
    public async Task CreateGroup_CheckExistence_Success()
    {
        long parentId = 0L;
        long subgroupId = 0L;
        long formId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var g1 = new Group()
            {
                Name = "Sports",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var g2 = new Group()
            {
                Name = "Mass Sport Events",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var f1 = new Form()
            {
                Name = "Financial Assistance",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            };
            
            ctx.Groups.AddRange(g1, g2);

            ctx.Forms.Add(f1);

            await ctx.SaveChangesAsync(TestCancellationToken);
            
            parentId = g1.Id;
            subgroupId = g2.Id;
            formId = f1.Id;
        });
        
        GroupCreationRequest request = new GroupCreationRequest()
        {
            Name = "Sport Events",
            ParentId = parentId,
            SubgroupIds = [subgroupId],
            FormIds = [formId]
        };

        var response
            = await ApiClient.PostAsJsonAsync("/api/groups", request, JsonOptions,
                                              TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<GroupDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/groups/");

        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<GroupDto>(JsonOptions, TestCancellationToken);

        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);

        return;

        static void ShouldBeSameProperties(GroupDto response, GroupCreationRequest request)
        {
            response.Name.Should().Be(request.Name);
            response.ParentId.Should().Be(request.ParentId);
            response.SubGroups.Should().ContainSingle(g => g.Id == request.SubgroupIds[0]);
            response.Forms.Should().ContainSingle(g => g.Id == request.FormIds[0]);
        }
    }

    [Fact]
    public async Task UpdateGroup_CheckChange_Success()
    {
        long groupId = 0L;
        long parentId = 0L;
        long subgroupId = 0L;
        long formId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var g1 = new Group()
            {
                Name = "Sports",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var g2 = new Group()
            {
                Name = "Business Events",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var g3 = new Group()
            {
                Name = "Mass Sport Events",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var f1 = new Form()
            {
                Name = "Inquiry",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            };
            
            ctx.Groups.AddRange(g1, g2, g3);

            ctx.Forms.Add(f1);

            await ctx.SaveChangesAsync(TestCancellationToken);

            parentId = g1.Id;
            groupId = g2.Id;
            subgroupId = g3.Id;
            formId = f1.Id;
        });
        
        GroupUpdateRequest request = new GroupUpdateRequest()
        {
            Name = "Sport Events",
            ParentId = parentId,
            SubgroupIds = [subgroupId],
            FormIds = [formId]
        };

        var response = await ApiClient.PutAsJsonAsync($"/api/groups/{groupId}", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/groups/{groupId}", TestCancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadFromJsonAsync<GroupDto>(JsonOptions, TestCancellationToken);
        getContent.Should().NotBeNull();
        getContent.Name.Should().Be(request.Name);
        getContent.ParentId.Should().Be(request.ParentId);
        getContent.SubGroups.Should().ContainSingle(g => g.Id == subgroupId);
        getContent.Forms.Should().ContainSingle(f => f.Id == formId);
    }

    [Fact]
    public async Task DeleteGroup_CheckExistence_Success()
    {
        long groupId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var g1 = new Group()
            {
                Name = "Sports",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            
            ctx.Groups.Add(g1);

            await ctx.SaveChangesAsync(TestCancellationToken);
            
            groupId = g1.Id;
        });

        var response = await ApiClient.DeleteAsync($"/api/groups/{groupId}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/groups/{groupId}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
