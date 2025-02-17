namespace Chillde.Repositories.Enums;

public enum RequestStatus
{
    Pending = 0, // Request has been submitted but not yet processed
    InProgress = 1, // Request is being worked on
    Completed = 2, // Request has been successfully completed
    Rejected = 3, // Request has been rejected
    Cancelled = 4 // Request was cancelled by the user or system
}