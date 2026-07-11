using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Exports;

namespace Slingboard.Infrastructure.Exports;

public class CsvExportService : ICsvExportService
{
    public byte[] Generate(BoardExportData data)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteField("TaskId");
            csv.WriteField("Title");
            csv.WriteField("Description");
            csv.WriteField("Column");
            csv.WriteField("Priority");
            csv.WriteField("DueDate");
            csv.WriteField("AssigneeName");
            csv.WriteField("AssigneeEmail");
            csv.WriteField("Labels");
            csv.WriteField("CreatedAt");
            csv.WriteField("UpdatedAt");
            csv.WriteField("Order");
            csv.WriteField("Status");
            csv.NextRecord();

            foreach (var column in data.Columns)
            {
                foreach (var task in column.Tasks)
                {
                    csv.WriteField(task.TaskId.ToString());
                    csv.WriteField(task.Title);
                    csv.WriteField(task.Description ?? string.Empty);
                    csv.WriteField(task.Column);
                    csv.WriteField(task.Priority);
                    csv.WriteField(task.DueDate?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty);
                    csv.WriteField(task.AssigneeName ?? string.Empty);
                    csv.WriteField(task.AssigneeEmail ?? string.Empty);
                    csv.WriteField(task.Labels);
                    csv.WriteField(task.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                    csv.WriteField(task.UpdatedAt.ToString("dd/MM/yyyy HH:mm"));
                    csv.WriteField(task.Order);
                    csv.WriteField(task.Status);
                    csv.NextRecord();
                }
            }
        }

        return memoryStream.ToArray();
    }
}