using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;

namespace TeamManagement.Contract;

public interface ITeamManager
{
    Task<IEnumerable<Team>> GetAllAsync();
    
    Task<bool> AddAsync(Team team);

    Task<bool> EditAsync(Team team);

    Task<bool> DeleteAsync(Team team);
}