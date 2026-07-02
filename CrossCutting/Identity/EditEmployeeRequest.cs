using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;

namespace CrossCutting.Identity;

public class EditEmployeeRequest
{
    public required User Employee { get; set; }
    
    public required List<Team> Teams { get; set; }
    
    public Role Role { get; set; }
}