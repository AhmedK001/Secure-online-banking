using Application.Interfaces;

namespace Application.Services;

public class GenerateService : IGenerateService
{
    public int GenerateRandomNumbers(Tuple<int, int> range)
    {
        Random random = new Random();
        return random.Next(range.Item1, range.Item2);
    }
}