using Slingboard.Application.Features.Exports;

namespace Slingboard.Application.Common.Interfaces;

public interface ICsvExportService
{
    byte[] Generate(BoardExportData data);
}