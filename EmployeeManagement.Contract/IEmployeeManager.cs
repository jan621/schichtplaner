using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Identity;

namespace EmployeeManagement.Contract;

public interface IEmployeeManager
{
    Task<IEnumerable<User>> GetAllAsync();
    
    Task<bool> AddAsync(User employee);

    /// <returns>The generated start password of the invited employee.</returns>
    Task<string> InviteEmployeeAsync(SignUpEmployeeRequest request);

    Task<bool> EditAsync(User employee);

    Task<bool> DeleteAsync(User employee);

    Task ConfirmEmployee(string employeeMail);

    Task ReleaseEmployeeAsync(User employee);
}