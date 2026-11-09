using HRMobile.Objects;
using HRMobile.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HRMobile.ViewModels;

public class UserViewModel : INotifyPropertyChanged
{
    public string UserName
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public string CompanyName
    {
        get => _companyName;
        set
        {
            _companyName = value;
            OnPropertyChanged();
        }
    }
    public bool IsSignedIn
    {
        get => _isSignedIn;
        set
        {
            _isSignedIn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSignedOut));
        }    
    }
    public bool IsSignedOut => !_isSignedIn;

    private string _username = string.Empty;
    private string _companyName = string.Empty;
    private bool _isSignedIn = false;
    private readonly IAuthService _authService;

    public UserViewModel(IAuthService authService)
    {
        _authService = authService;
        _authService.IdentityChanged += (sender, args) => Update(args.Tokens?.IdToken);
    }

    private void Update(string? token)
    {
        var parser = new TokenParser(token);
        if (UserName != parser.UserName) UserName = parser.UserName;
        if (CompanyName != parser.CompanyName) CompanyName = parser.CompanyName;
        if (IsSignedIn != parser.IsSignedIn) IsSignedIn = parser.IsSignedIn;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}