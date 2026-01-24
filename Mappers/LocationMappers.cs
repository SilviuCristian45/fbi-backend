using FbiApi.Models;
using FbiApi.Models.Entities;

namespace FbiApi.Mappers;

public static class LocationMappers
{
    // Mapare pentru LISTĂ (Summary)
    public static ReportDto toReportDto(this LocationWantedPerson person)
    {
        return new ReportDto(
            Id: person.Id,
            Name: "ceva",
            Url: person.FileUrl,
            Matches: person.personMatchResults.Select(p => new MatchItemDto(p.ImageUrl, p.Confidence)).ToList()
        );
    }
}