using CrossCutting.Entities;
using CrossCutting.Enums;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using Microsoft.AspNetCore.Identity;
using OrganizationManagement.Contract;

namespace OrganizationManagement;

public class OrganizationManager(
    IDatabaseManager<Organization, PlanerIdentityContext> databaseManager,
    UserManager<User> identityUserManager)
    : IOrganizationManager
{
    public async Task<User?> GetOrganizationLeaderAsync(Organization organization)
    {
        foreach (var user in organization.Users ?? Enumerable.Empty<User>())
        {
            var isAdministrator = await identityUserManager.IsInRoleAsync(user, Role.Administrator.ToString())
                || await identityUserManager.IsInRoleAsync(user, Role.Company.ToString())
                || await identityUserManager.IsInRoleAsync(user, Role.Developer.ToString());

            if (isAdministrator)
                return user;
        }

        // An organization without a leader is possible (e.g. self-registered
        // accounts only) — callers treat this as "nobody to exclude".
        return null;
    }

    public async Task<IEnumerable<Organization>> GetAllAsync()
    {
        var dbResults = await databaseManager.GetAllAsync();
        return dbResults;
    }

    public async Task<Organization?> GetByIdAsync(string id)
    {
        var dbResult = await databaseManager.GetByIdAsync(id);
        return dbResult;
    }

    public async Task<bool> CreateAsync(Organization entity)
    {
        var created = await databaseManager.AddAsync(entity);

        if (!created)
            throw new Exception("Failed to create organization");

        return created;
    }

    public async Task<bool> UpdateAsync(Organization entity, bool ignoreId = false)
    {
        var updated = await databaseManager.UpdateAsync(entity, ignoreId);
        return updated;
    }

    public async Task<bool> DeleteAsync(Organization entity, bool ignoreId = false)
    {
        var deleted = await databaseManager.DeleteAsync(entity, ignoreId);
        return deleted;
    }
}