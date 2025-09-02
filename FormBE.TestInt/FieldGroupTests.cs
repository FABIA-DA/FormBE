using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class FieldGroupTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllFieldGroups_CheckExistence_Success()
    {
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.FieldGroups.AddRange(new FieldGroup()
                                     {
                                         Name = "Location",
                                         FormFieldGroups = [],
                                         FieldGroupSingleChoiceFields = [],
                                         FieldGroupFields = []
                                     },
                                     new FieldGroup()
                                     {
                                         Name = "Business Type",
                                         FormFieldGroups = [],
                                         FieldGroupSingleChoiceFields = [],
                                         FieldGroupFields = []
                                     },
                                     new FieldGroup()
                                     {
                                         Name = "Personal Data",
                                         FormFieldGroups = [],
                                         FieldGroupSingleChoiceFields = [],
                                         FieldGroupFields = []
                                     });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.GetAsync("/api/field-groups", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content
            = await response.Content.ReadFromJsonAsync<FieldGroupListResponse>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.FieldGroups.Should().NotBeEmpty()
               .And.HaveCount(3);
    }

    [Fact]
    public async Task CreateFieldGroup_CheckExistence_Success()
    {
        long scfId = 0L;
        long fId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var scf = new SingleChoiceField()
            {
                Name = "Living",
                FieldGroupSingleChoiceFields = [],
                Options = [],
            };
            var f = new Field()
            {
                Name = "Postal Code",
                Description = null,
                IsOptional = false,
                FieldType = new FieldType()
                {
                    Name = "Four Numbers",
                    Description = null,
                    Regex = "\\d{4}",
                    Fields = []
                },
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            };
            
            ctx.SingleChoiceFields.Add(scf);

            ctx.Fields.Add(f);

            await ctx.SaveChangesAsync(TestCancellationToken);

            scfId = scf.Id;
            fId = f.Id;
        });
        
        FieldGroupCreationRequest request = new FieldGroupCreationRequest()
        {
            Name = "Location",
            SingleChoiceFieldIds = [scfId],
            FieldIds = [fId]
        };

        var response = await ApiClient.PostAsJsonAsync("/api/field-groups", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/field-groups/");

        var content = await response.Content.ReadFromJsonAsync<FieldGroupDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);

        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldGroupDto>(JsonOptions, TestCancellationToken);

        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
        
        static void ShouldBeSameProperties(FieldGroupDto response, FieldGroupCreationRequest request)
        {
            response.Name.Should().Be(request.Name);
            response.Fields.Should().NotBeEmpty()
                    .And.HaveCount(1)
                    .And.ContainSingle(f => f.Id == request.FieldIds[0]);
            response.SingleChoiceFields.Should().NotBeEmpty()
                    .And.HaveCount(1)
                    .And.ContainSingle(f => f.Id == request.SingleChoiceFieldIds[0]);
        }
    }

    [Fact]
    public async Task UpdateFieldGroup_CheckChange_Success()
    {
        long fieldGroupId = 0L;
        long scfId = 0L;
        long fId = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var scf = new SingleChoiceField()
            {
                Name = "Living",
                FieldGroupSingleChoiceFields = [],
                Options = [],
            };
            var f = new Field()
            {
                Name = "Postal Code",
                Description = null,
                IsOptional = false,
                FieldType = new FieldType()
                {
                    Name = "Four Numbers",
                    Description = null,
                    Regex = "\\d{4}",
                    Fields = []
                },
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            };
            var fg = new FieldGroup()
            {
                Name = "Postal Code",
                FormFieldGroups = [],
                FieldGroupSingleChoiceFields = [],
                FieldGroupFields = []
            };
            
            ctx.SingleChoiceFields.Add(scf);

            ctx.FieldGroups.Add(fg);

            ctx.Fields.Add(f);

            await ctx.SaveChangesAsync(TestCancellationToken);

            fieldGroupId = fg.Id;
            scfId = scf.Id;
            fId = f.Id;
        });
        
        FieldGroupUpdateRequest request = new FieldGroupUpdateRequest()
        {
            Name = "Location",
            SingleChoiceFieldIds = [scfId],
            FieldIds = [fId]
        };

        var response
            = await ApiClient.PutAsJsonAsync($"/api/field-groups/{fieldGroupId}", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();

        var getResponse = await ApiClient.GetAsync($"/api/field-groups/{fieldGroupId}", TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldGroupDto>(JsonOptions, TestCancellationToken);

        getContent.Should().NotBeNull();
        
        getContent.Name.Should().Be(request.Name);
        getContent.Fields.Should().NotBeEmpty()
                  .And.HaveCount(1)
                  .And.ContainSingle(f => f.Id == request.FieldIds[0]);
        getContent.SingleChoiceFields.Should().NotBeEmpty()
                  .And.HaveCount(1)
                  .And.ContainSingle(f => f.Id == request.SingleChoiceFieldIds[0]);
    }

    [Fact]
    public async Task DeleteFieldGroup_CheckExistence_Success()
    {
        long id = 1L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var fg = new FieldGroup()
            {
                Name = "Living Conditions",
                FormFieldGroups = [],
                FieldGroupSingleChoiceFields = [],
                FieldGroupFields = []
            };
            
            ctx.FieldGroups.Add(fg);

            await ctx.SaveChangesAsync(TestCancellationToken);

            id = fg.Id;
        });

        var response = await ApiClient.DeleteAsync($"/api/field-groups/{id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/field-groups/{id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
