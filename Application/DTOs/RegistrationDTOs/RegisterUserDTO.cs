namespace Application.DTOs.RegistrationDTOs;

public class RegisterUserDTO
{
    
    public int NationalId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    
    public override string ToString()
    {
        return $"NationalId: {NationalId}, " +
               $"FirstName: {FirstName}, " +
               $"LastName: {LastName}, "+
               $"DateOfBirth: {DateOfBirth.ToString("yyyy-MM-dd")}, " +  // Uncomment if you want to include DateOfBirth
               $"Email: {Email}, " +
               $"PhoneNumber: {PhoneNumber}"
               ;
    }
}