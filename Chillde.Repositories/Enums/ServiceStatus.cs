namespace Chillde.Repositories.Enums;

public enum ServiceStatus
{
    Inactive = 0, // Service is inactive and not available
    Active = 1, // Service is active and available for use
    Pending = 2, // Service is awaiting activation or approval
    Suspended = 3, // Service is temporarily suspended
    Completed = 4, // Service has been completed or finished
    Cancelled = 5 // Service was cancelled and is no longer available
}