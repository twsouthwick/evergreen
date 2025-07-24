using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace EvergreenOperator.Entities;

[KubernetesEntity(Group = "evergreen-ils.org", ApiVersion = "v1", Kind = "evergreen")]
public partial class V1EvergreenEntity : CustomKubernetesEntity<V1EvergreenEntity.EntitySpec>
{
    public override string ToString() => $"Evergreen Entity ({Metadata.Name}))";

    public class EntitySpec
    {
        public Images Images { get; set; } = new Images();

        public EvergreenDatabase Database { get; set; } = new EvergreenDatabase();

        public EvergreenService[] Services { get; set; } = [];
    }
}

public class Images
{
    public Image Ejabberd { get; set; } = new Image
    {
        Repository = "ejabberd/ecs",
        Tag = "25.07",
        PullPolicy = "IfNotPresent",
        ServiceName = "ejabberd",
    };

    public Image Memcached { get; set; } = new Image
    {
        Repository = "memcached",
        Tag = "1.6.38",
        PullPolicy = "IfNotPresent",
        ServiceName = "memcached",
    };
}

public class Image
{
    public bool IsManaged { get; init; } = true;
    public required string Repository { get; init; } = string.Empty;
    public required string Tag { get; init; } = "latest";
    public required string PullPolicy { get; init; } = "IfNotPresent";
    public required string ServiceName { get; init; }
    public override string ToString()
    {
        return $"{Repository}:{Tag}";
    }
}

public class EvergreenService
{
    public string Name { get; set; } = string.Empty;
}

public class EvergreenDatabase
{
    public string SecretRef { get; set; } = string.Empty;

    public string PasswordKey { get; set; } = "Password";
}