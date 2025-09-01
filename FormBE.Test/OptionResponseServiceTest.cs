using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class OptionResponseServiceTest
{
    private readonly IOptionResponseRepository _mockOptionResponseRepository;
    private readonly IClock _mockClock;
    private readonly IOptionResponseService _optionResponseService;

    public OptionResponseServiceTest()
    {
        _mockOptionResponseRepository = Substitute.For<IOptionResponseRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        uow.OptionResponseRepository.Returns(_mockOptionResponseRepository);
        _mockClock = Substitute.For<IClock>();
        ILogger<OptionResponseService> logger = Substitute.For<ILogger<OptionResponseService>>();
        _optionResponseService = new OptionResponseService(uow, _mockClock, logger);
    }

    [Fact]
    public async Task GetOptionResponsesAsync_Success()
    {
        IReadOnlyCollection<OptionResponse> responses = Util.GetTestOptionResponses();

        _mockOptionResponseRepository.GetOptionResponsesAsync(TestContext.Current.CancellationToken).Returns(responses);

        IReadOnlyCollection<OptionResponse> result
            = await _optionResponseService.GetOptionResponsesAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(responses.Count);
        result.Should().BeEquivalentTo(responses);
    }

    [Fact]
    public async Task GetOptionResponseByIdAsync_Success()
    {
        OptionResponse response = new()
        {
            Id = 0,
            TelephoneNumber = "0000",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Option = new()
            {
                Id = 0,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        };

        _mockOptionResponseRepository
            .GetOptionResponseByIdAsync(response.Id, false, TestContext.Current.CancellationToken).Returns(response);

        OneOf<OptionResponse, NotFound> result
            = await _optionResponseService.GetOptionResponseByIdAsync(response.Id,
                                                                      TestContext.Current.CancellationToken);

        result.Switch(optionResponse =>
                      {
                          optionResponse.Id.Should().Be(response.Id);
                          optionResponse.TelephoneNumber.Should().Be(response.TelephoneNumber);
                          optionResponse.SubmittedAt.Should().Be(response.SubmittedAt);
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetOptionResponseByIdAsync_NotFound()
    {
        const long ResponseId = 0;

        _mockOptionResponseRepository
            .GetOptionResponseByIdAsync(ResponseId, false, TestContext.Current.CancellationToken)
            .Returns((OptionResponse?) null);

        OneOf<OptionResponse, NotFound> result
            = await _optionResponseService.GetOptionResponseByIdAsync(ResponseId,
                                                                      TestContext.Current.CancellationToken);

        result.Switch(optionResponse => result.Should().NotBeOfType<OptionResponse>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task CreateOptionResponseAsync_Success()
    {
        const string Tele = "0000";
        Option option = new()
        {
            Id = 0,
            Name = "Option 1",
            SingleChoiceField = new()
            {
                Id = 0,
                Name = "Single Choice Field 1",
                FieldGroupSingleChoiceFields = [],
                Options = []
            },
            OptionFields = [],
            OptionResponses = []
        };
        Instant submittedAt = Instant.FromUtc(2025, 8, 4, 0, 0);

        _mockOptionResponseRepository.GetOptionByIdAsync(option.Id, true, TestContext.Current.CancellationToken)
                                     .Returns(option);
        _mockClock.GetCurrentInstant().Returns(submittedAt);

        OneOf<Success<OptionResponse>, IOptionResponseService.OptionNotFound> result
            = await _optionResponseService.CreateOptionResponseAsync(option.Id, Tele,
                                                                     TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          success.Value.OptionId.Should().Be(option.Id);
                          success.Value.TelephoneNumber.Should().Be(Tele);
                          success.Value.SubmittedAt.Should().Be(submittedAt);
                      },
                      optionNotFound =>
                          result.Should().NotBeOfType<IOptionResponseService.OptionNotFound>("should be found"));
    }

    [Fact]
    public async Task CreateOptionResponseAsync_OptionNotFound()
    {
        const string Tele = "0000";
        const long OptionId = 0;

        _mockOptionResponseRepository.GetOptionByIdAsync(OptionId, true, TestContext.Current.CancellationToken)
                                     .Returns((Option?) null);

        OneOf<Success<OptionResponse>, IOptionResponseService.OptionNotFound> result
            = await _optionResponseService.CreateOptionResponseAsync(OptionId, Tele,
                                                                     TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<OptionResponse>>("should not be found"),
                      optionNotFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteOptionResponseAsync_Success()
    {
        OptionResponse testResponse = new()
        {
            Id = 0,
            TelephoneNumber = "0000",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Option = new()
            {
                Id = 0,
                Name = "Option 1",
                SingleChoiceField = new()
                {
                    Id = 0,
                    Name = "Single Choice Field 1",
                    FieldGroupSingleChoiceFields = [],
                    Options = []
                },
                OptionFields = [],
                OptionResponses = []
            }
        };

        _mockOptionResponseRepository
            .GetOptionResponseByIdAsync(testResponse.Id, true, TestContext.Current.CancellationToken)
            .Returns(testResponse);

        OneOf<Success, NotFound> result
            = await _optionResponseService.DeleteOptionResponseAsync(testResponse.Id,
                                                                     TestContext.Current.CancellationToken);

        result.Switch(success =>
        {
            // expected
        }, notFound => result.Should().BeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteOptionResponseAsync_NotFound()
    {
        const long ResponseId = 0;

        _mockOptionResponseRepository
            .GetOptionResponseByIdAsync(ResponseId, true, TestContext.Current.CancellationToken)
            .Returns((OptionResponse?) null);

        OneOf<Success, NotFound> result
            = await _optionResponseService.DeleteOptionResponseAsync(ResponseId,
                                                                     TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
