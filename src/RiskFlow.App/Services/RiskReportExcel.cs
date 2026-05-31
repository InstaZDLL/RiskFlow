using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using RiskFlow.Core.Risks;

namespace RiskFlow.Services;

/// <summary>Génère le tableau des risques d'une analyse au format Excel (.xlsx).</summary>
public static class RiskReportExcel
{
    public static byte[] Generate(Analysis analysis, IReadOnlyList<Risk> risks, RiskMatrixModel model,
        string author, string organization, DateTimeOffset date)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Risques");

        // En-tête du document.
        ws.Cell(1, 1).Value = "Analyse de risques";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;

        var meta = string.Join("   —   ", new[] { analysis.Name, author, organization, $"Exporté le {date:dd/MM/yyyy}" }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
        ws.Cell(2, 1).Value = meta;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromArgb(90, 90, 90);

        const int groupRow = 4;
        const int headRow = 5;
        var dataRow = headRow + 1;

        // Ligne de groupes (fusionnée).
        ws.Range(groupRow, 1, groupRow, 4).Merge().Value = "IDENTIFICATION";
        var avant = ws.Range(groupRow, 5, groupRow, 7).Merge();
        avant.Value = "AVANT ATTÉNUATION";
        avant.Style.Fill.BackgroundColor = XLColor.FromArgb(253, 230, 138);
        var apres = ws.Range(groupRow, 8, groupRow, 11).Merge();
        apres.Value = "APRÈS ATTÉNUATION";
        apres.Style.Fill.BackgroundColor = XLColor.FromArgb(187, 247, 208);
        ws.Range(groupRow, 1, groupRow, 12).Style.Font.Bold = true;
        ws.Range(groupRow, 1, groupRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Ligne d'en-têtes de colonnes.
        var headers = new[] { "#", "Titre", "Catégorie", "Description", "Gravité", "Probabilité", "Niveau",
            "Stratégie", "Gravité", "Probabilité", "Niveau", "Continuer ?" };
        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 244);
        }

        // Données.
        var r = dataRow;
        var n = 1;
        foreach (var risk in risks)
        {
            ws.Cell(r, 1).Value = n;
            ws.Cell(r, 2).Value = risk.Title;
            ws.Cell(r, 3).Value = risk.Category;
            ws.Cell(r, 4).Value = risk.Description ?? string.Empty;
            ws.Cell(r, 5).Value = Label(model.SeverityLevels, risk.BeforeSeverityIndex);
            ws.Cell(r, 6).Value = Label(model.LikelihoodLevels, risk.BeforeLikelihoodIndex);
            LevelCell(ws.Cell(r, 7), model.Level(risk.BeforeSeverityIndex, risk.BeforeLikelihoodIndex));
            ws.Cell(r, 8).Value = risk.MitigationStrategy ?? string.Empty;
            ws.Cell(r, 9).Value = Label(model.SeverityLevels, risk.AfterSeverityIndex);
            ws.Cell(r, 10).Value = Label(model.LikelihoodLevels, risk.AfterLikelihoodIndex);
            LevelCell(ws.Cell(r, 11), model.Level(risk.AfterSeverityIndex, risk.AfterLikelihoodIndex));
            ws.Cell(r, 12).Value = risk.CanContinue ? "Oui" : "Non";
            r++;
            n++;
        }

        // Mise en forme : bordures, largeurs, retour à la ligne.
        var table = ws.Range(groupRow, 1, Math.Max(r - 1, headRow), 12);
        table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        table.Style.Border.OutsideBorderColor = XLColor.FromArgb(221, 221, 221);
        table.Style.Border.InsideBorderColor = XLColor.FromArgb(221, 221, 221);
        table.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        ws.Column(2).Width = 28;
        ws.Column(3).Width = 16;
        ws.Column(4).Width = 42;
        ws.Column(8).Width = 42;
        ws.Columns(5, 11).Width = 14;
        ws.Column(4).Style.Alignment.WrapText = true;
        ws.Column(8).Style.Alignment.WrapText = true;
        ws.Column(2).Style.Alignment.WrapText = true;
        ws.SheetView.FreezeRows(headRow);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void LevelCell(IXLCell cell, RiskLevel level)
    {
        var (_, red, green, blue) = RiskPalette.Argb(level);
        cell.Value = level.ToFr();
        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(red, green, blue);
        cell.Style.Font.FontColor = XLColor.White;
        cell.Style.Font.Bold = true;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static string Label(IReadOnlyList<string> levels, int index)
        => index >= 0 && index < levels.Count ? levels[index] : string.Empty;
}
