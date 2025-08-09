using Adw.Internal;
using Gtk;
using Builder = Gtk.Builder;

namespace Astra.Gtk.Views;

public class ErrorStatusPage : Adw.NavigationPage
{
    [Connect("_statusPage")]
    private readonly Adw.StatusPage? _statusPage = null;
    
    private ErrorStatusPage(
        string title,
        string description,
        Builder builder) : base(
        new NavigationPageHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
        
        _statusPage?.SetTitle(title);
        _statusPage?.SetDescription(description);
    }
    
    public ErrorStatusPage(string title, string description)
        : this(title, description, new Builder("ErrorStatusPage.ui"))
    {
    }
}