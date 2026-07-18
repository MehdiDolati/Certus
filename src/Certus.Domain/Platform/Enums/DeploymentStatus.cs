namespace Certus.Domain.Platform.Enums;

public enum DeploymentStatus
{
    Pending = 0,
    Copying = 1,
    Deployed = 2,
    Monitoring = 3,
    Error = 4,
    Stopped = 5
}
