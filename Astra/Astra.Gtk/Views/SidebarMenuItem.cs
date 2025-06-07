using Gtk;
using Gtk.Internal;
using Builder = Gtk.Builder;
using Image = Gtk.Image;
using Label = Gtk.Label;
using ListBoxRow = Gtk.ListBoxRow;

namespace Astra.Gtk.Views;

public class SidebarMenuItem : ListBoxRow
{
    public readonly Type PageViewType;
    
    [Connect("_root")]
    private readonly ListBoxRow? _root = null;
    
    [Connect("sidebar_menu_icon")]
    private readonly Image? _sidebarMenuIcon = null;
    
    [Connect("sidebar_menu_label")]
    private readonly Label? _sidebarMenuLabel = null;
    
    private SidebarMenuItem(
        string menuIconName,
        string menuIconLabel,
        Builder builder,
        Type pageViewType) :
        base(new ListBoxRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);

        PageViewType = pageViewType;
        _sidebarMenuIcon?.SetFromIconName(menuIconName);
        _sidebarMenuLabel?.SetLabel(menuIconLabel);
        _root?.SetName(menuIconLabel);
    }

    public SidebarMenuItem(
        string menuIconName,
        string menuLabel,
        Type pageType)
        : this(
            menuIconName,
            menuLabel,
            new Builder("SidebarMenuItem.ui"),
            pageType)
    {
    }
}