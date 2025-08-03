namespace FormBE.Shared;

public static class SharedUtil
{
    public static bool IdsEqual<T>(this List<T> first, IEnumerable<long> second, Func<T, long> keySelector)
    {
        HashSet<long> currentIds = first.Select(keySelector).ToHashSet();
        HashSet<long> newIds = second.ToHashSet();
        
        bool setEqual = currentIds.SetEquals(newIds);
        
        return setEqual;
    }
    
    public static (List<long> newIds, List<T> stillItems, List<T> oldItems)
        SeparateItemsById<T>(this List<T> currItems, List<long> newIds, Func<T, long> keySelector)
    {
        List<long> currIds = currItems.Select(keySelector).ToList();
        List<long> ids = newIds.Except(currIds).ToList();
        
        List<T> oldItems = currItems.ExceptBy(newIds, keySelector).ToList();
        List<T> stillItems = currItems.Except(oldItems).ToList();
        
        return (ids, stillItems, oldItems);
    }
}
