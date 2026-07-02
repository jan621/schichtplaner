using CrossCutting.Entities;
using CrossCutting.Managers;

namespace TagManagement.Contract;

public interface ITagManager : IEntityManager<Tag>
{
    Task<IEnumerable<Tag>> GetAllNonAccommodationAsync();

    Task UpdateSortIndex(List<Tag> currentTags, Tag tag, bool upward);
}