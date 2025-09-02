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
        long parentGroupId = 0L;
        long fieldGroupId = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var g1 = new Group()
            {
                Name = "Special Events",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var fg1 = new FieldGroup()
            {
                Name = "Location",
                FormFieldGroups = [],
                FieldGroupFields = [],
                FieldGroupSingleChoiceFields = []
            };
            
            ctx.Groups.Add(g1);

            ctx.FieldGroups.Add(fg1);

            await ctx.SaveChangesAsync(TestCancellationToken);
            
            parentGroupId = g1.Id;
            fieldGroupId = fg1.Id;
        });
        
        FormCreationRequest request = new FormCreationRequest()
        {
            Name = "Super Special Events",
            GroupId = parentGroupId,
            FieldGroupIds = [fieldGroupId]
        };

        var response
            = await ApiClient.PostAsJsonAsync<FormCreationRequest>("/api/forms", request, JsonOptions,
                                                                   TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/forms/");
        
        var content = await response.Content.ReadFromJsonAsync<FormDto>(TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);
        
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
        long formId = 0L;
        long groupId = 0L;
        long fieldGroupId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var g1 = new Group()
            {
                Name = "Inquiry",
                ParentId = null,
                Parent = null,
                SubGroups = [],
                Forms = []
            };
            var f1 = new Form()
            {
                Name = "Super Special Events",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            };
            var fg1 = new FieldGroup()
            {
                Name = "Location",
                FormFieldGroups = [],
                FieldGroupFields = [],
                FieldGroupSingleChoiceFields = []
            };
            
            ctx.Groups.Add(g1);
            ctx.Forms.Add(f1);
            ctx.FieldGroups.Add(fg1);

            await ctx.SaveChangesAsync(TestCancellationToken);

            formId = f1.Id;
            groupId = g1.Id;
            fieldGroupId = fg1.Id;
        });
        
        FormUpdateRequest request = new FormUpdateRequest()
        {
            Name = "Super Unspecial Events",
            GroupId = groupId,
            FieldGroupIds = [fieldGroupId]
        };

        var response = await ApiClient.PutAsJsonAsync($"/api/forms/{formId}", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/forms/{formId}", TestCancellationToken);
        
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
        long formId = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var f1 = new Form()
            {
                Name = "Normal Events",
                GroupId = null,
                Group = null,
                FormFieldGroups = []
            };
            
            ctx.Forms.Add(f1);

            await ctx.SaveChangesAsync(TestCancellationToken);
            
            formId = f1.Id;
        });
        
        var response = await ApiClient.DeleteAsync($"/api/forms/{formId}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/forms/{formId}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
