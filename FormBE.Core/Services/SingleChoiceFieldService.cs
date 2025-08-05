using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using FormBE.Shared;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface ISingleChoiceFieldService
{
    /// <summary>
    /// Get all <see cref="SingleChoiceField"/>s.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IReadOnlyCollection{SingleChoiceField}"/> of <see cref="SingleChoiceField"/>s.</returns>
    public ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a <see cref="SingleChoiceField"/> by its id.
    /// </summary>
    /// <param name="singleChoiceFieldId">The id of the field to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="SingleChoiceField"/> if successful or a <see cref="NotFound"/> if the single choice field was not found.</returns>
    public ValueTask<OneOf<SingleChoiceField, NotFound>> GetSingleChoiceFieldByIdAsync(
        long singleChoiceFieldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a <see cref="SingleChoiceField"/> with its initial values.
    /// </summary>
    /// <param name="name">The initial name.</param>
    /// <param name="options">All initial options, where invalid field ids are ignored.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created <see cref="SingleChoiceField"/>.</returns>
    public ValueTask<SingleChoiceField> CreateSingleChoiceFieldAsync(
        string name, List<(string Name, List<long> FieldIds)> options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a <see cref="SingleChoiceField"/> with its new values.
    /// </summary>
    /// <param name="singleChoiceFieldId">The id of the field to update.</param>
    /// <param name="name">The new name.</param>
    /// <param name="knownOptions">The known options with their updated values.</param>
    /// <param name="newOptions">The new options to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> if there were no problems or a <see cref="NotFound"/> if the field was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> UpdateSingleChoiceFieldAsync(
        long singleChoiceFieldId, string name, List<(long Id, string Name, List<long> FieldIds)> knownOptions,
        List<(string Name, List<long> FieldIds)> newOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a <see cref="SingleChoiceField"/> by its id.
    /// </summary>
    /// <param name="singleChoiceFieldId">The id of the field to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> or a <see cref="NotFound"/> if the field was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteSingleChoiceFieldAsync(
        long singleChoiceFieldId, CancellationToken cancellationToken = default);
}

internal class SingleChoiceFieldService(
    ISingleChoiceFieldRepository singleChoiceFieldRepository,
    IFieldRepository fieldRepository,
    IUnitOfWork uow,
    ILogger<SingleChoiceFieldService> logger) : ISingleChoiceFieldService
{
    public async ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default) =>
        await singleChoiceFieldRepository.GetSingleChoiceFieldsAsync(cancellationToken);

    public async ValueTask<OneOf<SingleChoiceField, NotFound>> GetSingleChoiceFieldByIdAsync(
        long singleChoiceFieldId, CancellationToken cancellationToken = default)
    {
        SingleChoiceField? singleChoiceField
            = await singleChoiceFieldRepository.GetSingleChoiceFieldByIdAsync(singleChoiceFieldId, false,
                                                                              cancellationToken);

        if (singleChoiceField == null)
        {
            logger.LogInformation("Tried to get single choice field with id {SingleChoiceFieldId}, but was not found",
                                  singleChoiceFieldId);

            return new NotFound();
        }

        return singleChoiceField;
    }

    public async ValueTask<SingleChoiceField> CreateSingleChoiceFieldAsync(
        string name,
        List<(string Name, List<long> FieldIds)> options,
        CancellationToken cancellationToken = default)
    {
        SingleChoiceField field = new()
        {
            Name = name,
            FieldGroupSingleChoiceFields = [],
            Options = []
        };
        List<Option> realOptions = [];

        List<long> fieldIds = options.SelectMany(o => o.FieldIds).ToList();

        IReadOnlyCollection<Field> fields = await fieldRepository.GetFieldsByIdsAsync(cancellationToken, fieldIds);
        List<List<Field>> orderedFields = [];
        int k = 0;

        for (int i = 0; i < options.Count; i++)
        {
            orderedFields.Add([]);
            for (int j = 0; j < options[i].FieldIds.Count; j++)
            {
                if (fields.ElementAtOrDefault(k) != null
                    && options[i].FieldIds[j] == fields.ElementAtOrDefault(k)?.Id)
                {
                    orderedFields[i].Add(fields.ElementAt(k));
                    k++;
                }
            }
        }

        for (var i = 0; i < options.Count; i++)
        {
            Option option = new()
            {
                Name = options[i].Name,
                SingleChoiceField = field,
                OptionFields = [],
                OptionResponses = []
            };

            List<OptionField> optionFields = orderedFields[i].Select(f => new OptionField()
            {
                Option = option,
                Field = f
            }).ToList();

            foreach (var optionField in optionFields)
            {
                singleChoiceFieldRepository.AddOptionField(optionField);
            }

            option.OptionFields = optionFields;

            singleChoiceFieldRepository.AddOption(option);

            realOptions.Add(option);
        }

        field.Options = realOptions;

        singleChoiceFieldRepository.AddSingleChoiceField(field);
        logger.LogInformation("Created single choice field with id {SingleChoiceFieldId}", field.Id);

        return field;
    }

    public async ValueTask<OneOf<Success, NotFound>> UpdateSingleChoiceFieldAsync(
        long singleChoiceFieldId, string name, List<(long Id, string Name, List<long> FieldIds)> knownOptions,
        List<(string Name, List<long> FieldIds)> newOptions,
        CancellationToken cancellationToken = default)
    {
        SingleChoiceField? singleChoiceField
            = await singleChoiceFieldRepository.GetSingleChoiceFieldByIdAsync(singleChoiceFieldId, true,
                                                                              cancellationToken);

        if (singleChoiceField == null)
        {
            logger.LogInformation("Tried to update single choice field by id {SingleChoiceFieldId}, but was not found",
                                  singleChoiceFieldId);

            return new NotFound();
        }

        if (singleChoiceField.Name != name)
        {
            singleChoiceField.Name = name;
        }

        List<long> optionIds = knownOptions.Select(o => o.Id).ToList();

        if (!singleChoiceField.Options.IdsEqual(optionIds, o => o.Id))
        {
            (List<long> newIds, List<Option> stillOptions, List<Option> oldOptions)
                = singleChoiceField.Options.SeparateItemsById(optionIds, o => o.Id);

            foreach (var option in oldOptions)
            {
                foreach (var optionField in option.OptionFields)
                {
                    singleChoiceFieldRepository.RemoveOptionField(optionField);
                }

                singleChoiceFieldRepository.RemoveOption(option);
            }

            singleChoiceField.Options = stillOptions;
        }

        List<Option> realNewOptions = [];
        
        if (newOptions.Count > 0)
        {
            List<long> fieldIds = newOptions.SelectMany(o => o.FieldIds).ToList();

            IReadOnlyCollection<Field> fields = await fieldRepository.GetFieldsByIdsAsync(cancellationToken, fieldIds);
            int k = 0;
            
            for (var i = 0; i < newOptions.Count; i++)
            {
                (string newName, List<long> newFieldIds) = newOptions[i];
                Option option = new()
                {
                    Name = newName,
                    OptionFields = [],
                    SingleChoiceField = singleChoiceField,
                    OptionResponses = []
                };
                
                List<Field> newFields = [];
                for (int j = 0; j < newOptions[i].FieldIds.Count; j++)
                {
                    Field? field = fields.ElementAtOrDefault(k);
                    if (field != null
                        && newFieldIds[j] == field.Id)
                    {
                        newFields.Add(field);
                        k++;
                    }
                }

                foreach (var optionFields in newFields.Select(f => new OptionField()
                         {
                             Option = option,
                             Field = f
                         }))
                {
                    singleChoiceFieldRepository.AddOptionField(optionFields);
                    option.OptionFields.Add(optionFields);
                }
                
                singleChoiceFieldRepository.AddOption(option);
                singleChoiceField.Options.Add(option);
            }
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated a single choice field with id {SingleChoiceFieldId}", singleChoiceField.Id);
        
        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteSingleChoiceFieldAsync(
        long singleChoiceFieldId, CancellationToken cancellationToken = default)
    {
        SingleChoiceField? field = await singleChoiceFieldRepository.GetSingleChoiceFieldByIdAsync(singleChoiceFieldId, true, cancellationToken);

        if (field == null)
        {
            logger.LogInformation("Tried to delete single choice field with id {SingleChoiceFieldId}, but was not found", singleChoiceFieldId);

            return new NotFound();
        }
        
        singleChoiceFieldRepository.RemoveSingleChoiceField(field);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted single choice field with id {SingleChoiceFieldId}", field.Id);

        return new Success();
    }
}
