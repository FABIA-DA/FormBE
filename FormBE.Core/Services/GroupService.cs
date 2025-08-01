using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IGroupService
{
    public ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default);

    public ValueTask<OneOf<Group, NotFound>> GetGroupByIdAsync(int groupId,
                                                               CancellationToken cancellationToken = default);

    public ValueTask<OneOf<Success<Group>, ParentNotFound>> CreateGroupAsync(int? parentGroupId, string name,
                                                             CancellationToken cancellationToken = default);

    public ValueTask<OneOf<Success, NotFound, ParentNotFound, ParentIsSelf>> UpdateGroupAsync(int groupId, int? parentGroupId, string name,
                                                                CancellationToken cancellationToken = default);

    public ValueTask<OneOf<Success, NotFound>> DeleteGroupAsync(int groupId,
                                                                CancellationToken cancellationToken = default);

    public struct ParentNotFound;

    public struct ParentIsSelf;
}

internal class GroupService(IGroupRepository groupRepository, IUnitOfWork uow, ILogger<GroupService> logger) : IGroupService
{
    public async ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default) =>
        await groupRepository.GetGroupsAsync(cancellationToken);

    public async ValueTask<OneOf<Group, NotFound>> GetGroupByIdAsync(int groupId,
                                                               CancellationToken cancellationToken = default)
    {
        Group? group = await groupRepository.GetGroupByIdAsync(groupId, false, cancellationToken);

        if (group == null)
        {
            logger.LogInformation("Group with id {GroupId} was not found", groupId);
            return new NotFound();
        }

        return group;
    }

    public async ValueTask<OneOf<Success<Group>, IGroupService.ParentNotFound>> CreateGroupAsync(int? parentGroupId, string name,
                                                             CancellationToken cancellationToken = default)
    {
        if (parentGroupId.HasValue)
        {
            Group? parent = await groupRepository.GetGroupByIdAsync(parentGroupId.Value, false, cancellationToken);

            if (parent == null)
            {
                logger.LogInformation("Parent group with id {GroupId} was not found", parentGroupId.Value);

                return new IGroupService.ParentNotFound();
            }
        }
        
        Group group = new()
        {
            ParentId = parentGroupId,
            Name = name,
            SubGroups = [],
            Forms = []
        };

        groupRepository.AddGroup(group);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("New group with id {GroupId} was created", group.Id);

        return new Success<Group>(group);
    }

    public async ValueTask<OneOf<Success, NotFound, IGroupService.ParentNotFound, IGroupService.ParentIsSelf>> UpdateGroupAsync(int groupId, int? parentGroupId, string name,
                                                                CancellationToken cancellationToken = default)
    {
        Group? group  = await groupRepository.GetGroupByIdAsync(groupId, true, cancellationToken);

        if (group == null)
        {
            logger.LogInformation("Tried to update group with id {GroupId}, but it was not found", groupId);

            return new NotFound();
        }

        if (group.ParentId != parentGroupId)
        {
            if (parentGroupId.HasValue)
            {
                if (groupId == parentGroupId.Value)
                {
                    logger.LogInformation("Tried to update parent of group with id {GroupId} to self", groupId);

                    return new IGroupService.ParentIsSelf();
                }
                
                Group? parent = await groupRepository.GetGroupByIdAsync(parentGroupId.Value, false, cancellationToken);

                if (parent == null)
                {
                    logger.LogInformation("Tried to update group with id {GroupId} to be child of group with id {ParentId}, but parent was not found", groupId, parentGroupId.Value);
                    return new IGroupService.ParentNotFound();
                }

                group.Parent = parent;
            }
            
            group.ParentId = parentGroupId;
        }

        if (group.Name != name)
        {
            group.Name = name;
        }
        
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated group with id {GroupId} to parent with id {parentGroupId} and name {GroupName}", groupId, parentGroupId, name);
        
        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteGroupAsync(int groupId,
                                                                CancellationToken cancellationToken = default)
    {
        Group? group = await groupRepository.GetGroupByIdAsync(groupId, true, cancellationToken);

        if (group == null)
        {
            logger.LogInformation("Tried to delete group with id {GroupId}, but it was not found", groupId);
            return new NotFound();
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted group with id {GroupId}", groupId);
        
        return new Success();
    }
}
