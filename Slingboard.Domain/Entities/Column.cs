using Slingboard.Domain.Exceptions;

namespace Slingboard.Domain.Entities;

public class Column
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BoardId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public int? Limit { get; private set; }

    private Column() { }

    internal static Column Create(Guid boardId, string title, int order, int? limit = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Título da coluna é obrigatório.");

        return new Column
        {
            BoardId = boardId,
            Title = title.Trim(),
            Order = order,
            Limit = limit
        };
    }

    public void UpdateDetails(string title, int? limit)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Título da coluna é obrigatório.");

        Title = title.Trim();
        Limit = limit;
    }

    public void UpdateOrder(int newOrder) => Order = newOrder;
}