using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RiskFlow.Core.Risks;

namespace RiskFlow.Services;

/// <summary>Risque sérialisé (format d'export RiskFlow).</summary>
public sealed class RiskExportDto
{
    public int RiskNumber { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Fonctionnel";
    public string? Description { get; set; }
    public int BeforeSeverityIndex { get; set; }
    public int BeforeLikelihoodIndex { get; set; }
    public string? MitigationStrategy { get; set; }
    public int AfterSeverityIndex { get; set; }
    public int AfterLikelihoodIndex { get; set; }
    public bool CanContinue { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>Analyse sérialisée (format d'export RiskFlow natif).</summary>
public sealed class AnalysisExportDto
{
    public string FormatVersion { get; set; } = "riskflow/1";
    public DateTimeOffset? ExportedAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelKey { get; set; } = RiskMatrixModels.Default.Key;
    public string? Author { get; set; }
    public string? Organization { get; set; }
    public string? ProjectDescription { get; set; }
    public List<RiskExportDto> Risks { get; set; } = [];
}

/// <summary>Sérialise / désérialise une analyse au format JSON RiskFlow (round-trip fidèle).</summary>
public static class AnalysisJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(Analysis analysis, IReadOnlyList<Risk> risks, DateTimeOffset exportedAt)
    {
        var dto = new AnalysisExportDto
        {
            ExportedAt = exportedAt,
            Name = analysis.Name,
            ModelKey = analysis.ModelKey,
            Author = analysis.Author,
            Organization = analysis.Organization,
            ProjectDescription = analysis.ProjectDescription,
            Risks = risks.Select(r => new RiskExportDto
            {
                RiskNumber = r.RiskNumber,
                Title = r.Title,
                Category = r.Category,
                Description = r.Description,
                BeforeSeverityIndex = r.BeforeSeverityIndex,
                BeforeLikelihoodIndex = r.BeforeLikelihoodIndex,
                MitigationStrategy = r.MitigationStrategy,
                AfterSeverityIndex = r.AfterSeverityIndex,
                AfterLikelihoodIndex = r.AfterLikelihoodIndex,
                CanContinue = r.CanContinue,
                SortOrder = r.SortOrder,
            }).ToList(),
        };

        return JsonSerializer.Serialize(dto, Options);
    }

    public static AnalysisExportDto Deserialize(string json)
        => JsonSerializer.Deserialize<AnalysisExportDto>(json, Options)
           ?? throw new FormatException("Fichier JSON invalide.");
}
