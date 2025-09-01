using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class FieldResponseServiceTest
{
    private readonly IFieldResponseRepository _mockFieldResponseRepository;
    private readonly IFieldRepository _mockFieldRepository;
    private readonly IClock _mockClock;
    private readonly FieldResponseService _fieldResponseService;

    public FieldResponseServiceTest()
    {
        _mockFieldResponseRepository = Substitute.For<IFieldResponseRepository>();
        _mockFieldRepository = Substitute.For<IFieldRepository>();
        _mockClock = Substitute.For<IClock>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        uow.FieldResponseRepository.Returns(_mockFieldResponseRepository);
        uow.FieldRepository.Returns(_mockFieldRepository);
        ILogger<FieldResponseService> logger = Substitute.For<ILogger<FieldResponseService>>();
        _fieldResponseService
            = new FieldResponseService(uow, _mockClock, logger);
    }

    [Fact]
    public async Task GetFieldResponsesAsync_Success()
    {
        List<FieldResponse> fieldResponses = Util.GetTestFieldResponses();

        _mockFieldResponseRepository.GetAllFieldResponses(TestContext.Current.CancellationToken)
                                    .Returns(fieldResponses);

        IReadOnlyCollection<FieldResponse> result
            = await _fieldResponseService.GetAllFieldResponsesAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(fieldResponses.Count);
        result.Should().BeEquivalentTo(fieldResponses);
    }

    [Fact]
    public async Task GetFieldResponseByIdAsync_Success()
    {
        FieldResponse testResponse = new()
        {
            Id = 0L,
            TelephoneNumber = "0000",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0L,
                Name = "Input",
                Description = null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0L,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        };

        _mockFieldResponseRepository
            .GetFieldResponseByIdAsync(testResponse.Id, false, TestContext.Current.CancellationToken)
            .Returns(testResponse);

        OneOf<FieldResponse, NotFound> result
            = await _fieldResponseService.GetFieldResponseByIdAsync(testResponse.Id,
                                                                    TestContext.Current.CancellationToken);

        result.Switch(fieldResponse => fieldResponse.Should().BeEquivalentTo(testResponse),
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetFieldResponseByIdAsync_NotFound()
    {
        const long FieldResponseId = 0L;

        _mockFieldResponseRepository
            .GetFieldResponseByIdAsync(FieldResponseId, false, TestContext.Current.CancellationToken)
            .Returns((FieldResponse?) null);

        OneOf<FieldResponse, NotFound> result
            = await _fieldResponseService.GetFieldResponseByIdAsync(FieldResponseId,
                                                                    TestContext.Current.CancellationToken);

        result.Switch(fieldResponse => result.Should().NotBeOfType<FieldResponse>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task CreateFieldResponseAsync_Success()
    {
        const string Tele = "0000";
        const string Value = "test";
        Instant submittedAt = Instant.FromUtc(2025, 8, 4, 0, 0);

        Field field = new()
        {
            Id = 0L,
            Name = "Test",
            Description = null,
            IsOptional = false,
            FieldGroupFields = [],
            FieldResponses = [],
            OptionFields = [],
            FieldType = new()
            {
                Id = 0L,
                Name = "Test",
                Description = null,
                Regex = "\\w+",
                Fields = []
            }
        };

        _mockFieldRepository.GetFieldByIdAsync(field.Id, true, TestContext.Current.CancellationToken).Returns(field);
        _mockClock.GetCurrentInstant().Returns(submittedAt);

        OneOf<Success<FieldResponse>, IFieldResponseService.FieldNotFound> result
            = await _fieldResponseService.CreateFieldResponseAsync(field.Id, Tele, Value,
                                                                   TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          success.Value.TelephoneNumber.Should().Be(Tele);
                          success.Value.Value.Should().Be(Value);
                          success.Value.SubmittedAt.Should().Be(submittedAt);
                      },
                      fieldNotFound =>
                          result.Should().NotBeOfType<IFieldResponseService.FieldNotFound>("field should be found"));
    }

    [Fact]
    public async Task CreateFieldResponseAsync_FieldNotFound()
    {
        const string Tele = "0000";
        const string Value = "test";
        const long FieldId = 0;

        _mockFieldRepository.GetFieldByIdAsync(FieldId, true, TestContext.Current.CancellationToken)
                            .Returns((Field?) null);

        OneOf<Success<FieldResponse>, IFieldResponseService.FieldNotFound> result
            = await _fieldResponseService.CreateFieldResponseAsync(FieldId, Tele, Value,
                                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success<FieldResponse>>("field should not be found"),
                      fieldNotFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteFieldResponseAsync_Success()
    {
        FieldResponse testResponse = new()
        {
            Id = 0L,
            TelephoneNumber = "0000",
            Value = "test",
            SubmittedAt = Instant.FromUtc(2025, 8, 4, 0, 0),
            Field = new()
            {
                Id = 0L,
                Name = "Input",
                Description = null,
                IsOptional = false,
                FieldGroupFields = [],
                FieldResponses = [],
                OptionFields = [],
                FieldType = new()
                {
                    Id = 0L,
                    Name = "Text",
                    Description = null,
                    Regex = "\\w+",
                    Fields = []
                }
            }
        };

        _mockFieldResponseRepository
            .GetFieldResponseByIdAsync(testResponse.Id, true, TestContext.Current.CancellationToken)
            .Returns(testResponse);

        OneOf<Success, NotFound> result
            = await _fieldResponseService.DeleteFieldResponseAsync(testResponse.Id,
                                                                   TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteFieldResponseAsync_NotFound()
    {
        const long ResponseId = 0L;
        
        _mockFieldResponseRepository.GetFieldResponseByIdAsync(ResponseId, true,  TestContext.Current.CancellationToken).Returns((FieldResponse?) null);
        
        OneOf<Success, NotFound> result
            = await _fieldResponseService.DeleteFieldResponseAsync(ResponseId,
                                                                   TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
