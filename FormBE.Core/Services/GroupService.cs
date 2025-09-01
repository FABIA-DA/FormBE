using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using FormBE.Shared;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IGroupService
{
    /// <summary>
    /// Get all groups.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All groups.</returns>
    public ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a <see cref="Group"/> by its id.
    /// </summary>
    /// <param name="groupId">The id of the group to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="Group"/> or <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<Group, NotFound>> GetGroupByIdAsync(long groupId,
                                                               CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new group.
    /// </summary>
    /// <param name="parentGroupId">The optional group id of the parent group.</param>
    /// <param name="name">The name of the new group.</param>
    /// <param name="subGroupIds">A list of ids for the subgroups.</param>
    /// <param name="formsIds">A list of ids for the forms.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> with the group or a <see cref="ParentNotFound"/></returns>
    public ValueTask<OneOf<Success<Group>, ParentNotFound>> CreateGroupAsync(long? parentGroupId, string name, List<long> subGroupIds, List<long> formsIds,
                                                                             CancellationToken cancellationToken
                                                                                 = default);

    /// <summary>
    /// Update a group by its id.
    /// </summary>
    /// <param name="groupId">The group id of the group to be updated.</param>
    /// <param name="parentGroupId">The optional group id of the new parent group.</param>
    /// <param name="name">The new name for the group.</param>
    /// <param name="subGroupIds">The new subgroups of the group.</param>
    /// <param name="formIds">The new forms of the group.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> if there were no problems. A <see cref="NotFound"/> if the group was not found. A <see cref="ParentNotFound"/> if the parent group was not found. Or a <see cref="ParentIsSelf"/> if the group id is the same as the parent group id.</returns>
    public ValueTask<OneOf<Success, NotFound, ParentNotFound, ParentIsSelf>> UpdateGroupAsync(
        long groupId, long? parentGroupId, string name, List<long> subGroupIds, List<long> formIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a group.
    /// </summary>
    /// <param name="groupId">The group id of the group to be deleted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> if the group was successfully deleted or a <see cref="NotFound"/> if the group was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteGroupAsync(long groupId,
                                                                CancellationToken cancellationToken = default);

    public struct ParentNotFound;

    public struct ParentIsSelf;
}

internal class GroupService(
    IUnitOfWork uow,
    ILogger<GroupService> logger)
    : IGroupService
{
    private IGroupRepository GroupRepository => uow.GroupRepository;
    private IFormRepository FormRepository => uow.FormRepository;
    
    public async ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default) =>
        await GroupRepository.GetGroupsAsync(cancellationToken);

    public async ValueTask<OneOf<Group, NotFound>> GetGroupByIdAsync(long groupId,
                                                                     CancellationToken cancellationToken = default)
    {
        Group? group = await GroupRepository.GetGroupByIdAsync(groupId, false, cancellationToken);

        if (group == null)
        {
            logger.LogInformation("Group with id {GroupId} was not found", groupId);

            return new NotFound();
        }

        return group;
    }

    public async ValueTask<OneOf<Success<Group>, IGroupService.ParentNotFound>> CreateGroupAsync(
        long? parentGroupId, string name, List<long> subGroupIds,  List<long> formIds,
        CancellationToken cancellationToken = default)
    {
        if (parentGroupId.HasValue)
        {
            Group? parent = await GroupRepository.GetGroupByIdAsync(parentGroupId.Value, false, cancellationToken);

            if (parent == null)
            {
                logger.LogInformation("Parent group with id {GroupId} was not found", parentGroupId.Value);

                return new IGroupService.ParentNotFound();
            }
        }
        
        IReadOnlyCollection<Group> subgroups = subGroupIds.Count == 0 ? [] : await GroupRepository.GetGroupsByIdsAsync(cancellationToken, subGroupIds);

        IReadOnlyCollection<Form> forms = formIds.Count == 0 ? [] : await FormRepository.GetFormsByIdsAsync(cancellationToken, formIds);
        
        Group group = new()
        {
            ParentId = parentGroupId,
            Name = name,
            SubGroups = subgroups.ToList(),
            Forms = forms.ToList()
        };

        GroupRepository.AddGroup(group);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("New group with id {GroupId} was created", group.Id);

        return new Success<Group>(group);
    }

    public async ValueTask<OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf>>
        UpdateGroupAsync(long groupId, long? parentGroupId, string name, List<long> subGroupIds,
                         List<long> formIds,
                         CancellationToken cancellationToken = default)
    {
        Group? group = await GroupRepository.GetGroupByIdAsync(groupId, true, cancellationToken);

        if (group == null)
        {
            logger.LogInformation("Tried to update group with id {GroupId}, but it was not found", groupId);

            return new NotFound();
        }

        if (parentGroupId.HasValue
            && group.ParentId != parentGroupId.Value)
        {
            if (groupId == parentGroupId.Value)
            {
                logger.LogInformation("Tried to update parent of group with id {GroupId} to self", groupId);

                return new IGroupService.ParentIsSelf();
            }

            Group? parent = await GroupRepository.GetGroupByIdAsync(parentGroupId.Value, false, cancellationToken);

            if (parent == null)
            {
                logger.LogInformation("Tried to update group with id {GroupId} to be child of group with id {ParentId}, but parent was not found",
                                      groupId, parentGroupId.Value);

                return new IGroupService.ParentNotFound();
            }

            group.ParentId = parentGroupId;
        }

        if (group.Name != name)
        {
            group.Name = name;
        }
        
        if (!group.SubGroups.IdsEqual(subGroupIds, g => g.Id))
        {
            IReadOnlyCollection<Group> subGroups = await GroupRepository.GetGroupsByIdsAsync(cancellationToken, subGroupIds);

            group.SubGroups = subGroups.ToList();
        }
        
        HashSet<long> currentForms = group.Forms.Select(g => g.Id).ToHashSet();
        HashSet<long> newForms = formIds.ToHashSet();
        
        if (!group.Forms.IdsEqual(formIds, f => f.Id))
        {
            IReadOnlyCollection<Form> forms = await FormRepository.GetFormsByIdsAsync(cancellationToken, formIds);

            group.Forms = forms.ToList();
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated group with id {GroupId} to parent with id {parentGroupId} and name {GroupName}",
                              groupId, parentGroupId, name);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteGroupAsync(long groupId,
                                                                      CancellationToken cancellationToken = default)
    {
        Group? group = await GroupRepository.GetGroupByIdAsync(groupId, true, cancellationToken);

        if (group == null)
        {
            logger.LogInformation("Tried to delete group with id {GroupId}, but it was not found", groupId);

            return new NotFound();
        }

        GroupRepository.RemoveGroup(group);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted group with id {GroupId}", groupId);

        return new Success();
    }
}
