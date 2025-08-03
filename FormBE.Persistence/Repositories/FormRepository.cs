using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFormRepository
{
    /// <summary>
    /// Gets a form by its id.
    /// </summary>
    /// <param name="formId">The id of the form to get.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The form or null if not found.</returns>
    public ValueTask<Form?> GetFormByIdAsync(long formId, bool tracking = true,
                                             CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all forms without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All forms without tracking.</returns>
    public ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a subset of existing forms with tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="formIds">The ids of the forms to get.</param>
    /// <returns>Gets the requested forms with tracking.</returns>
    public ValueTask<IReadOnlyCollection<Form>> GetFormsByIdsAsync(CancellationToken cancellationToken = default, params List<long> formIds);
    
    /// <summary>
    /// Adds tracking for this form.
    /// </summary>
    /// <param name="form">The form to add.</param>
    public void AddForm(Form form);
    
    /// <summary>
    /// Adds tracking for the form with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="form">The form to delete.</param>
    public void RemoveForm(Form form);
}

internal class FormRepository(DbSet<Form> forms) : IFormRepository
{
    private IQueryable<Form> Forms => forms;
    private IQueryable<Form> NoTracking => Forms.AsNoTracking();

    public async ValueTask<Form?> GetFormByIdAsync(long formId, bool tracking = true,
                                                   CancellationToken cancellationToken = default)
    {
        IQueryable<Form> query = Forms;

        if (!tracking)
        {
            query = NoTracking;
        }

        Form? form = await query.Include(f => f.FormFieldGroups)
                                .FirstOrDefaultAsync(x => x.Id == formId, cancellationToken);

        return form;
    }

    public async ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<Form> query = NoTracking;

        IReadOnlyCollection<Form> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<Form>> GetFormsByIdsAsync(CancellationToken cancellationToken = default, params List<long> formIds)
    {
        if (formIds.Count == 0)
        {
            return [];
        }
        
        IQueryable<Form> query = Forms.Where(f => formIds.Contains(f.Id));
        
        IReadOnlyCollection<Form> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public void AddForm(Form form)
    {
        forms.Add(form);
    }

    public void RemoveForm(Form form)
    {
        forms.Remove(form);
    }
}
