using EvergreenOperator.Entities;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Entities;
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

            await CreateOrPatchAsync(entity.CreateDeployment(image), cancellationToken);

            foreach (var service in image.Services)
            {
                await CreateOrPatchAsync(entity.CreateService(service), cancellationToken);
            }

            logger.LogInformation("Created {Image}:{Tag} deployment and service for {EntityName}/{EntityNamespace}.", image.Repository, image.Tag, entity.Name(), entity.Namespace());
        }

        logger.LogInformation("Reconciled entity {Entity}.", entity);
    }

    public Task DeletedAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleted entity {Entity}.", entity);

        return Task.CompletedTask;
    }

    private async Task CreateOrPatchAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        where TEntity : IKubernetesObject<V1ObjectMeta>
    {
        try
        {
            await client.CreateAsync(entity, cancellationToken);
        }
        catch (HttpOperationException e) when (e.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            logger.LogInformation("Conflict found for {Type} {Deployment}", entity.Kind, entity);
            var existing = await client.GetAsync<TEntity>(entity.Name(), entity.Namespace(), cancellationToken);

            if (existing is not { })
            {
                throw;
            }

#pragma warning disable CA2252 // This API requires opting into preview features
            await client.PatchAsync(entity, entity.CreateJsonPatch(existing), cancellationToken);
#pragma warning restore CA2252 // This API requires opting into preview features
        }
    }
}
