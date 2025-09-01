using GObject;

namespace Astra.Gtk.Views.Providers;

public class ToastProvider(Adw.ToastOverlay toastOverlay) : IToastProvider
{
    public void ShowToast(string message)
    {
        var toast = new Adw.Toast();

        toast.Title = message;
        
        toastOverlay.AddToast(toast);
    }

    public void ShowToast(string message, string buttonName, SignalHandler<Adw.Toast> buttonAction)
    {
        var toast = new Adw.Toast();

        toast.Title = message;
        toast.ButtonLabel = buttonName;
        toast.OnButtonClicked += buttonAction;
        
        toastOverlay.AddToast(toast);
    }
}