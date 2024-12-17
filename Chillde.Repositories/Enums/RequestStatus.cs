namespace Chillde.Repositories.Enums
{
    public enum RequestStatus
    {
        Pending = 0,      // Request has been submitted but not yet processed
        InProgress = 1,   // Request is being worked on
        Completed = 2,    // Request has been successfully completed
        Resolved = 3,     // Request has been resolved, closed, or handled
        Rejected = 4,     // Request has been rejected
        Cancelled = 5,    // Request was cancelled by the user or system
    }
}