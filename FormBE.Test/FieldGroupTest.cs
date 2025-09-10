using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using NSubstitute;
using OneOf.Types;
using OneOf;

namespace FormBE.Test;

public class FieldGroupTest
{
    private readonly IFieldGroupRepository _mockFieldGroupRepository;
    private readonly ISingleChoiceFieldRepository _mockSingleChoiceFieldRepository;
    private readonly IFieldRepository _mockFieldRepository;
    private readonly FieldGroupService _fieldGroupService;

    public FieldGroupTest()
    {
        _mockFieldGroupRepository = Substitute.For<IFieldGroupRepository>();
        _mockFieldRepository = Substitute.For<IFieldRepository>();
        _mockSingleChoiceFieldRepository = Substitute.For<ISingleChoiceFieldRepository>();
        IUnitOfWork uow = Substitute.For<IUnitOfWork>();
        uow.FieldGroupRepository.Returns(_mockFieldGroupRepository);
        uow.SingleChoiceFieldRepository.Returns(_mockSingleChoiceFieldRepository);
        uow.FieldRepository.Returns(_mockFieldRepository);
        ILogger<FieldGroupService> logger = Substitute.For<ILogger<FieldGroupService>>();

        _fieldGroupService = new FieldGroupService(uow, logger);
    }

    [Fact]
    public async Task GetFieldGroupsAsync_Success()
    {
        IReadOnlyCollection<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> testFieldGroups
            = Util.GetTestFieldGroups();

        _mockFieldGroupRepository.GetFieldGroupsAsync(TestContext.Current.CancellationToken).Returns(testFieldGroups);

        IReadOnlyCollection<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> result
            = await _fieldGroupService.GetFieldGroupsAsync(TestContext.Current.CancellationToken);

        result.Count.Should().Be(testFieldGroups.Count, "should have same count");
        result.Should().BeEquivalentTo(testFieldGroups, "should be same group");
    }

    [Fact]
    public async Task GetFieldGroupByIdAsync_Success()
    {
        FieldGroup testFieldGroup = new()
        {
            Id = 0,
            Name = "Test",
            FormFieldGroups = [],
            FieldGroupFields = [],
            FieldGroupSingleChoiceFields = []
        };

        _mockFieldGroupRepository
            .GetFieldGroupByIdAsync(testFieldGroup.Id, false, TestContext.Current.CancellationToken)
            .Returns(testFieldGroup);

        OneOf<FieldGroup, NotFound> result
            = await _fieldGroupService.GetFieldGroupByIdAsync(testFieldGroup.Id, TestContext.Current.CancellationToken);

        result.Switch(fieldGroup => { fieldGroup.Should().BeEquivalentTo(testFieldGroup, "should be same group"); },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task GetFieldGroupByIdAsync_NotFound()
    {
        const long FieldGroupId = 0;

        _mockFieldGroupRepository.GetFieldGroupByIdAsync(FieldGroupId, false, TestContext.Current.CancellationToken)
                                 .Returns((FieldGroup?) null);

        OneOf<FieldGroup, NotFound> result
            = await _fieldGroupService.GetFieldGroupByIdAsync(FieldGroupId, TestContext.Current.CancellationToken);

        result.Switch(fieldGroup => result.Should().NotBeOfType<FieldGroup>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task CreateFieldGroupAsync_Success()
    {
        List<(long Id, string Name, int OptionCount)> singleChoiceFields = Util.GetTestSingleChoiceFields();
        List<SingleChoiceField> testSingleChoiceFields = singleChoiceFields.GetSingleChoiceFields();
        List<Field> fields = Util.GetTestFields();
        List<long> singleChoiceFieldIds = singleChoiceFields.GetIds();
        List<long> fieldIds = fields.GetIds();

        FieldGroup testFieldGroup = new()
        {
            Id = 0,
            Name = "Test",
            FormFieldGroups = [],
            FieldGroupFields = [],
            FieldGroupSingleChoiceFields = []
        };

        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldsByIdsAsync(TestContext.Current.CancellationToken, singleChoiceFieldIds)
            .Returns(testSingleChoiceFields);
        _mockFieldRepository.GetFieldsByIdsAsync(TestContext.Current.CancellationToken, fieldIds).Returns(fields);

        FieldGroup result = await _fieldGroupService.CreateFieldGroupAsync(testFieldGroup.Name, singleChoiceFieldIds,
                                                                           fieldIds,
                                                                           TestContext.Current.CancellationToken);

        result.Name.Should().Be(testFieldGroup.Name);
        result.FieldGroupSingleChoiceFields.Count.Should().Be(singleChoiceFieldIds.Count);
        result.FieldGroupFields.Count.Should().Be(fieldIds.Count);
    }

    [Fact]
    public async Task UpdateFieldGroupAsync_Success()
    {
        List<(long Id, string Name, int OptionCount)> singleChoiceFields = Util.GetTestSingleChoiceFields();
        List<SingleChoiceField> testSingleChoiceFields = singleChoiceFields.GetSingleChoiceFields();
        List<Field> fields = Util.GetTestFields();
        List<long> singleChoiceFieldIds = singleChoiceFields.GetIds();
        List<long> fieldIds = fields.GetIds();

        FieldGroup testFieldGroup = new()
        {
            Id = 0,
            Name = "Test",
            FormFieldGroups = [],
            FieldGroupFields = [],
            FieldGroupSingleChoiceFields = []
        };

        testFieldGroup.FieldGroupSingleChoiceFields = testSingleChoiceFields.Take(2)
                                                                        .Select(f => new FieldGroupSingleChoiceField()
                                                                        {
                                                                            FieldGroup = testFieldGroup,
                                                                            SingleChoiceField = f
                                                                        }).ToList();
        singleChoiceFieldIds = singleChoiceFieldIds.Skip(2).ToList();

        testFieldGroup.FieldGroupFields = fields.Take(2).Select(f => new FieldGroupField()
        {
            FieldGroup = testFieldGroup,
            Field = f
        }).ToList();
        fieldIds = fieldIds.Skip(2).ToList();

        _mockFieldGroupRepository.GetFieldGroupByIdAsync(testFieldGroup.Id, true, TestContext.Current.CancellationToken)
                                 .Returns(testFieldGroup);
        _mockSingleChoiceFieldRepository
            .GetSingleChoiceFieldsByIdsAsync(TestContext.Current.CancellationToken,
                                             Arg.Is<List<long>>(list => list.SequenceEqual(singleChoiceFieldIds)))
            .Returns(testSingleChoiceFields);
        _mockFieldRepository.GetFieldsByIdsAsync(TestContext.Current.CancellationToken,
                                                 Arg.Is<List<long>>(list => list.SequenceEqual(fieldIds)))
                            .Returns(fields);

        OneOf<Success, NotFound> result
            = await _fieldGroupService.UpdateFieldGroupAsync(testFieldGroup.Id, "New Test Name", singleChoiceFieldIds,
                                                             fieldIds, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task UpdateFieldGroupAsync_NotFound()
    {
        const long TestFieldGroupId = 0;

        _mockFieldGroupRepository.GetFieldGroupByIdAsync(TestFieldGroupId, true, TestContext.Current.CancellationToken)
                                 .Returns((FieldGroup?) null);

        OneOf<Success, NotFound> result
            = await _fieldGroupService.UpdateFieldGroupAsync(TestFieldGroupId, "New Test Name", [],
                                                             [], TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }

    [Fact]
    public async Task DeleteFieldGroupAsync_Success()
    {
        FieldGroup testFieldGroup = new()
        {
            Id = 0,
            Name = "Test",
            FormFieldGroups = [],
            FieldGroupFields = [],
            FieldGroupSingleChoiceFields = []
        };

        _mockFieldGroupRepository.GetFieldGroupByIdAsync(testFieldGroup.Id, true, TestContext.Current.CancellationToken)
                                 .Returns(testFieldGroup);

        OneOf<Success, NotFound> result
            = await _fieldGroupService.DeleteFieldGroupAsync(testFieldGroup.Id, TestContext.Current.CancellationToken);

        result.Switch(success =>
                      {
                          // expected
                      },
                      notFound => result.Should().NotBeOfType<NotFound>("should be found"));
    }

    [Fact]
    public async Task DeleteFieldGroupAsync_NotFound()
    {
        const long TestFieldGroupId = 0;

        _mockFieldGroupRepository.GetFieldGroupByIdAsync(TestFieldGroupId, true, TestContext.Current.CancellationToken)
                                 .Returns((FieldGroup?) null);

        OneOf<Success, NotFound> result
            = await _fieldGroupService.DeleteFieldGroupAsync(TestFieldGroupId, TestContext.Current.CancellationToken);

        result.Switch(success => result.Should().NotBeOfType<Success>("should not be found"),
                      notFound =>
                      {
                          // expected
                      });
    }
}
