using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class FieldGroupTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    private static void ShouldBeSameProperties(FieldGroupDto response, FieldGroupRequest request)
    {
        response.Name.Should().Be(request.Name);
        response.Fields.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.ContainSingle(f => f.Id == request.FieldIds[0]);
        response.SingleChoiceFields.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.ContainSingle(f => f.Id == request.SingleChoiceFieldIds[0]);
    }
    
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

        var response = await ApiClient.GetAsync("/api/fieldGroups", TestCancellationToken);

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
        const long Id = 1L;
        FieldGroupRequest request = new FieldGroupRequest()
        {
            Name = "Location",
            SingleChoiceFieldIds = [1L],
            FieldIds = [1L]
        };

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.SingleChoiceFields.Add(new SingleChoiceField()
            {
                Name = "Living",
                FieldGroupSingleChoiceFields = [],
                Options = [],
            });

            ctx.Fields.Add(new Field()
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
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.PostAsJsonAsync("/api/fieldGroups", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().Be($"/api/fieldGroups/{Id}");

        var content = await response.Content.ReadFromJsonAsync<FieldGroupDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);

        var getResponse = await ApiClient.GetAsync($"/api/fieldGroups/{Id}", TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldGroupDto>(JsonOptions, TestCancellationToken);

        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
    }

    [Fact]
    public async Task UpdateFieldGroup_CheckChange_Success()
    {
        const long Id = 1L;
        FieldGroupRequest request = new FieldGroupRequest()
        {
            Name = "Location",
            SingleChoiceFieldIds = [1L],
            FieldIds = [1L]
        };

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.SingleChoiceFields.Add(new SingleChoiceField()
            {
                Name = "Living",
                FieldGroupSingleChoiceFields = [],
                Options = [],
            });

            ctx.FieldGroups.Add(new FieldGroup()
            {
                Name = "Postal Code",
                FormFieldGroups = [],
                FieldGroupSingleChoiceFields = [],
                FieldGroupFields = []
            });

            ctx.Fields.Add(new Field()
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
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response
            = await ApiClient.PutAsJsonAsync($"/api/fieldGroups/{Id}", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();

        var getResponse = await ApiClient.GetAsync($"/api/fieldGroups/{Id}", TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldGroupDto>(JsonOptions, TestCancellationToken);

        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
    }

    [Fact]
    public async Task DeleteFieldGroup_CheckExistence_Success()
    {
        const long Id = 1L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.FieldGroups.Add(new FieldGroup()
            {
                Name = "Living Conditions",
                FormFieldGroups = [],
                FieldGroupSingleChoiceFields = [],
                FieldGroupFields = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.DeleteAsync($"/api/fieldGroups/{Id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();
        
        var getResponse = await ApiClient.GetAsync($"/api/fieldGroups/{Id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
