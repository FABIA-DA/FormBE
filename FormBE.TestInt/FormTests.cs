using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class FormTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllForms_Existing_Success()
    {
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Forms.AddRange(new Form()
                               {
                                   Name = "Inquiry",
                                   GroupId = null,
                                   Group = null,
                                   FormFieldGroups = []
                               },
                               new Form()
                               {
                                   Name = "Request",
                                   GroupId = null,
                                   Group = null,
                                   FormFieldGroups = []
                               },
                               new Form()
                               {
                                   Name = "Special Events",
                                   GroupId = null,
                                   Group = null,
                                   FormFieldGroups = []
                               });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.GetAsync("/api/forms", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<FormListResponse>(TestCancellationToken);

        content.Should().NotBeNull();
        content.Forms.Should().NotBeEmpty()
               .And.HaveCount(3);
    }

    [Fact]
    public async Task CreateForm_CheckExistence_Success()
    {
        const long Id = 1L;
        FormCreationRequest request = new FormCreationRequest()
        {
            Name = "Super Special Events",
            GroupId = 1L,
            FieldGroupIds = [1L]
        };

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Groups.Add(new Group()
            {
                Name = "Special Events",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            });

            ctx.FieldGroups.Add(new FieldGroup()
            {
                Name = "Location",
                FormFieldGroups = [],
                FieldGroupFields = [],
                FieldGroupSingleChoiceFields = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response
            = await ApiClient.PostAsJsonAsync<FormCreationRequest>("/api/forms", request, JsonOptions,
                                                                   TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().Be($"/api/forms/{Id}");
        
        var content = await response.Content.ReadFromJsonAsync<FormDto>(TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        var getResponse = await ApiClient.GetAsync($"/api/forms/{Id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FormDto>(TestCancellationToken);

        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);

        return;

        static void ShouldBeSameProperties(FormDto response, FormCreationRequest request)
        {
            response.Name.Should().Be(request.Name);
            response.GroupId.Should().Be(request.GroupId);
            response.FieldGroups.Should().ContainSingle(fg => fg.Id == request.FieldGroupIds[0]);
        }
    }

    [Fact]
    public async Task UpdateForm_CheckChange_Success()
    {
        const long Id = 1L;
        FormUpdateRequest request = new FormUpdateRequest()
        {
            Name = "Super Unspecial Events",
            GroupId = 1L,
            FieldGroupIds = [1L]
        };
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Groups.Add(new Group()
            {
                Name = "Inquiry",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            });

            ctx.Forms.Add(new Form()
            {
                Name = "Super Special Events",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            });

            ctx.FieldGroups.Add(new FieldGroup()
            {
                Name = "Location",
                FormFieldGroups = [],
                FieldGroupFields = [],
                FieldGroupSingleChoiceFields = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.PutAsJsonAsync($"/api/forms/{Id}", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/forms/{Id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FormDto>(TestCancellationToken);
        
        getContent.Should().NotBeNull();
        getContent.Name.Should().Be(request.Name);
        getContent.GroupId.Should().Be(request.GroupId);
        getContent.FieldGroups.Should().ContainSingle(fg => fg.Id == request.FieldGroupIds[0]);
    }

    [Fact]
    public async Task DeleteForm_CheckExistence_Success()
    {
        const long Id = 1L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Forms.Add(new Form()
            {
                Name = "Normal Events",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });
        
        var response = await ApiClient.DeleteAsync($"/api/forms/{Id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/forms/{Id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
