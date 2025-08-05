using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class SingleChoiceFieldServiceTest
{
    private readonly ISingleChoiceFieldRepository _mockSingleChoiceFieldRepository;
    private readonly IFieldRepository _mockFieldRepository;
    private readonly ISingleChoiceFieldService _singleChoiceFieldService;

    public SingleChoiceFieldServiceTest()
    {
        _mockSingleChoiceFieldRepository = Substitute.For<ISingleChoiceFieldRepository>();
        _mockFieldRepository = Substitute.For<IFieldRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        ILogger<SingleChoiceFieldService> logger = Substitute.For<ILogger<SingleChoiceFieldService>>();
        _singleChoiceFieldService
            = new SingleChoiceFieldService(_mockSingleChoiceFieldRepository, _mockFieldRepository, uow, logger);
    }

    [Fact]
    public async Task GetSingleChoiceFieldsAsync_Success()
    {
        IReadOnlyCollection<SingleChoiceField> fields = Util.GetTestSingleChoiceFields();

        _mockSingleChoiceFieldRepository.GetSingleChoiceFieldsAsync(TestContext.Current.CancellationToken)
                                        .Returns(fields);

        IReadOnlyCollection<SingleChoiceField> result
            = await _singleChoiceFieldService.GetSingleChoiceFieldsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(fields.Count);
        result.Should().BeEquivalentTo(fields);
    }

    [Fact]
    public async Task GetSingleChoiceFieldByIdAsync_Success()
    {
        SingleChoiceField field = new()
        {
            Id = 0,
            Name = "Field 1",
            FieldGroupSingleChoiceFields = [],
            Options = []
        };

        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldByIdAsync(field.Id, false, TestContext.Current.CancellationToken).Returns(field);

        OneOf<SingleChoiceField, NotFound> result
            = await _singleChoiceFieldService.GetSingleChoiceFieldByIdAsync(field.Id,
                                                                            TestContext.Current.CancellationToken);

        result.Switch(field =>
                      {
                          field.Id.Should().Be(field.Id);
                          field.Name.Should().Be(field.Name);
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetSingleChoiceFieldByIdAsync_NotFound()
    {
        const long FieldId = 0;

        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldByIdAsync(FieldId, false, TestContext.Current.CancellationToken)
            .Returns((SingleChoiceField?) null);

        OneOf<SingleChoiceField, NotFound> result
            = await _singleChoiceFieldService.GetSingleChoiceFieldByIdAsync(FieldId,
                                                                            TestContext.Current.CancellationToken);

        result.Switch(field => result.Should().NotBeOfType<SingleChoiceField>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task CreateSingleChoiceFieldAsync_Success()
    {
        List<(string Name, List<long> FieldIds)> options
            = Util.GetTestOptions().Select(o => (o.Name, o.FieldIds)).ToList();
        List<Field> fields = options.SelectMany(o => o.FieldIds.Select(id => Util.GetTestFields().Find(f => f.Id == id))
                                                      .Where(f => f != null)
                                                      .OfType<Field>()
                                                      .ToList())
                                    .ToList();
        List<long> flatFieldIds = options.SelectMany(o => o.FieldIds).ToList();

        SingleChoiceField field = new()
        {
            Id = 0,
            Name = "Field 1",
            FieldGroupSingleChoiceFields = [],
            Options = []
        };

        _mockFieldRepository.GetFieldsByIdsAsync(TestContext.Current.CancellationToken,
                                                 Arg.Is<List<long>>(list => list.SequenceEqual(flatFieldIds)))
                            .Returns(fields);

        SingleChoiceField result
            = await _singleChoiceFieldService.CreateSingleChoiceFieldAsync(field.Name, options,
                                                                           TestContext.Current.CancellationToken);

        result.Name.Should().Be(field.Name);
        result.Options.Count.Should().Be(options.Count);
    }

    [Fact]
    public async Task UpdateSingleChoiceFieldAsync_Success()
    {
        const string NewName = "1.Field";
        List<(long Id, string Name, List<long> FieldIds)> passedOptions = Util.GetTestOptions();
        SingleChoiceField field = new()
        {
            Id = 0,
            Name = "Field 1",
            FieldGroupSingleChoiceFields = [],
            Options = []
        };
        List<Option> options = Util.GetTestOptions().Select(o => new Option()
        {
            SingleChoiceField = field,
            Name = o.Name,
            OptionFields = [],
            OptionResponses = []
        }).ToList();
        List<List<Field>> usedFields = Util.GetTestOptions().Select(o => o.FieldIds)
                                           .Select(list => list.Select(id => Util.GetTestFields().Find(f => f.Id == id))
                                                               .Where(f => f != null)
                                                               .OfType<Field>()
                                                               .ToList())
                                           .ToList();
        for (int i = 0; i < options.Count; i++)
        {
            options[i].OptionFields = usedFields[i].Select(f => new OptionField()
            {
                Option = options[i],
                Field = f
            }).ToList();
        }

        field.Options = options.Take(2).ToList();

        options = options.Skip(2).ToList();

        passedOptions = passedOptions.Skip(2).ToList();

        List<Field> fields = passedOptions.SelectMany(o => o.FieldIds
                                                            .Select(id => Util.GetTestFields().Find(f => f.Id == id))
                                                            .Where(f => f != null)
                                                            .OfType<Field>()
                                                            .ToList())
                                          .ToList();
        List<long> flatFieldIds = passedOptions.SelectMany(o => o.FieldIds)
                                               .ToList();

        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldByIdAsync(field.Id, true, TestContext.Current.CancellationToken).Returns(field);

        _mockSingleChoiceFieldRepository.GetOptionsByIdsAsync(TestContext.Current.CancellationToken,
                                                              Arg.Is<List<long>>(list =>
                                                                  list.SequenceEqual(options.Select(o => o.Id))))
                                        .Returns(options);
        _mockFieldRepository.GetFieldsByIdsAsync(TestContext.Current.CancellationToken,
                                                 Arg.Is<List<long>>(list => list.SequenceEqual(flatFieldIds)))
                            .Returns(fields);

        OneOf<Success, NotFound> result
            = await _singleChoiceFieldService.UpdateSingleChoiceFieldAsync(field.Id, NewName, passedOptions,
                                                                           passedOptions
                                                                               .Select(o => (o.Name, o.FieldIds))
                                                                               .ToList()
                                                                           , TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task UpdateSingleChoiceFieldAsync_NotFound()
    {
        const long FieldId = 0;
        List<(long Id, string Name, List<long> FieldIds)> passedOptions = Util.GetTestOptions();

        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldByIdAsync(FieldId, true, TestContext.Current.CancellationToken)
            .Returns((SingleChoiceField?) null);

        OneOf<Success, NotFound> result
            = await _singleChoiceFieldService.UpdateSingleChoiceFieldAsync(FieldId, "NotFound", passedOptions, [],
                                                                           TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteSingleChoiceFieldAsync_Success()
    {
        SingleChoiceField field = new()
        {
            Id = 0,
            Name = "Field 1",
            FieldGroupSingleChoiceFields = [],
            Options = []
        };

        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldByIdAsync(field.Id, true, TestContext.Current.CancellationToken).Returns(field);

        OneOf<Success, NotFound> result
            = await _singleChoiceFieldService.DeleteSingleChoiceFieldAsync(field.Id,
                                                                           TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteSingleChoiceFieldAsync_NotFound()
    {
        const long FieldId = 0;
        
        _mockSingleChoiceFieldRepository.GetSingleChoiceFieldByIdAsync(FieldId, true, TestContext.Current.CancellationToken).Returns((SingleChoiceField?) null);
        
        OneOf<Success, NotFound> result
            = await _singleChoiceFieldService.DeleteSingleChoiceFieldAsync(FieldId,
                                                                           TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
