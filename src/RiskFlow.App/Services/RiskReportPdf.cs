using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RiskFlow.Core.Risks;

namespace RiskFlow.Services;

/// <summary>Génère le rapport PDF d'une analyse : en-tête, tableau détaillé et matrices.</summary>
public static class RiskReportPdf
{
    private const string Grid = "#DDDDDD";
    private const string HeaderBg = "#F2F2F4";

    public static byte[] Generate(Analysis analysis, IReadOnlyList<Risk> risks, RiskMatrixModel model,
        string author, string organization, DateTimeOffset date)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(8.5f).FontColor("#1A1A1A"));

                page.Header().Element(h => Header(h, analysis, author, organization, date));
                page.Content().PaddingVertical(8).Column(col =>
                {
                    col.Item().Element(e => Table(e, risks, model));

                    col.Item().PaddingTop(18).Text(LanguageManager.Get("Report_MatrixBefore")).Bold().FontSize(11);
                    col.Item().PaddingTop(4).Element(e => Matrix(e, risks, model, useAfter: false));

                    col.Item().PaddingTop(14).Text(LanguageManager.Get("Report_MatrixAfter")).Bold().FontSize(11);
                    col.Item().PaddingTop(4).Element(e => Matrix(e, risks, model, useAfter: true));
                });
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("RiskFlow · ").FontColor(Colors.Grey.Medium);
                    t.Span($"{date:dd/MM/yyyy}").FontColor(Colors.Grey.Medium);
                    t.Span("   ·   ").FontColor(Colors.Grey.Medium);
                    t.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                    t.Span(" / ").FontColor(Colors.Grey.Medium);
                    t.TotalPages().FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();
    }

    private static void Header(IContainer container, Analysis analysis, string author, string organization, DateTimeOffset date)
    {
        var parts = new[] { analysis.Name, author, organization, $"{LanguageManager.Get("Report_ExportedOn")} {date:dd/MM/yyyy}" }
            .Where(p => !string.IsNullOrWhiteSpace(p));

        container.BorderBottom(1).BorderColor(Grid).PaddingBottom(8).Column(col =>
        {
            col.Item().Text(LanguageManager.Get("Report_Title")).Bold().FontSize(18);
            col.Item().Text(string.Join("  —  ", parts)).FontColor(Colors.Grey.Darken1).FontSize(9.5f);
            if (!string.IsNullOrWhiteSpace(analysis.ProjectDescription))
                col.Item().PaddingTop(2).Text(analysis.ProjectDescription).FontColor(Colors.Grey.Darken1).FontSize(9);
        });
    }

    private static void Table(IContainer container, IReadOnlyList<Risk> risks, RiskMatrixModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(18);   // #
                c.RelativeColumn(2.2f);  // Titre
                c.RelativeColumn(1.3f);  // Catégorie
                c.RelativeColumn(3f);    // Description
                c.RelativeColumn(1.3f);  // Gravité av
                c.RelativeColumn(1.3f);  // Proba av
                c.RelativeColumn(1f);    // Niveau av
                c.RelativeColumn(3f);    // Stratégie
                c.RelativeColumn(1.3f);  // Gravité ap
                c.RelativeColumn(1.3f);  // Proba ap
                c.RelativeColumn(1f);    // Niveau ap
                c.ConstantColumn(38);    // Continuer
            });

            table.Header(h =>
            {
                var titles = new[]
                {
                    LanguageManager.Get("Col_Num"), LanguageManager.Get("Col_Title"),
                    LanguageManager.Get("Col_Category"), LanguageManager.Get("Col_Description"),
                    LanguageManager.Get("Col_Severity"), LanguageManager.Get("Col_Likelihood"),
                    LanguageManager.Get("Col_Level"), LanguageManager.Get("Col_Strategy"),
                    LanguageManager.Get("Col_Severity"), LanguageManager.Get("Col_Likelihood"),
                    LanguageManager.Get("Col_Level"), LanguageManager.Get("Col_Continue"),
                };
                foreach (var title in titles)
                {
                    h.Cell().Background(HeaderBg).Border(0.5f).BorderColor(Grid)
                        .Padding(4).Text(title).Bold().FontSize(8);
                }
            });

            var n = 1;
            foreach (var risk in risks)
            {
                var beforeLevel = model.Level(risk.BeforeSeverityIndex, risk.BeforeLikelihoodIndex);
                var afterLevel = model.Level(risk.AfterSeverityIndex, risk.AfterLikelihoodIndex);

                Text(table, n.ToString());
                Text(table, risk.Title);
                Text(table, RiskText.Category(risk.Category));
                Text(table, risk.Description ?? string.Empty);
                Text(table, Label(model.SeverityLevels, risk.BeforeSeverityIndex));
                Text(table, Label(model.LikelihoodLevels, risk.BeforeLikelihoodIndex));
                LevelCell(table, beforeLevel);
                Text(table, risk.MitigationStrategy ?? string.Empty);
                Text(table, Label(model.SeverityLevels, risk.AfterSeverityIndex));
                Text(table, Label(model.LikelihoodLevels, risk.AfterLikelihoodIndex));
                LevelCell(table, afterLevel);
                Text(table, LanguageManager.Get(risk.CanContinue ? "Detail_Yes" : "Detail_No"));
                n++;
            }
        });
    }

    private static void Matrix(IContainer container, IReadOnlyList<Risk> risks, RiskMatrixModel model, bool useAfter)
    {
        var nSev = model.SeverityCount;
        var nLik = model.LikelihoodCount;

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(80);
                for (var s = 0; s < nSev; s++)
                    c.RelativeColumn();
            });

            // En-tête : coin vide + libellés de gravité.
            table.Cell().Background(HeaderBg).Border(0.5f).BorderColor(Grid).Padding(4).Text(string.Empty);
            for (var s = 0; s < nSev; s++)
                table.Cell().Background(HeaderBg).Border(0.5f).BorderColor(Grid)
                    .Padding(4).AlignCenter().Text(LanguageManager.Get(model.SeverityLevels[s])).Bold().FontSize(8);

            // Lignes : probabilité la plus forte en haut.
            for (var rowIdx = 1; rowIdx <= nLik; rowIdx++)
            {
                var likIndex = nLik - rowIdx;
                table.Cell().Background(HeaderBg).Border(0.5f).BorderColor(Grid)
                    .Padding(4).AlignMiddle().Text(LanguageManager.Get(model.LikelihoodLevels[likIndex])).Bold().FontSize(8);

                for (var s = 0; s < nSev; s++)
                {
                    var level = model.Level(s, likIndex);
                    var numbers = CellNumbers(risks, s, likIndex, useAfter);
                    table.Cell().Background(RiskPalette.Hex(level)).Border(0.5f).BorderColor(Grid)
                        .MinHeight(26).Padding(4).AlignCenter().AlignMiddle()
                        .Text(numbers).FontColor("#FFFFFF").Bold().FontSize(8);
                }
            }
        });
    }

    private static string CellNumbers(IReadOnlyList<Risk> risks, int severityIndex, int likelihoodIndex, bool useAfter)
    {
        var ids = risks
            .Select((r, i) => (r, n: i + 1))
            .Where(x => (useAfter ? x.r.AfterSeverityIndex : x.r.BeforeSeverityIndex) == severityIndex
                     && (useAfter ? x.r.AfterLikelihoodIndex : x.r.BeforeLikelihoodIndex) == likelihoodIndex)
            .Select(x => $"R{x.n}");
        return string.Join(", ", ids);
    }

    private static void Text(TableDescriptor table, string value)
        => table.Cell().Border(0.5f).BorderColor(Grid).Padding(4).Text(value).FontSize(8);

    private static void LevelCell(TableDescriptor table, RiskLevel level)
        => table.Cell().Background(RiskPalette.Hex(level)).Border(0.5f).BorderColor(Grid)
            .Padding(4).AlignCenter().Text(RiskText.Level(level)).FontColor("#FFFFFF").Bold().FontSize(8);

    private static string Label(IReadOnlyList<string> levels, int index)
        => index >= 0 && index < levels.Count ? LanguageManager.Get(levels[index]) : string.Empty;
}
