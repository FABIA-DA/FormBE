using System.Net;
using AwesomeAssertions.Equivalency.Steps;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class FieldTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllFields_Existing_Success()
    {
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var ft = new FieldType()
            {
                Name = "Digit",
                Description = null,
                Regex = "\\d",
                Fields = []
            };

            ctx.Fields.AddRange(new Field()
                                {
                                    Name = "Number of persons in your house",
                                    Description = null,
                                    IsOptional = true,
                                    FieldType = ft,
                                    FieldGroupFields = [],
                                    FieldResponses = [],
                                    OptionFields = []
                                },
                                new Field()
                                {
                                    Name = "Number of pets in your house",
                                    Description = null,
                                    IsOptional = true,
                                    FieldType = ft,
                                    FieldGroupFields = [],
                                    FieldResponses = [],
                                    OptionFields = []
                                },
                                new Field()
                                {
                                    Name = "Number of personal computers in your house",
                                    Description = null,
                                    IsOptional = true,
                                    FieldType = ft,
                                    FieldGroupFields = [],
                                    FieldResponses = [],
                                    OptionFields = []
                                });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.GetAsync("/api/fields", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<FieldListResponse>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Fields.Should().NotBeNullOrEmpty()
               .And.HaveCount(3);
    }

    [Fact]
    public async Task CreateField_CheckExistence_Success()
    {
        long fieldTypeId = 0L;
        
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

            fieldTypeId = ft.Id;
        });

        FieldCreationRequest request = new FieldCreationRequest()
        {
            Name = "Do you live alone?",
            Description = null,
            IsOptional = false,
            FieldTypeId = fieldTypeId,
        };
        
        var response = await ApiClient.PostAsJsonAsync("/api/fields", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/fields/");

        var content = await response.Content.ReadFromJsonAsync<FieldDto>(JsonOptions, TestCancellationToken);
        
        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
        
        return;

        static void ShouldBeSameProperties(FieldDto response, FieldCreationRequest request)
        {
            response.Name.Should().Be(request.Name);
            request.Description.Should().Be(request.Description);
            request.IsOptional.Should().Be(request.IsOptional);
            request.FieldTypeId.Should().Be(request.FieldTypeId);
        }
    }

    [Fact]
    public async Task UpdateFieldById_CheckChange_Success()
    {
        long fieldId = 0L;
        long typeId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var ft1 = new FieldType()
            {
                Name = "Digit",
                Description = null,
                Regex = "\\d",
                Fields = []
            };
            var ft2 = new FieldType()
            {
                Name = "Yes/No",
                Description = null,
                Regex = "(?:Yes)|(?:No)",
                Fields = []
            };
            var f = new Field()
            {
                Name = "Would you recommend our service?",
                Description = null,
                FieldType = ft2,
                FieldGroupFields = [],
                OptionFields = [],
                FieldResponses = []
            };
            
            ctx.Fields.Add(f);
            ctx.FieldTypes.Add(ft1);
            
            await ctx.SaveChangesAsync(TestCancellationToken);

            fieldId = f.Id;
            typeId = ft1.Id;
        });

        FieldUpdateRequest request = new FieldUpdateRequest()
        {
            Name = "How would you rate our service, zero to nine?",
            Description = null,
            IsOptional = false,
            FieldTypeId = typeId,
        };
        
        var response = await ApiClient.PutAsJsonAsync($"/api/fields/{fieldId}", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await ApiClient.GetAsync($"/api/fields/{fieldId}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        getContent.Name.Should().Be(request.Name);
        getContent.Description.Should().Be(request.Description);
        getContent.IsOptional.Should().Be(request.IsOptional);
        getContent.FieldTypeId.Should().Be(request.FieldTypeId);
    }

    [Fact]
    public async Task DeleteFieldById_CheckExistence_Success()
    {
        long fieldId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {            
            var ft1 = new FieldType()
            {
                Name = "Yes/No",
                Description = null,
                Regex = "(?:Yes)|(?:No)",
                Fields = []
            };
            var f = new Field()
            {
                Name = "Would you recommend our service?",
                Description = null,
                FieldType = ft1,
                FieldGroupFields = [],
                OptionFields = [],
                FieldResponses = []
            };
            
            ctx.Fields.Add(f);
            
            await ctx.SaveChangesAsync(TestCancellationToken);

            fieldId = f.Id;
        });
        
        var response = await ApiClient.DeleteAsync($"/api/fields/{fieldId}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await ApiClient.GetAsync($"/api/fields/{fieldId}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
