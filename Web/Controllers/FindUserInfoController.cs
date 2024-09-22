using Application.DTOs.SearchUserDto;
using Application.Interfaces;
using Application.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/findUserInfo")]
public class FindUserInfoController : ControllerBase
{
    private readonly ISearchUserService _searchUserService;

    public FindUserInfoController(ISearchUserService searchUserService)
    {
        _searchUserService = searchUserService;
    }
    
    [HttpGet("findById")]
    private IActionResult FindUserById([FromQuery] SearchByIdDto? userNationalId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userSearchResult = _searchUserService.FindUser(
            StringToIntMapper.ConvertStringToInt(userNationalId));

        return Ok(new {Message = userSearchResult});
    }
}