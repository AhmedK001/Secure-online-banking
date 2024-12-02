namespace Application.Interfaces;

public interface IValidate
{
    (bool, string) IsContainsNumbers(string value);
}