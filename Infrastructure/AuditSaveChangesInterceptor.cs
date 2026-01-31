using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MassTransit;

namespace myDotnetApiExcercise.Infrastructure;

public interface IAuditableEntity
{
    Guid Id { get; set; }
    DateTime CreateAt { get; set; }
    DateTime UpdateAt { get; set; }
}
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        var entries = eventData.Context.ChangeTracker.Entries();

        // Dựa theo những thuộc tính được cấu hình sẵn để gán giá trị mặc định trước khi cập nhật vào database
        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditableEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreateAt = DateTime.UtcNow;
                    entity.UpdateAt = DateTime.UtcNow;
                    if (entity.Id == Guid.Empty)
                    {
                        entity.Id = NewId.NextGuid();
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdateAt = DateTime.UtcNow;
                }
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}