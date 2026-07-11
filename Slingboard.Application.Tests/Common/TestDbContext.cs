using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Tests.Common;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskLabel> TaskLabels => Set<TaskLabel>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v));

        modelBuilder.Entity<Board>().Property(b => b.BackgroundColor)
            .HasConversion(
                c => c == null ? null : c.Value,
                v => v == null ? null : Domain.ValueObjects.HexColor.Create(v));

        modelBuilder.Entity<Label>().Property(l => l.Color)
            .HasConversion(c => c.Value, v => Domain.ValueObjects.HexColor.Create(v));

        modelBuilder.Entity<BoardMember>().HasKey(m => new { m.BoardId, m.UserId });
        modelBuilder.Entity<TaskLabel>().HasKey(tl => new { tl.TaskId, tl.LabelId });

        modelBuilder.Entity<Board>().Navigation(b => b.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
        modelBuilder.Entity<Board>().Navigation(b => b.Columns).UsePropertyAccessMode(PropertyAccessMode.Field);
        modelBuilder.Entity<Board>().Navigation(b => b.Labels).UsePropertyAccessMode(PropertyAccessMode.Field);
        modelBuilder.Entity<TaskItem>().Navigation(t => t.Labels).UsePropertyAccessMode(PropertyAccessMode.Field);

        base.OnModelCreating(modelBuilder);
    }
}