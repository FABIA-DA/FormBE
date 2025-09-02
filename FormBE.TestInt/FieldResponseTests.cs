using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public sealed class FieldResponseTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllFieldResponses_Existing_Success()
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
            var f = new Field()
            {
                Name = "How many cats do you have?",
                Description = null,
                IsOptional = false,
                FieldType = ft,
                FieldGroupFields = [],
                OptionFields = [],
                FieldResponses = []
            };

            ctx.Fields.Add(f);

            ctx.FieldResponses.AddRange(new FieldResponse()
            {
                TelephoneNumber = "1000",
                Value = "1",
                Field = f,
                SubmittedAt = TestClock.GetCurrentInstant()
            }, new FieldResponse()
            {
                TelephoneNumber = "1001",
                Value = "3",
                Field = f,
                SubmittedAt = TestClock.GetCurrentInstant().Minus(Duration.FromHours(1))
            }, new FieldResponse()
            {
                TelephoneNumber = "1002",
                Value = "6",
                Field = f,
                SubmittedAt = TestClock.GetCurrentInstant().Plus(Duration.FromDays(2))
            });
            
            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.GetAsync("/api/field-responses", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content
            = await response.Content.ReadFromJsonAsync<FieldResponseListResponse>(JsonOptions, TestCancellationToken);
        
        content.Should().NotBeNull();
        content.Responses.Should().NotBeNullOrEmpty()
               .And.HaveCount(3);
    }

    [Fact]
    public async Task CreateFieldResponse_checkExistence_Success()
    {
        long fieldId = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var ft = new FieldType()
            {
                Name = "Digit",
                Description = null,
                Regex = "\\d",
                Fields = []
            };
            var f = new Field()
            {
                Name = "How many cats do you have?",
                Description = null,
                IsOptional = false,
                FieldType = ft,
                FieldGroupFields = [],
                OptionFields = [],
                FieldResponses = []
            };

            ctx.Fields.Add(f);

            await ctx.SaveChangesAsync(TestCancellationToken);

            fieldId = f.Id;
        });

        FieldResponseCreationRequest request = new FieldResponseCreationRequest()
        {
            TelephoneNumber = "1000",
            Value = "1",
            FieldId = fieldId
        };
        
        var response = await ApiClient.PostAsJsonAsync("/api/field-responses", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/field-responses/");

        var content = await response.Content.ReadFromJsonAsync<FieldResponseDto>(JsonOptions, TestCancellationToken);
        
        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadFromJsonAsync<FieldResponseDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        ShouldBeSameProperties(getContent, request);
        
        return;

        static void ShouldBeSameProperties(FieldResponseDto response, FieldResponseCreationRequest request)
        {
            response.TelephoneNumber.Should().Be(request.TelephoneNumber);
            response.Value.Should().Be(request.Value);
            response.FieldId.Should().Be(request.FieldId);
        }
    }

    [Fact]
    public async Task DeleteFieldResponseById_CheckExistence_Success()
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
            var f = new Field()
            {
                Name = "How many cats do you have?",
                Description = null,
                IsOptional = false,
                FieldType = ft,
                FieldGroupFields = [],
                OptionFields = [],
                FieldResponses = []
            };
            var fr = new FieldResponse()
            {
                TelephoneNumber = "1000",
                Value = "1",
                Field = f,
                SubmittedAt = TestClock.GetCurrentInstant()
            };

            ctx.FieldResponses.Add(fr);
            
            await  ctx.SaveChangesAsync(TestCancellationToken);

            id = fr.Id;
        });
        
        var response = await ApiClient.DeleteAsync($"/api/field-responses/{id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await ApiClient.GetAsync($"/api/field-responses/{id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
