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
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete</param>
    /// <returns>The form or null if not found.</returns>
    public ValueTask<Form?> GetFormByIdAsync(long formId, bool tracking = true,
                                             CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all forms or only those which ids are in the <see cref="formIds"/> set.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete</param>
    /// <param name="formIds">An optional set for ids to get with tracking.</param>
    /// <returns>All forms without tracking or forms from the <see cref="formIds"/> with tracking</returns>
    public ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default, params HashSet<long> formIds);
    
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

    public async ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default, params HashSet<long> formIds)
    {
        IQueryable<Form> query = NoTracking;

        if (formIds.Count > 0)
        {
            query = Forms.Where(x => formIds.Contains(x.Id));
        }

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
