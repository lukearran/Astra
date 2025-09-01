using GObject;

namespace Astra.Gtk.Views.Providers;

public interface IToastProvider
{
    public void ShowToast(string message);

    public void ShowToast(string message, string buttonName, SignalHandler<Adw.Toast> buttonAction);
}