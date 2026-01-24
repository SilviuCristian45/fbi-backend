using System.Text.Json.Serialization;
using System.Collections.Generic;


    // 1. Răspunsul Rădăcină (Containerul principal)
    public record GetAllReportsResponse(
        [property: JsonPropertyName("reports")] List<ReportDto> Reports
    );

    // 2. Raportul individual (ce vede agentul în listă)
    public record ReportDto(
        [property: JsonPropertyName("id")] int Id,       // Sau Guid/string, depinde ce ai în DB
        [property: JsonPropertyName("name")] string Name, // Titlul raportului
        [property: JsonPropertyName("url")] string Url,   // Poza încărcată de agent
        [property: JsonPropertyName("matches")] List<MatchItemDto> Matches // Lista de suspecți găsiți
    );

    // 3. Match-ul individual (rezultatul de la AI)
    public record MatchItemDto(
        [property: JsonPropertyName("url")] string Url,         // Poza de la FBI
        [property: JsonPropertyName("confidence")] double Confidence // Procentajul (ex: 85.5)
    );
