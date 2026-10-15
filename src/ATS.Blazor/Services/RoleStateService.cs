namespace ATS.Blazor.Services;

public enum AppRole
{
    None,
    Candidate,
    Admin
}

public class RoleStateService
{
    public AppRole CurrentRole { get; private set; } = AppRole.None;

    public event Action? OnChange;

    public bool IsAdmin => CurrentRole == AppRole.Admin;
    public bool IsCandidate => CurrentRole == AppRole.Candidate;
    public bool HasRole => CurrentRole != AppRole.None;

    public void SelectRole(AppRole role)
    {
        CurrentRole = role;
        NotifyStateChanged();
    }

    public void ResetRole()
    {
        CurrentRole = AppRole.None;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
