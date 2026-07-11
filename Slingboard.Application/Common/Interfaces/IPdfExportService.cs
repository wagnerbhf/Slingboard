using Slingboard.Application.Features.Exports;

namespace Slingboard.Application.Common.Interfaces;

public interface IPdfExportService
{
    byte[] Generate(BoardExportData data);
}