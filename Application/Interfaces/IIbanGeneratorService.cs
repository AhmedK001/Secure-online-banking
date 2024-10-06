namespace Application.Interfaces;

public interface IIbanGeneratorService
{
    string GenerateIban(int nationalId);
}