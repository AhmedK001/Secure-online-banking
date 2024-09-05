namespace Core.Interfaces;

public interface IIbanGeneratorService
{
    Task<string> GenerateIban(int nationalId);
}