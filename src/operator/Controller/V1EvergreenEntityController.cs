using EvergreenOperator.Entities;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Rbac;
using Microsoft.Extensions.Logging;

namespace EvergreenOperator.Controller;

[EntityRbac(typeof(V1EvergreenEntity), Verbs = RbacVerb.All)]
public class V1EvergreenEntityController(ILogger<V1EvergreenEntityController> logger)
    : IEntityController<V1EvergreenEntity>
{
    public Task ReconcileAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reconciling entity {Entity}.", entity);
        return Task.CompletedTask;
    }

    public Task DeletedAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting entity {Entity}.", entity);
        return Task.CompletedTask;
    }
}
