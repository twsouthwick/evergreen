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

    public IEnumerator<Image> GetEnumerator()
    {
        yield return Ejabberd;
        yield return Memcached;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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