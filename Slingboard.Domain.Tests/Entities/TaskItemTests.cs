using FluentAssertions;
using Slingboard.Domain.Entities;
using Slingboard.Domain.Enums;
using Slingboard.Domain.Events;
using Slingboard.Domain.Exceptions;

namespace Slingboard.Domain.Tests.Entities;

public class TaskItemTests
{
    private static readonly Guid BoardId = Guid.NewGuid();
    private static readonly Guid ColumnId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_ComDadosValidos_DeveCriarComPrioridadeDefaultMedium()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Implementar login", UserId, order: 0);

        task.Priority.Should().Be(TaskPriority.Medium);
        task.Title.Should().Be("Implementar login");
        task.Order.Should().Be(0);
    }

    [Fact]
    public void Create_DeveDispararTaskCreatedDomainEvent()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Implementar login", UserId, order: 0);

        task.DomainEvents.Should().ContainSingle(e => e is TaskCreatedDomainEvent);
    }

    [Fact]
    public void Create_ComTituloVazio_DeveLancarExcecao()
    {
        var act = () => TaskItem.Create(BoardId, ColumnId, "", UserId, order: 0);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_ComTituloMaiorQue200Caracteres_DeveLancarExcecao()
    {
        var tituloGrande = new string('A', 201);
        var act = () => TaskItem.Create(BoardId, ColumnId, tituloGrande, UserId, order: 0);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void MoveTo_DeveAtualizarColumnIdEOrder()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);
        var novaColuna = Guid.NewGuid();

        task.MoveTo(novaColuna, newOrder: 5, movedByUserId: UserId);
        task.ColumnId.Should().Be(novaColuna);
        task.Order.Should().Be(5);
    }

    [Fact]
    public void MoveTo_DeveDispararTaskMovedDomainEvent()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);
        var novaColuna = Guid.NewGuid();

        task.MoveTo(novaColuna, newOrder: 2, movedByUserId: UserId);
        task.DomainEvents.Should().ContainSingle(e => e is TaskMovedDomainEvent);
    }

    [Fact]
    public void AddLabel_ComMesmaLabelDuasVezes_NaoDeveDuplicar()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);
        var labelId = Guid.NewGuid();

        task.AddLabel(labelId);
        task.AddLabel(labelId);
        task.Labels.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveLabel_ComLabelExistente_DeveRemover()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);
        var labelId = Guid.NewGuid();

        task.AddLabel(labelId);
        task.RemoveLabel(labelId);
        task.Labels.Should().BeEmpty();
    }

    [Fact]
    public void AssignTo_ComUsuario_DeveAtualizarAssigneeId()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);
        var assigneeId = Guid.NewGuid();

        task.AssignTo(assigneeId);
        task.AssigneeId.Should().Be(assigneeId);
    }

    [Fact]
    public void AssignTo_ComNull_DeveDesatribuir()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);

        task.AssignTo(Guid.NewGuid());
        task.AssignTo(null);
        task.AssigneeId.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_DeveAtualizarCamposEUpdatedAt()
    {
        var task = TaskItem.Create(BoardId, ColumnId, "Task", UserId, order: 0);
        var updatedAtAntes = task.UpdatedAt;

        Thread.Sleep(10);

        task.UpdateDetails("Novo título", "Nova descrição", TaskPriority.Urgent, DateTime.UtcNow.AddDays(1));
        task.Title.Should().Be("Novo título");
        task.Priority.Should().Be(TaskPriority.Urgent);
        task.UpdatedAt.Should().BeAfter(updatedAtAntes);
    }
}