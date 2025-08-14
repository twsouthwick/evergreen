using EvergreenOperator.Entities;
using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace EvergreenOperator.Controller;

public static class ControllerExtensions
{
    public static V1Service CreateService(this V1EvergreenEntity entity, Service image)
    {
        var service = new V1Service()
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = new()
            {
                Name = $"{image.Name}",
                NamespaceProperty = entity.Metadata.NamespaceProperty,
                Annotations = new Dictionary<string, string>
                    {
                       { $"{entity.ApiGroup()}/service", image.Name}
                    }
            },
            Spec = new()
            {
                Selector = new Dictionary<string, string>
                    {
                        { $"{entity.ApiGroup()}/service", image.Name }
                    },
                Ports = [.. image.Ports.Select(p=>new V1ServicePort
                    {
                        Port = p.Port,
                        TargetPort = p.Port,
                        Protocol = "TCP",
                        Name = p.Name,
                    })],
                Type = "ClusterIP"
            }
        };

        service.AddOwnerReference(entity.MakeOwnerReference());

        return service;
    }

    public static V1Deployment CreateDeployment(this V1EvergreenEntity entity, Image image)
    {
        var deployment = new V1Deployment()
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = new()
            {
                Name = $"{entity.Metadata.Name}-{image.Name}",
                NamespaceProperty = entity.Metadata.NamespaceProperty,
                Annotations = new Dictionary<string, string>
                {
                    { $"{entity.ApiGroup()}/service", image.Name }
                }
            },
            Spec = new()
            {
                Selector = new V1LabelSelector()
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        { $"{entity.ApiGroup()}/service", image.Name }
                    }
                },
                Replicas = 1,
                Template = new()
                {
                    Metadata = new()
                    {
                        Name = $"{entity.Metadata.Name}-{image.Name}",
                        NamespaceProperty = entity.Metadata.NamespaceProperty,
                        Labels = new Dictionary<string, string>
                        {
                            { $"{entity.ApiGroup()}/service", image.Name },
                            { "evergreen", entity.Metadata.Name }
                        }
                    },
                    Spec = new()
                    {
                        Containers = new List<V1Container>
                        {
                            new()
                            {
                                Name = image.Name,
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

        return deployment;
    }
}
