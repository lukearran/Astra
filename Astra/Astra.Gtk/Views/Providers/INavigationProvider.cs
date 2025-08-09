using Adw;

namespace Astra.Gtk.Views.Providers;

public interface INavigationProvider
{
    public void Go(NavigationPage page);
    public void Back();
}