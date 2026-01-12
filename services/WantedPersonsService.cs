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

        return ServiceResult<PaginatedResponse<WantedPersonSummaryResponse>>.Ok( new PaginatedResponse<WantedPersonSummaryResponse>(totalItems, dtos) );
    }

    public async Task<ServiceResult<WantedPersonSummaryResponse>> GetByIdAsync(int id)
    {
        var person = await _context.WantedPersons
        .Include(p => p.Images)
        .Include(p => p.Aliases)
        .Include(p => p.Files)
        .Include(p => p.Subjects)
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
        {
            return ServiceResult<WantedPersonSummaryResponse>.Fail($"Persoana cu id {id} nu a fost gasită.");
        }

        // Dacă l-a găsit, îl împachetăm în rezultat
        return ServiceResult<WantedPersonSummaryResponse>.Ok(WantedPersonMappers.ToSummaryDto(person));
    }
}