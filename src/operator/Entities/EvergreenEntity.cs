using k8s.Models;
using KubeOps.Abstractions.Entities;
using System.Collections;

namespace EvergreenOperator.Entities;

[KubernetesEntity(Group = "evergreen-ils.org", ApiVersion = "v1", Kind = "evergreen")]
public partial class V1EvergreenEntity : CustomKubernetesEntity<V1EvergreenEntity.EntitySpec>
{
    public override string ToString() => $"Evergreen Entity ({Metadata.Name}))";

    public class EntitySpec
    {
    }
}

public class Images : IEnumerable<Image>
{
    public Image Ejabberd { get; set; } = new Image("ejabberd")
    {
        Repository = "ejabberd/ecs",
        Tag = "25.07",
        Services = [
            new("ejabberd-private"){
                Ports = [
                    new("c2s", 5222),
                    new("s2s", 5269),
                    new("http", 5280),
                    new("https", 5443)
                    ]
            },
            new("ejabberd-public"){
                Ports = [
                    new("c2s", 5222),
                    new("s2s", 5269),
                    new("http", 5280),
                    new("https", 5443)
                    ]
            }
            ]
    };

    public Image OpenSrfRouter { get; } = new Image("opensrf-router")
    {
        Repository = "opensrf-router"
    };

    public Image OpenSrfWebSocket { get; } = new Image("opensrf-websocker")
    {
        Repository = "opensrf-websocket",
        Services = [new("opensrf-websocket") { Ports = [new("http", 7682)] }]
    };

    public Image Evergreen { get; } = new Image("evergreen")
    {
        Repository = "evergreen",
        Services = [new("http") { Ports = [new("http", 80)] }]
    };

    public Image Memcached { get; set; } = new Image("memcached")
    {
        Repository = "memcached",
        Tag = "1.6.38",
        Services = [new("memcached") { Ports = [new("tcp", 11211)] }]
    };

    public IEnumerator<Image> GetEnumerator()
    {
        yield return Ejabberd;
        yield return Memcached;
        yield return OpenSrfRouter;
        yield return OpenSrfWebSocket;
        yield return Evergreen;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public record Image(string Name)
{
    public required string Repository { get; init; }
    public string Tag { get; init; } = "latest";
    public string PullPolicy { get; init; } = "IfNotPresent";
    public Service[] Services { get; init; } = [];

    public override string ToString()
    {
        return $"{Repository}:{Tag}";
    }
}

public record Service(string Name)
{
    public ServicePort[] Ports { get; init; } = [];
}

public record ServicePort(string Name, int Port);

public class EvergreenService
{
    public string Name { get; set; } = string.Empty;
}

public class EvergreenDatabase
{
    public string SecretRef { get; set; } = string.Empty;

    public string PasswordKey { get; set; } = "Password";
}