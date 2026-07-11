using FluentAssertions;
using Slingboard.Domain.Entities;
using Slingboard.Domain.Enums;
using Slingboard.Domain.Exceptions;

namespace Slingboard.Domain.Tests.Entities;

public class BoardTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void Create_DeveGerarTresColunasPadrao()
    {
        var board = Board.Create("Meu Board", OwnerId);

        board.Columns.Should().HaveCount(3);
        board.Columns.Select(c => c.Title).Should().ContainInOrder("To Do", "In Progress", "Done");
    }

    [Fact]
    public void Create_DeveAdicionarOwnerComoMembro()
    {
        var board = Board.Create("Meu Board", OwnerId);

        board.Members.Should().ContainSingle(m => m.UserId == OwnerId && m.Role == BoardMemberRole.Owner);
    }

    [Fact]
    public void Create_ComTituloVazio_DeveLancarExcecao()
    {
        var act = () => Board.Create("", OwnerId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_ComTituloMaiorQue100Caracteres_DeveLancarExcecao()
    {
        var tituloGrande = new string('A', 101);

        var act = () => Board.Create(tituloGrande, OwnerId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AddMember_ComUsuarioNovo_DeveAdicionarComSucesso()
    {
        var board = Board.Create("Meu Board", OwnerId);
        var newUserId = Guid.NewGuid();

        var member = board.AddMember(newUserId, BoardMemberRole.Member);

        board.Members.Should().Contain(member);
        board.IsMember(newUserId).Should().BeTrue();
    }

    [Fact]
    public void AddMember_ComUsuarioJaMembro_DeveLancarExcecao()
    {
        var board = Board.Create("Meu Board", OwnerId);

        var act = () => board.AddMember(OwnerId, BoardMemberRole.Admin);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*já é membro*");
    }

    [Fact]
    public void RemoveMember_ComOwner_DeveLancarExcecao()
    {
        var board = Board.Create("Meu Board", OwnerId);

        var act = () => board.RemoveMember(OwnerId);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Owner não pode ser removido*");
    }

    [Fact]
    public void RemoveMember_ComMembroExistente_DeveRemoverComSucesso()
    {
        var board = Board.Create("Meu Board", OwnerId);
        var memberId = Guid.NewGuid();
        board.AddMember(memberId, BoardMemberRole.Member);

        board.RemoveMember(memberId);

        board.IsMember(memberId).Should().BeFalse();
    }

    [Fact]
    public void RemoveColumn_QuandoResta1Coluna_DeveLancarExcecao()
    {
        var board = Board.Create("Meu Board", OwnerId);
        var columnsToRemove = board.Columns.Skip(1).Select(c => c.Id).ToList();
        foreach (var id in columnsToRemove)
            board.RemoveColumn(id);

        var lastColumnId = board.Columns.Single().Id;
        var act = () => board.RemoveColumn(lastColumnId);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*pelo menos uma coluna*");
    }

    [Fact]
    public void AddColumn_DeveIncrementarOrderCorretamente()
    {
        var board = Board.Create("Meu Board", OwnerId);

        var novaColuna = board.AddColumn("Em Revisão");

        novaColuna.Order.Should().Be(3);
    }

    [Fact]
    public void AddLabel_ComNomeDuplicado_DeveLancarExcecao()
    {
        var board = Board.Create("Meu Board", OwnerId);
        var color = Slingboard.Domain.ValueObjects.HexColor.Create("#22C55E");
        board.AddLabel("Backend", color);

        var act = () => board.AddLabel("backend", color);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Já existe uma label*");
    }

    [Fact]
    public void AddLabel_ComNomesDiferentes_DevePermitir()
    {
        var board = Board.Create("Meu Board", OwnerId);
        var color = Slingboard.Domain.ValueObjects.HexColor.Create("#22C55E");

        board.AddLabel("Backend", color);
        board.AddLabel("Frontend", color);

        board.Labels.Should().HaveCount(2);
    }
}