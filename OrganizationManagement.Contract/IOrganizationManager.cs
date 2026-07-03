using CrossCutting.Entities;
using CrossCutting.Managers;

namespace OrganizationManagement.Contract;

public interface IOrganizationManager : IEntityManager<Organization>
{
    Task<User?> GetOrganizationLeaderAsync(Organization organization);
}