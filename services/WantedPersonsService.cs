namespace FbiApi.Services;

using FbiApi.Data;
using FbiApi.Models;
using FbiApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using FbiApi.Utils;
using FbiApi.Mappers;
using FbiApi.Hubs;
using Microsoft.AspNetCore.SignalR;

public class WantedPersonsService : IWantedPersonsService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<SurveilanceHub> _hubContext;

    private readonly ILogger<WantedPersonsService> _logger;

    public WantedPersonsService(AppDbContext context, 
    IHubContext<SurveilanceHub> hubContext,
    ILogger<WantedPersonsService> logger
    )
    {
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<OperationStatus>> SavePersonToFavourite(int personId,string username, string keycloakId, bool save) {
        try {
            if (save) {
                _context.SaveWantedPersons.Add(new SaveWantedPerson { UserId = keycloakId, WantedPersonId = personId } );
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveActivity", 
                    $"Agentul {username} a adăugat un suspect la favoriți!");
            } else {
                await _context.SaveWantedPersons
                .Where(x => x.UserId == keycloakId && x.WantedPersonId == personId)
                .ExecuteDeleteAsync();
            }
            await _context.SaveChangesAsync();
            return new OperationStatus(true);
        } catch (Exception e) {
            Console.WriteLine(e.Message);
            return new ServiceError(e.Message);
        }
    }

     public async Task<ServiceResult<PaginatedResponse<WantedPersonSummaryResponse>>> GetAllSavedAsync(PaginatedQueryDto paginatedQueryDto,
     string keycloakId
     )
    {

        string search = paginatedQueryDto.Search;
        int pageSize = paginatedQueryDto.PageSize;
        int page = paginatedQueryDto.PageNumber;

        var query = _context.WantedPersons
            .Include(p => p.Images)
            .Include(p => p.SaveWantedPersons)
            .Where(p => p.SaveWantedPersons != null && p.SaveWantedPersons.Select(q => q.UserId).Contains(keycloakId))
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

    public async Task<ServiceResult<OperationStatus>> reportLocation(ReportLocationRequest reportLocationRequest, string userId, string username) {
        
        try {
            var location = _context.LocationWantedPersons.Add(new LocationWantedPerson {
                Latitude = reportLocationRequest.Lat,
                Longitude = reportLocationRequest.Lng,
                Description = reportLocationRequest.Details,
                WantedPersonId = reportLocationRequest.WantedId,
                UserId = userId,
                Username = username,
                ReportedAt = DateTime.UtcNow,
                FileUrl = reportLocationRequest.FileUrl,
            });

            await _context.SaveChangesAsync();

            var sightingDto = new {
                id = location.Entity.Id,
                lat = location.Entity.Latitude,
                lng = location.Entity.Longitude,
                details = location.Entity.Description,
                reportedBy = location.Entity.Username,
                timestamp = location.Entity.ReportedAt,
                wantedPersonId = location.Entity.WantedPersonId
            };

            await _hubContext.Clients.All.SendAsync("ReceiveLocation", sightingDto);

            return new OperationStatus(true);
        } catch(Exception e) {
            _logger.LogError(e.Message);
            return new OperationStatus(false);
        }
    }

     public async Task<ServiceResult<List<LocationReportDto>>> GetSightings(int id) {
        try {
            var reports = await _context.LocationWantedPersons // Presupun ca ai tabela asta
                .Where(r => r.WantedPersonId == id)
                .OrderByDescending(r => r.ReportedAt)
                .Select(r => new LocationReportDto(r.Id, r.Latitude, r.Longitude, r.Description, r.Username, r.ReportedAt, r.WantedPersonId, r.FileUrl))
                .ToListAsync();
            return reports;
        } catch(Exception e) {
            _logger.LogError(e.Message);
            return new List<LocationReportDto>();
        }
     } 
}