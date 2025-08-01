using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFormRepository
{
    public ValueTask<Form?> GetFormByIdAsync(int formId, bool tracking = true,
                                             CancellationToken cancellationToken = default);

    public ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default);
    public void AddForm(Form form);
    public void RemoveForm(Form form);
}

internal class FormRepository(DbSet<Form> forms) : IFormRepository
{
    private IQueryable<Form> Forms => forms;
    private IQueryable<Form> NoTracking => Forms.AsNoTracking();

    public async ValueTask<Form?> GetFormByIdAsync(int formId, bool tracking = true,
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

    public void AddForm(Form form)
    {
        forms.Add(form);
    }

    public void RemoveForm(Form form)
    {
        forms.Remove(form);
    }
}
