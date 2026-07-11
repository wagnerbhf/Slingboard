using Microsoft.EntityFrameworkCore;
using Slingboard.Domain.Entities;

namespace Slingboard.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Board> Boards { get; }
    DbSet<BoardMember> BoardMembers { get; }
    DbSet<Column> Columns { get; }
    DbSet<Label> Labels { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<TaskLabel> TaskLabels { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}