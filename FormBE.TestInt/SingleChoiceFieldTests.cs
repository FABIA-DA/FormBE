using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public class SingleChoiceFieldTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllSingleChoiceFields_Existing_Success()
    {
        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.SingleChoiceFields.AddRange(new SingleChoiceField()
                                            {
                                                Name = "Gender",
                                                FieldGroupSingleChoiceFields = [],
                                                Options = []
                                            },
                                            new SingleChoiceField()
                                            {
                                                Name = "Housing Condition",
                                                FieldGroupSingleChoiceFields = [],
                                                Options = []
                                            },
                                            new SingleChoiceField()
                                            {
                                                Name = "Business Type",
                                                FieldGroupSingleChoiceFields = [],
                                                Options = []
                                            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.GetAsync("/api/singleChoiceFields", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content
            = await response.Content.ReadFromJsonAsync<SingleChoiceFieldListResponse>(JsonOptions,
             TestCancellationToken);
        
        content.Should().NotBeNull();
        content.Fields.Should().NotBeEmpty()
               .And.HaveCount(3);
    }

    [Fact]
    public async Task CreateSingleChoiceField_CheckExistence_Success()
    {
        const long Id = 1L;
        SingleChoiceFieldCreationRequest request = new()
        {
            Name = "Business Type",
            Options = new Dictionary<string, List<long>>()
            {
                ["AG"] = [1L],
                ["GmbH"] = [2L]
            } 
        };

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Fields.AddRange(new Field()
            {
                Name = "Number of Employees",
                Description = null,
                IsOptional = false,
                FieldType = new FieldType()
                {
                    Name = "Digits",
                    Description = null,
                    Regex = "\\d+",
                    Fields = []
                },
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            }, new Field()
            {
                Name = "Is the company in business",
                Description = null,
                IsOptional = false,
                FieldType = new FieldType()
                {
                    Name = "Digits",
                    Description = null,
                    Regex = "(?:Yes)|(?:No)",
                    Fields = []
                },
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            });

            await ctx.SaveChangesAsync(TestCancellationToken);
        });
        
        var response = await ApiClient.PostAsJsonAsync("/api/singleChoiceFields", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().Be($"/api/singleChoiceFields/{Id}");
        
        var content = await response.Content.ReadFromJsonAsync<SingleChoiceFieldDto>(JsonOptions, TestCancellationToken);
        
        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);

        var getResponse = await ApiClient.GetAsync($"/api/singleChoiceFields/{Id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadFromJsonAsync<SingleChoiceFieldDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
        
        return;

        static void ShouldBeSameProperties(SingleChoiceFieldDto response, SingleChoiceFieldCreationRequest request)
        {
            response.Name.Should().Be(request.Name);
            response.Options.Should().NotBeNull()
                    .And.NotBeEmpty()
                    .And.HaveCount(request.Options.Count);
        }
    }
}
