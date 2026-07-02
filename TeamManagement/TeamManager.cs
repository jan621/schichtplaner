using CrossCutting.Entities;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using TeamManagement.Contract;

namespace TeamManagement;

public class TeamManager(
    IDatabaseManager<Team, PlanerIdentityContext> databaseManager)
    : ITeamManager
{
    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        var teams = await databaseManager.GetAllAsync();
        return teams;
    }

    public async Task<bool> AddAsync(Team team)
    {
        var created = await databaseManager.AddAsync(team);
        return created;
    }

    public async Task<bool> EditAsync(Team team)
    {
        var edited = await databaseManager.UpdateAsync(team, true);
        return edited;
    }

    public async Task<bool> DeleteAsync(Team team)
    {
        var deleted = await databaseManager.DeleteAsync(team);
        return deleted;
    }
}