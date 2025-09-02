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

        var response = await ApiClient.GetAsync("/api/single-choice-fields", TestCancellationToken);

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
        long fieldId1 = 0L;
        long fieldId2 = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var field1 = new Field()
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
            };
            var field2 = new Field()
            {
                Name = "Is the company in business",
                Description = null,
                IsOptional = false,
                FieldType = new FieldType()
                {
                    Name = "Yes/No",
                    Description = null,
                    Regex = "(?:Yes)|(?:No)",
                    Fields = []
                },
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            };
            
            ctx.Fields.AddRange(field1, field2);

            await ctx.SaveChangesAsync(TestCancellationToken);

            fieldId1 = field1.Id;
            fieldId2 = field2.Id;
        });
        
        SingleChoiceFieldCreationRequest request = new()
        {
            Name = "Business Type",
            Options =
            [
                new()
                {
                    Name = "AG",
                    FieldIds = [fieldId1]
                },
                new NewOptions()
                {
                    Name = "GmbH",
                    FieldIds = [fieldId2]
                }
            ]
        };

        var response
            = await ApiClient.PostAsJsonAsync("/api/single-choice-fields", request, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/single-choice-fields/");

        var content
            = await response.Content.ReadFromJsonAsync<SingleChoiceFieldDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);

        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent
            = await getResponse.Content.ReadFromJsonAsync<SingleChoiceFieldDto>(JsonOptions, TestCancellationToken);

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

    [Fact]
    public async Task UpdateSingleChoiceField_CheckChange_Success()
    {
        long scfId = 0L;
        long fieldId1 = 0L;
        long fieldId2 = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var digitsType = new FieldType { Name = "Digits", Description = null, Regex = "\\d+", Fields = [] };
            var yesNoType = new FieldType
                { Name = "Yes/No", Description = null, Regex = "(?:Yes)|(?:No)", Fields = [] };

            var f1 = new Field
            {
                Name = "Number of People",
                Description = null,
                IsOptional = false,
                FieldType = digitsType,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            };
            var f2 = new Field
            {
                Name = "Is the environment well-maintained?",
                Description = null,
                IsOptional = false,
                FieldType = yesNoType,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = []
            };

            var scf = new SingleChoiceField
            {
                Name = "Business Type",
                FieldGroupSingleChoiceFields = [],
                Options = []
            };

            var existingOption = new Option
            {
                Name = "Bungalow",
                SingleChoiceField = scf,
                OptionFields = [],
                OptionResponses = []
            };

            ctx.Fields.AddRange(f1, f2);
            ctx.SingleChoiceFields.Add(scf);
            ctx.Options.Add(existingOption);

            await ctx.SaveChangesAsync(TestCancellationToken);

            scfId = scf.Id;
            fieldId1 = f1.Id;
            fieldId2 = f2.Id;
        });

        var request = new SingleChoiceFieldUpdateRequest
        {
            Name = "Housing Conditions",
            OldOptions = [new() { Id = 1L, Name = "Homestead", FieldIds = [fieldId1] }],
            NewOptions = [new NewOptions() { Name = "Flat", FieldIds = [fieldId2] }]
        };

        var response
            = await ApiClient.PutAsJsonAsync($"/api/single-choice-fields/{scfId}", request, JsonOptions,
                                             TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.Should().BeEmpty();

        var getResponse = await ApiClient.GetAsync($"/api/single-choice-fields/{scfId}", TestCancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content
            = await getResponse.Content.ReadFromJsonAsync<SingleChoiceFieldDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Name.Should().Be(request.Name);
        content.Options.Should().NotBeNullOrEmpty()
               .And.ContainSingle(o =>
                                      o.Name == "Homestead" &&
                                      o.Fields[0].Id == fieldId1)
               .And.ContainSingle(o =>
                                      o.Name == "Flat" &&
                                      o.Fields[0].Id == fieldId2);
    }

    [Fact]
    public async Task DeleteSingleChoiceField_CheckExistence_Success()
    {
        long scfId = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var scf = new SingleChoiceField { Name = "Business Type", FieldGroupSingleChoiceFields = [], Options = [] };

            ctx.SingleChoiceFields.Add(scf);

            await ctx.SaveChangesAsync(TestCancellationToken);

            scfId = scf.Id;
        });
        
        var response = await ApiClient.DeleteAsync($"/api/single-choice-fields/{scfId}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await ApiClient.GetAsync($"/api/single-choice-fields/{scfId}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
