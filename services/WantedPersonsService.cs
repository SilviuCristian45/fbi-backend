namespace FbiApi.Services;

using FbiApi.Data;
using FbiApi.Models;
using FbiApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using FbiApi.Utils;
using FbiApi.Mappers;

public class WantedPersonsService : IWantedPersonsService
{
    private readonly AppDbContext _context;

    public WantedPersonsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<PaginatedResponse<WantedPersonSummaryResponse>>> GetAllAsync(PaginatedQueryDto paginatedQueryDto)
    {

        string search = paginatedQueryDto.Search;
        int pageSize = paginatedQueryDto.PageSize;
        int page = paginatedQueryDto.PageNumber;

        var query = _context.WantedPersons
            .Include(p => p.Images) // Doar imaginile pt listare
            .AsNoTracking() // Optimizare: Read-Only e mai rapid
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(p => 
                (p.Title != null && p.Title.ToLower().Contains(search)) || 
                (p.Description != null && p.Description.ToLower().Contains(search))
            );
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.PublicationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(); 

        var dtos = items.Select(p => p.ToSummaryDto()).ToList();

        return new PaginatedResponse<WantedPersonSummaryResponse>(totalItems, dtos);
    }

   public async Task<ServiceResult<WantedPersonDetailResponse>> GetByIdAsync(int id)
    {
        var person = await _context.WantedPersons
           .Include(p => p.Images)
            .Include(p => p.Aliases)
            .Include(p => p.Files)
            .Include(p => p.Subjects)
            // 2. Optimizare (Doar citim, nu modificăm)
            .AsNoTracking() 
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
        {
            return new ServiceError("Not found person");
        }

        return WantedPersonMappers.ToDetailDto(person);
    }
}