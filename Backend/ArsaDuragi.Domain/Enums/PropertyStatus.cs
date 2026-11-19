namespace ArsaDuragi.Domain.Enums;

public enum PropertyStatus
{
    Pending = 1,   // waiting for admin moderation
    Active = 2,    // approved and publicly visible
    Rejected = 3,  // rejected by an admin (RejectionReason is filled)
    Sold = 4,
    Archived = 5
}
