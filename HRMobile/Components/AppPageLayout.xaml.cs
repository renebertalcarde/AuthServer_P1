namespace HRMobile.Components;

public partial class AppPageLayout : ContentView
{
    public static readonly BindableProperty IdentityProperty =
        BindableProperty.Create(
            nameof(User),
            typeof(object),
            typeof(AppPageLayout));

    public static readonly BindableProperty PageContentProperty =
        BindableProperty.Create(
            nameof(PageContent),
            typeof(View),
            typeof(AppPageLayout));

    public object? User
    {
        get => GetValue(IdentityProperty);
        set => SetValue(IdentityProperty, value);
    }

    public View? PageContent
    {
        get => (View?)GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public AppPageLayout()
    {
        InitializeComponent();
    }
}