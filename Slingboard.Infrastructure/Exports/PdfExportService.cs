using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Exports;

namespace Slingboard.Infrastructure.Exports;

public class PdfExportService : IPdfExportService
{
    public byte[] Generate(BoardExportData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(data.BoardTitle).FontSize(20).Bold();
                    col.Item().Text($"Exportado em {data.GeneratedAt:dd/MM/yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).Text($"{data.TotalTasks} tasks · {data.MemberCount} membros").FontSize(10);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    foreach (var column in data.Columns)
                    {
                        col.Item().PaddingBottom(6).Text(column.Title).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                        if (column.Tasks.Count == 0)
                        {
                            col.Item().PaddingBottom(10).Text("Nenhuma task nesta coluna.").FontColor(Colors.Grey.Medium).Italic();
                            continue;
                        }

                        foreach (var task in column.Tasks)
                        {
                            col.Item().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Column(taskCol =>
                            {
                                taskCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(task.Title).Bold().FontSize(11);
                                    row.ConstantItem(70).AlignRight().Text(task.Priority)
                                        .FontColor(PriorityColor(task.Priority)).Bold();
                                });

                                if (!string.IsNullOrWhiteSpace(task.Description))
                                    taskCol.Item().PaddingTop(2).Text(task.Description).FontSize(9).FontColor(Colors.Grey.Darken1);

                                taskCol.Item().PaddingTop(4).Row(row =>
                                {
                                    if (task.DueDate.HasValue)
                                    {
                                        var isOverdue = task.DueDate < DateTime.UtcNow;
                                        row.AutoItem().Text($"Vence: {task.DueDate:dd/MM/yyyy}")
                                            .FontSize(9).FontColor(isOverdue ? Colors.Red.Medium : Colors.Grey.Darken1);
                                        row.AutoItem().PaddingLeft(10);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task.AssigneeName))
                                        row.AutoItem().Text($"Responsável: {task.AssigneeName}").FontSize(9);
                                });

                                if (task.LabelBadges.Count > 0)
                                {
                                    taskCol.Item().PaddingTop(4).Row(row =>
                                    {
                                        foreach (var (name, color) in task.LabelBadges)
                                        {
                                            row.AutoItem().PaddingRight(4).Background(color).PaddingHorizontal(6).PaddingVertical(2)
                                                .Text(name).FontSize(8).FontColor(Colors.White);
                                        }
                                    });
                                }
                            });

                            col.Item().PaddingBottom(6);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string PriorityColor(string priority) => priority switch
    {
        "Urgent" => Colors.Red.Medium,
        "High" => Colors.Orange.Medium,
        "Medium" => Colors.Blue.Medium,
        _ => Colors.Grey.Medium
    };
}