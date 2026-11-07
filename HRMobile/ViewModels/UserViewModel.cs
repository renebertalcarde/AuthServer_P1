using HRMobile.Objects;
using HRMobile.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HRMobile.ViewModels;

public class UserViewModel : TokenParser, INotifyPropertyChanged
{
    public new string UserName
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public new string CompanyName
    {
        get => _companyName;
        set
        {
            _companyName = value;
            OnPropertyChanged();
        }
    }
    public new bool IsSignedIn
    {
        get => _isSignedIn;
        set
        {
            _isSignedIn = value;
            OnPropertyChanged();
        }    
    }
    public bool IsSignedOut
    {
        get => !_isSignedIn;
        set
        {
            _isSignedIn = !value;
            OnPropertyChanged();
        }
    }

    private string _username = string.Empty;
    private string _companyName = string.Empty;
    private bool _isSignedIn = false;

    public UserViewModel(IAuthService authService) : base(authService)
    {
        Fetched += UserViewModel_Fetched;
    }

    private void UserViewModel_Fetched(object? sender, EventArgs e)
    {
        if (UserName != base.UserName) 
            UserName = base.UserName ?? "{Unknown}";
        if (CompanyName != base.CompanyName)
            CompanyName = base.CompanyName ?? "{Unknown}";
        if (IsSignedIn != base.IsSignedIn)
            IsSignedIn = base.IsSignedIn;
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