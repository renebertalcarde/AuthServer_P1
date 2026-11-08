namespace HRMobile.Components;

public partial class AppPageLayout : ContentView
{
    public static readonly BindableProperty HeaderProperty =
        BindableProperty.Create(
            nameof(Header),
            typeof(AppHeader),
            typeof(AppPageLayout),
            propertyChanged: OnHeaderChanged);

    public AppHeader? Header
    {
        get => (AppHeader?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly BindableProperty PageContentProperty =
        BindableProperty.Create(
            nameof(PageContent),
            typeof(View),
            typeof(AppPageLayout));

    public View? PageContent
    {
        get => (View?)GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public AppPageLayout()
    {
        InitializeComponent();
    }

    private static void OnHeaderChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        var layout = (AppPageLayout)bindable;

        layout.HeaderContainer.Content = newValue as AppHeader;
    }
}