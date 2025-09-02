using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class FieldTypeTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllFieldTypes_Existing_Success()
    {
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.FieldTypes.AddRange(new FieldType()
            {
                Name = "Digit",
                Description = null,
                Regex = "\\d",
                Fields = []
            }, new FieldType()
            {
                Name = "Yes/No",
                Description = null,
                Regex = "(?:Yes)|(?:No)",
                Fields = []
            }, new FieldType()
            {
                Name = "Digits",
                Description = null,
                Regex = "\\d+",
                Fields = []
            });
            
            await ctx.SaveChangesAsync(TestCancellationToken);
        });
        
        var response = await ApiClient.GetAsync("/api/field-types", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<FieldTypeListResponse>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Types.Should().NotBeNullOrEmpty()
               .And.HaveCount(3);
    }

    [Fact]
    public async Task CreateFieldType_CheckExistence_Success()
    {
        FieldTypeCreationRequest request = new FieldTypeCreationRequest()
        {
            Name = "Digit",
            Description = null,
            Regex = "\\d",
        };
        
        var response = await ApiClient.PostAsJsonAsync("/api/field-types", request, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/field-types/");

        var content = await response.Content.ReadFromJsonAsync<FieldTypeDto>(JsonOptions, TestCancellationToken);
        
        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldTypeDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
        
        return;

        static void ShouldBeSameProperties(FieldTypeDto response, FieldTypeCreationRequest rquest)
        {
            response.Name.Should().Be(rquest.Name);
            response.Description.Should().Be(rquest.Description);
            response.Regex.Should().Be(rquest.Regex);
        }
    }

    [Fact]
    public async Task UpdateFieldTypeById_CheckChange_Success()
    {
        long id = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var ft = new FieldType()
            {
                Name = "Digit",
                Description = null,
                Regex = "\\d",
                Fields = []
            };

            ctx.FieldTypes.Add(ft);

            await ctx.SaveChangesAsync(TestCancellationToken);

            id = ft.Id;
        });

        FieldTypeUpdateRequest request = new FieldTypeUpdateRequest()
        {
            Name = "Digigts",
            Description = null,
            Regex = "\\d+"
        };
        
        var response = await ApiClient.PutAsJsonAsync($"/api/field-types/{id}", request, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await ApiClient.GetAsync($"/api/field-types/{id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldTypeDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        getContent.Name.Should().Be(request.Name);
        getContent.Description.Should().Be(request.Description);
        getContent.Regex.Should().Be(request.Regex);
    }

    [Fact]
    public async Task DeleteFieldTypeById_CheckExistence_Success()
    {
        long id = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var ft = new FieldType()
            {
                Name = "Yes/No",
                Description = null,
                Regex = "(?:Yes)|(?:No)",
                Fields = []
            };

            ctx.FieldTypes.Add(ft);

            await ctx.SaveChangesAsync(TestCancellationToken);

            id = ft.Id;
        });
        
        var response = await ApiClient.DeleteAsync($"/api/field-types/{id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await ApiClient.GetAsync($"/api/field-types/{id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
