using System.Net;
using FormBE.Controllers;
using FormBE.Core.Logic;
using FormBE.Persistence.Model;
using FormBE.TestInt.Util;

namespace FormBE.TestInt;

public class OptionResponseTests(WebApiTestFixture webApiTestFixture) : WebApiTestBase(webApiTestFixture)
{
    [Fact]
    public async Task GetAllOptionResponses_Existing_Success()
    {
        var tele1 = "01234567890";
        var tele2 = "01234561234";
        var submit1 = TestClock.GetCurrentInstant();
        var submit2 = TestClock.GetCurrentInstant().Plus(Duration.FromHours(1));
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var scf = new SingleChoiceField()
            {
                Name = "Business Type",
                FieldGroupSingleChoiceFields = [],
                Options = []
            };
            var o1 = new Option()
            {
                Name = "GmbH",
                SingleChoiceField = scf,
                OptionFields = [],
                OptionResponses = []
            };
            var o2 = new Option()
            {
                Name = "OG",
                SingleChoiceField = scf,
                OptionFields = [],
                OptionResponses = []
            };
            
            ctx.SingleChoiceFields.Add(scf);
            
            ctx.Options.AddRange(o1, o2);

            ctx.OptionResponses.AddRange(new OptionResponse()
                                         {
                                             TelephoneNumber = tele1,
                                             Option = o1,
                                             SubmittedAt = submit1
                                         },
                                         new OptionResponse()
                                         {
                                             TelephoneNumber = tele2,
                                             Option = o2,
                                             SubmittedAt = submit2
                                         });
            
            await ctx.SaveChangesAsync(TestCancellationToken);
        });

        var response = await ApiClient.GetAsync("/api/option-responses", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<OptionResponseListResponse>(JsonOptions, TestCancellationToken);
        
        content.Should().NotBeNull();
        content.Responses.Should().NotBeEmpty()
               .And.HaveCount(2)
               .And.ContainSingle(or => or.TelephoneNumber == tele1 &&
                                        or.SubmittedAt == submit1)
               .And.ContainSingle(or => or.TelephoneNumber == tele2 &&
                                        or.SubmittedAt == submit2);
    }

    [Fact]
    public async Task CreateOptionResponse_CheckExistence_Success()
    {
        long optionId = 0L;
        
        await ModifyDatabaseContentAsync(async ctx =>
        {
            var scf = new SingleChoiceField()
            {
                Name = "Housing Conditions",
                FieldGroupSingleChoiceFields = [],
                Options = []
            };
            var o = new Option()
            {
                Name = "Homestead",
                SingleChoiceField = scf,
                OptionFields = [],
                OptionResponses = []
            };
            
            ctx.SingleChoiceFields.Add(scf);
            ctx.Options.Add(o);
            
            await ctx.SaveChangesAsync(TestCancellationToken);

            optionId = o.Id;
        });

        OptionResponseCreationRequest request = new()
        {
            OptionId = optionId,
            TelephoneNumber = "1000"
        };
        
        var response = await ApiClient.PostAsJsonAsync("/api/option-responses", request, JsonOptions, TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().StartWith("/api/option-responses/");

        var content = await response.Content.ReadFromJsonAsync<OptionResponseDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        var getResponse = await ApiClient.GetAsync(response.Headers.Location.AbsolutePath, TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadFromJsonAsync<OptionResponseDto>(JsonOptions, TestCancellationToken);
        
        getContent.Should().NotBeNull();
        ShouldBeSameProperties(content, request);
        
        return;

        static void ShouldBeSameProperties(OptionResponseDto response, OptionResponseCreationRequest request)
        {
            response.OptionId.Should().Be(request.OptionId);
            response.TelephoneNumber.Should().Be(request.TelephoneNumber);
        }
    }

    [Fact]
    public async Task DeleteOptionResponse_CheckExistence_Success()
    {
        long id = 0L;

        await ModifyDatabaseContentAsync(async ctx =>
        {
            var scf = new SingleChoiceField()
            {
                Name = "Housing Conditions",
                FieldGroupSingleChoiceFields = [],
                Options = []
            };
            var o = new Option()
            {
                Name = "Homestead",
                SingleChoiceField = scf,
                OptionFields = [],
                OptionResponses = []
            };
            var or = new OptionResponse()
            {
                TelephoneNumber = "1000",
                Option = o,
                SubmittedAt = TestClock.GetCurrentInstant()
            };

            ctx.SingleChoiceFields.Add(scf);
            ctx.Options.Add(o);
            ctx.OptionResponses.Add(or);

            await ctx.SaveChangesAsync(TestCancellationToken);

            id = or.Id;
        });
        
        var response = await ApiClient.DeleteAsync($"/api/option-responses/{id}", TestCancellationToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await ApiClient.GetAsync($"/api/option-responses/{id}", TestCancellationToken);
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
