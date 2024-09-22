using Application.DTOs.SearchUserDto;

namespace Application.Mappers;

public class StringToIntMapper
{
    public static int ConvertStringToInt(SearchByIdDto seachDto)
    {
        return int.Parse(seachDto.NationalId);
    }
}