using System.Windows;
using Forms = System.Windows.Forms;

namespace TidyFlow.Services;

/// <summary>TidyFlow's icon and menu in the Windows notification area.</summary>
public sealed class TrayIcon : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Forms.ToolStripMenuItem _watchItem;

    public TrayIcon(Action open, Action organizeNow, Action preview, Action toggleWatcher, Action exit)
    {
        _watchItem = new Forms.ToolStripMenuItem("Watch folder for new files", null, (_, _) => toggleWatcher());

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add(new Forms.ToolStripMenuItem("Open TidyFlow", null, (_, _) => open()) { Font = new System.Drawing.Font(menu.Font, System.Drawing.FontStyle.Bold) });
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Organize now", null, (_, _) => organizeNow());
        menu.Items.Add("Preview changes...", null, (_, _) => preview());
        menu.Items.Add(_watchItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exit());

        _icon = new Forms.NotifyIcon
        {
            Text = "TidyFlow",
            Icon = LoadIcon(),
            ContextMenuStrip = menu,
            Visible = true,
        };
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == Forms.MouseButtons.Left)
                open();
        };
    }

    public void SetWatching(bool watching)
    {
        _watchItem.Checked = watching;
        _icon.Text = watching ? "TidyFlow (watching for new files)" : "TidyFlow";
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }

    private static System.Drawing.Icon LoadIcon()
    {
        var resource = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/icon.ico"));
        return resource is null
            ? System.Drawing.SystemIcons.Application
            : new System.Drawing.Icon(resource.Stream, Forms.SystemInformation.SmallIconSize);
    }
}
