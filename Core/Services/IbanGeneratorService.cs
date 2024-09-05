using System.Net.Http.Json;
using Core.Entities;
using Core.Interfaces;

namespace Core.Services;

public class IbanGeneratorService : IIbanGeneratorService
{
    public async Task<string> GenerateIban(int nationalId)
    {
        
        string nationalIdAsString = nationalId.ToString();
        
        string ibanPrefix = "00";


        Random random = new Random();
        
        // generate random 7 digits, In order to make iban numbers hardly tracked.
        int random7Digits = GeneratedRandom7DigitsNumber();
        
        // first 4 digits of the national Id
        string nationalIdFirst4Digits = nationalIdAsString.Substring(0,4);
        // final 4 digits of the national Id
        string nationalIdFianl4Digits = nationalIdAsString.Substring(nationalIdAsString.Length - 4,nationalIdAsString.Length -6);
        
        // Form of generating Iban
        return
            $"{ibanPrefix}{nationalIdFirst4Digits}0{random7Digits}0{nationalIdFianl4Digits}";
    }

    private int GeneratedRandom7DigitsNumber()
    {
        Random random = new Random();
        
        // generate random 7 digits, In order to make iban numbers hardly tracked.
        return random.Next(2090209, 8607080);
    }

}