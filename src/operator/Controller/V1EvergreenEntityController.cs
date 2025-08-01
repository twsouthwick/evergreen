using EvergreenOperator.Entities;
using Json.More;
using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;

namespace EvergreenOperator.Controller;

[EntityRbac(typeof(V1EvergreenEntity), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Deployment), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Service), Verbs = RbacVerb.All)]
public class V1EvergreenEntityController(IKubernetesClient client, ILogger<V1EvergreenEntityController> logger)
    : IEntityController<V1EvergreenEntity>
{
    public async Task ReconcileAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reconciling entity {Entity}.", entity);

        var images = new Images();

        foreach (var image in images)
        {
            logger.LogInformation("Adding {Image}:{Tag} deployment and service for {EntityName}/{EntityNamespace}.", image.Repository, image.Tag, entity.Name(), entity.Namespace());

            await client.CreateAsync(entity.CreateDeployment(image), cancellationToken);
            await client.CreateAsync(entity.CreateService(image), cancellationToken);

            logger.LogInformation("Created {Image}:{Tag} deployment and service for {EntityName}/{EntityNamespace}.", image.Repository, image.Tag, entity.Name(), entity.Namespace());
        }

        logger.LogInformation("Reconciled entity {Entity}.", entity);
    }

    public Task DeletedAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleted entity {Entity}.", entity);

        return Task.CompletedTask;
    }
}
