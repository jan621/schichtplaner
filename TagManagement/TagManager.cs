using CrossCutting.Entities;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using TagManagement.Contract;

namespace TagManagement;

public class TagManager(
    IDatabaseManager<Tag, PlanerContext> databaseManager)
    : ITagManager
{
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        var tags = await databaseManager.GetAllAsync();
        return tags;
    }

    public async Task<Tag?> GetByIdAsync(string id)
    {
        var tag = await databaseManager.GetByIdAsync(id);
        return tag;
    }

    public async Task<bool> CreateAsync(Tag entity)
    {
        var created = await databaseManager.AddAsync(entity);
        return created;
    }

    public async Task<bool> UpdateAsync(Tag entity,bool ignoreId)
    {
        var updated = await databaseManager.UpdateAsync(entity, ignoreId);
        return updated;
    }

    public async Task<bool> DeleteAsync(Tag entity, bool ignoreId)
    {
        var deleted = await databaseManager.DeleteAsync(entity, ignoreId);
        return deleted;
    }

    public async Task<IEnumerable<Tag>> GetAllNonAccommodationAsync()
    {
        var dbResult = await databaseManager.GetAllAsync();
        var tags = dbResult.Where(t => t.IsNonAccommodation);
        return tags;
    }

    public async Task UpdateSortIndex(List<Tag> currentTags, Tag tag, bool upward)
    {
        
        if (upward)
        {
            if (tag.SortIndex < currentTags.Max(t => t.SortIndex))
            {
                tag.SortIndex++;
            }
            
            var tagsWithSameIndex = currentTags.Where(t => t.SortIndex == tag.SortIndex && t.Id != tag.Id).ToList();
            
            foreach (var otherTag in tagsWithSameIndex)
            {
                otherTag.SortIndex--;
                await databaseManager.UpdateAsync(otherTag);
            }
        }
        else
        {
            if (tag.SortIndex > currentTags.Min(t => t.SortIndex))
            {
                tag.SortIndex--;
            }
            
            var tagsWithSameIndex = currentTags.Where(t => t.SortIndex == tag.SortIndex && t.Id != tag.Id).ToList();
            
            foreach (var otherTag in tagsWithSameIndex)
            {
                otherTag.SortIndex++;
                await databaseManager.UpdateAsync(otherTag);
            }
        }

        await databaseManager.UpdateAsync(tag);
    }
}