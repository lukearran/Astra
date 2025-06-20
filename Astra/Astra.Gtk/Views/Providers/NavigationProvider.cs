using Adw;

namespace Astra.Gtk.Views.Providers;

public class NavigationProvider(
    Adw.NavigationView navigationView)
    : INavigationProvider
{
    public void Go(NavigationPage page) => navigationView.Push(page);

    public void Back() => navigationView.Pop();
}