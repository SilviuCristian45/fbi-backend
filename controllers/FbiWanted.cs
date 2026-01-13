using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // <--- Nu uita asta!

using FbiApi.Models; // Asigură-te că faci using la DTO
using FbiApi.Services;
using FbiApi.Utils;
using FbiApi.Mappers;


[ApiController]
[Route("api/[controller]")]
public class FbiWanted : ControllerBase {
    private readonly IWantedPersonsService _service;

    // Injectăm Service-ul, NU DbContext-ul
    public FbiWanted(IWantedPersonsService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = nameof(Role.USER))]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<WantedPersonSummaryResponse>>>> GetAll(
        [FromQuery] PaginatedQueryDto paginatedQueryDto
    )
    {
        var result = await _service.GetAllAsync(paginatedQueryDto);
        if (result.Success == false) 
        {
            return BadRequest(ApiResponse<PaginatedResponse<WantedPersonSummaryResponse>>.Error(result.ErrorMessage));
        }
        return Ok(ApiResponse<PaginatedResponse<WantedPersonSummaryResponse>>.Success(result.Data));
    }

    [HttpGet("{id}")]
     [Authorize(Roles = nameof(Role.USER))]
    public async Task< ActionResult<ApiResponse<WantedPersonDetailResponse>>> GetById(int id)
    {
        var person = await _service.GetByIdAsync(id);

        if (person.Success == false)
        {
            return NotFound(ApiResponse<WantedPersonDetailResponse>.Error(person.ErrorMessage));
        }

        return Ok(ApiResponse<WantedPersonDetailResponse>.Success(person.Data));
    }

    [HttpPost("{personId}/{save}")]
    [Authorize(Roles = nameof(Role.USER))]
    public async Task<ActionResult<ApiResponse<SaveFavouritePerson>>> saveFavouritePerson(int personId, bool save) {
         var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier);    
         if (string.IsNullOrEmpty(keycloakId))
         {
            return Unauthorized(ApiResponse<SaveFavouritePerson>.Error("Utilizatorul nu a putut fi identificat."));
         }
         var result = await _service.SavePersonToFavourite(personId, keycloakId, save);
         if (result.Success == false)
         {
            return NotFound(ApiResponse<SaveFavouritePerson>.Error(result.ErrorMessage));
         }
         return Ok(ApiResponse<SaveFavouritePerson>.Success(result.Data));
    }
}