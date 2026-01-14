using FbiApi.Models; // Pt PagedResult
using FbiApi.Models.Entities;
using FbiApi.Utils;

namespace FbiApi.Services;

public interface IWantedPersonsService
{
    Task<ServiceResult<PaginatedResponse<WantedPersonSummaryResponse>>> GetAllAsync(PaginatedQueryDto paginatedQueryDto);
    
     Task<ServiceResult<PaginatedResponse<WantedPersonSummaryResponse>>> GetAllSavedAsync(PaginatedQueryDto paginatedQueryDto, string keycloakId);

    Task<ServiceResult<WantedPersonDetailResponse?>> GetByIdAsync(int id);

    Task<ServiceResult<SaveFavouritePerson>> SavePersonToFavourite(int personId, string username, string keycloakId, bool save);
}