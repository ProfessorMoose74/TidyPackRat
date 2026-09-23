using CommunityToolkit.Mvvm.ComponentModel;
using TidyFlow.Core.Models;

namespace TidyFlow.ViewModels;

/// <summary>One editable row in the categories table.</summary>
public sealed partial class CategoryViewModel : ObservableObject
{
    public CategoryViewModel(FileCategory category)
    {
        Name = category.Name;
        ExtensionsText = string.Join(", ", category.Extensions);
        Destination = category.Destination;
        Enabled = category.Enabled;
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>Extensions as the user types them: "jpg, .png; heic".</summary>
    [ObservableProperty]
    public partial string ExtensionsText { get; set; }

    [ObservableProperty]
    public partial string Destination { get; set; }

    [ObservableProperty]
    public partial bool Enabled { get; set; }

    public FileCategory ToModel() => new()
    {
        Name = Name.Trim(),
        Extensions = FileCategory.ParseExtensions(ExtensionsText),
        Destination = Destination.Trim(),
        Enabled = Enabled,
    };
}
