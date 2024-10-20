namespace Application.Interfaces;

public interface IGenerateService
{
    int GenerateRandomNumbers(Tuple<int, int> range);
}