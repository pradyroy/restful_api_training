using Users.Domain.Enums;

namespace Users.Domain.Entities;

public sealed class User
{
    public long Id { get; private set; }

    public string UserName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public UserRole Role { get; private set; } = UserRole.ReadOnly;

    public string EmailId { get; private set; } = default!;
    public string MobileNum { get; private set; } = default!;
    public string? ProfilePicUrl { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User(
        string userName,
        string passwordHash,
        string fullName,
        UserRole role,
        string emailId,
        string mobileNum,
        string? profilePicUrl = null)
    {
        UserName = userName;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        EmailId = emailId;
        MobileNum = mobileNum;
        ProfilePicUrl = profilePicUrl;
    }

    public void UpdateFullName(string fullName)
    {
        FullName = fullName;
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
    }

    public void UpdateEmail(string emailId)
    {
        EmailId = emailId;
    }

    public void UpdateMobile(string mobileNum)
    {
        MobileNum = mobileNum;
    }

    public void UpdateProfilePicUrl(string? url)
    {
        ProfilePicUrl = url;
    }

}
