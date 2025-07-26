using EvergreenOperator.Entities;
using Json.More;
using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;

namespace EvergreenOperator.Controller;

[EntityRbac(typeof(V1EvergreenEntity), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Deployment), Verbs = RbacVerb.All)]
public class V1EvergreenEntityController(IKubernetesClient client, ILogger<V1EvergreenEntityController> logger)
    : IEntityController<V1EvergreenEntity>
{
    public async Task ReconcileAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reconciling entity {Entity}.", entity);

        var images = new Images();

        foreach (var image in images)
        {
            logger.LogInformation("Adding {Image}:{Tag} deployment/service for {EntityName}/{EntityNamespace}.", image.Repository, image.Tag, entity.Name(), entity.Namespace());

            var deployment = new V1Deployment()
            {
                ApiVersion = "apps/v1",
                Kind = "Deployment",
                Metadata = new()
                {
                    Name = $"{entity.Metadata.Name}-{image.ServiceName}",
                    NamespaceProperty = entity.Metadata.NamespaceProperty,
                    Annotations = new Dictionary<string, string>
                    {
                       { $"{entity.ApiGroup()}/service", image.ServiceName}
                    }
                },
                Spec = new()
                {
                    Selector = new V1LabelSelector()
                    {
                        MatchLabels = new Dictionary<string, string>
                        {
                            { $"{entity.ApiGroup()}/service", image.ServiceName }
                        }
                    },
                    Replicas = 1,
                    Template = new()
                    {
                        Metadata = new()
                        {
                            Name = $"{entity.Metadata.Name}-{image.ServiceName}",
                            NamespaceProperty = entity.Metadata.NamespaceProperty,
                            Labels = new Dictionary<string, string>
                            {
                                { $"{entity.ApiGroup()}/service", image.ServiceName },
                                { "evergreen", entity.Metadata.Name }
                            }
                        },
                        Spec = new()
                        {
                            Containers = new List<V1Container>
                            {
                                new()
                                {
                                    Name = image.ServiceName,
                                    Image = $"{image.Repository}:{image.Tag}",
                                    ImagePullPolicy = image.PullPolicy,
                                }
                            },
                            RestartPolicy = "Always"
                        }
                    }
                }
            };

            deployment.AddOwnerReference(entity.MakeOwnerReference());

            var result = await client.CreateAsync(deployment, cancellationToken);

            var service = new V1Service()
            {
                ApiVersion = "v1",
                Kind = "Service",
                Metadata = new()
                {
                    Name = $"{entity.Metadata.Name}-{image.ServiceName}",
                    NamespaceProperty = entity.Metadata.NamespaceProperty,
                    Annotations = new Dictionary<string, string>
                    {
                       { $"{entity.ApiGroup()}/service", image.ServiceName}
                    }
                },
                Spec = new()
                {
                    Selector = new Dictionary<string, string>
                    {
                        { $"{entity.ApiGroup()}/service", image.ServiceName }
                    },
                    Ports = new List<V1ServicePort>
                    {
                        new()
                        {
                            Port = 80,
                            TargetPort = 80,
                            Protocol = "TCP",
                            Name = "http"
                        }
                    },
                    Type = "ClusterIP"
                }
            };

            service.AddOwnerReference(entity.MakeOwnerReference());

            await client.CreateAsync(service, cancellationToken);

            logger.LogInformation("Created {Image}:{Tag} deployment/service for {EntityName}/{EntityNamespace}.", image.Repository, image.Tag, entity.Name(), entity.Namespace());
        }

        logger.LogInformation("Reconciled entity {Entity}.", entity);
    }

    public Task DeletedAsync(V1EvergreenEntity entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting entity {Entity}.", entity);

        return Task.CompletedTask;
    }
}
