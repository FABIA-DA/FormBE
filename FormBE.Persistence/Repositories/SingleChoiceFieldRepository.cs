using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface ISingleChoiceFieldRepository
{
    /// <summary>
    /// Get a single choice field by its id.
    /// </summary>
    /// <param name="singleChoiceFieldId">The id of the single choice field.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The single choice field to get or null if not found.</returns>
    public ValueTask<SingleChoiceField?> GetSingleChoiceFieldByIdAsync(long singleChoiceFieldId, bool tracking = true,
                                                                       CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all single choice fields without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All single choice fields without tracking.</returns>
    public ValueTask<IReadOnlyCollection<(long Id, string Name, int OptionCount)>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a subset of existing single choice fields with tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="singleChoiceFieldIds">The ids of the single choice fields to get.</param>
    /// <returns>Gets the requested single choice fields with tracking.</returns>
    public ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsByIdsAsync(
        CancellationToken cancellationToken = default, params List<long> singleChoiceFieldIds);

    /// <summary>
    /// Add a single choice field to the tracking of EF Core.
    /// </summary>
    /// <param name="field">The single choice field to add.</param>
    public void AddSingleChoiceField(SingleChoiceField field);

    /// <summary>
    /// Add a single choice field to the tracking with a <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="field">The single choice field to delete.</param>
    public void RemoveSingleChoiceField(SingleChoiceField field);

    /// <summary>
    /// Get <see cref="Option"/>s by their ids with tracking while invalid optionIds are ignored.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="optionIds">The ids of the options to get.</param>
    /// <returns>A <see cref="IReadOnlyCollection{Option}"/> of <see cref="Option"/>s.</returns>
    public ValueTask<IReadOnlyCollection<Option>> GetOptionsByIdsAsync(CancellationToken cancellationToken = default,
                                                                       params List<long> optionIds);

    /// <summary>
    /// Begins tracking for multiple <see cref="Option"/>.
    /// </summary>
    /// <param name="newOptions">The options to be added.</param>
    public void AddOptions(params IEnumerable<Option> newOptions);

    /// <summary>
    /// Begins to track for multiple <see cref="Option"/> with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="oldOptions">The options to delete.</param>
    public void RemoveOptions(params IEnumerable<Option> oldOptions);

    /// <summary>
    /// Begins tracking for multiple <see cref="OptionField"/>.
    /// </summary>
    /// <param name="fields">The items to add.</param>
    public void AddOptionFields(params IEnumerable<OptionField> fields);

    /// <summary>
    /// Begins tracking for multiple <see cref="OptionField"/> with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fields">The items to delete.</param>
    public void RemoveOptionFields(params IEnumerable<OptionField> fields);
}

internal class SingleChoiceFieldRepository(
    DbSet<SingleChoiceField> singleChoiceFields,
    DbSet<Option> options,
    DbSet<OptionField> optionFields) : ISingleChoiceFieldRepository
{
    private IQueryable<SingleChoiceField> SingleChoiceFields => singleChoiceFields;
    private IQueryable<SingleChoiceField> NoTracking => SingleChoiceFields.AsNoTracking();

    private static IQueryable<SingleChoiceField> FullInclude(IQueryable<SingleChoiceField> self) =>
        self.Include(f => f.Options)
            .ThenInclude(o => o.OptionFields)
            .ThenInclude(o => o.Field)
            .ThenInclude(o => o.FieldType)
            .AsSplitQuery();

    public async ValueTask<SingleChoiceField?> GetSingleChoiceFieldByIdAsync(
        long singleChoiceFieldId, bool tracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SingleChoiceField> query = SingleChoiceFields;

        if (!tracking)
        {
            query = NoTracking;
        }

        SingleChoiceField? field = await FullInclude(query)
            .FirstOrDefaultAsync(f => f.Id == singleChoiceFieldId, cancellationToken);

        return field;
    }

    public async ValueTask<IReadOnlyCollection<(long Id, string Name, int OptionCount)>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        IQueryable<SingleChoiceField> query = NoTracking;

        IReadOnlyCollection<(long Id, string Name, int OptionCount)> coll
            = (await query.Select(scf => new { Id = scf.Id, Name = scf.Name, OptionCount = scf.Options.Count })
                          .ToListAsync(cancellationToken)).Select(scf => (scf.Id, scf.Name, scf.OptionCount)).ToList();

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsByIdsAsync(
        CancellationToken cancellationToken = default,
        params List<long> singleChoiceFieldIds)
    {
        if (singleChoiceFieldIds.Count == 0)
        {
            return [];
        }

        IQueryable<SingleChoiceField> query = SingleChoiceFields.Where(f => singleChoiceFieldIds.Contains(f.Id));

        IReadOnlyCollection<SingleChoiceField> coll = await FullInclude(query)
            .ToListAsync(cancellationToken);

        return coll;
    }

    public void AddSingleChoiceField(SingleChoiceField field)
    {
        singleChoiceFields.Add(field);
    }

    public void RemoveSingleChoiceField(SingleChoiceField field)
    {
        singleChoiceFields.Remove(field);
    }

    public async ValueTask<IReadOnlyCollection<Option>> GetOptionsByIdsAsync(
        CancellationToken cancellationToken = default, params List<long> optionIds)
    {
        if (optionIds.Count == 0)
        {
            return [];
        }

        IQueryable<Option> query = options
                                   .Include(o => o.OptionFields)
                                   .Where(o => optionIds.Contains(o.Id));

        IReadOnlyCollection<Option> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public void AddOptions(params IEnumerable<Option> newOptions)
    {
        options.AddRange(newOptions);
    }

    public void RemoveOptions(params IEnumerable<Option> oldOptions)
    {
        options.RemoveRange(oldOptions);
    }

    public void AddOptionFields(params IEnumerable<OptionField> fields)
    {
        optionFields.AddRange(fields);
    }

    public void RemoveOptionFields(params IEnumerable<OptionField> fields)
    {
        optionFields.RemoveRange(fields);
    }
}
