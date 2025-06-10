using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DesktopKnowledgeAvalonia.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DesktopKnowledgeAvalonia.Services;

namespace DesktopKnowledgeAvalonia.Views;

public partial class SettingWindow : AppWindowBase
{
    private SettingWindowViewModel? ViewModel => DataContext as SettingWindowViewModel;
    private GeneralSettingsViewModel? GeneralSettings => ViewModel?.SelectedCategory?.Content as GeneralSettingsViewModel;
    private LocalizationService? _localizationService;

    public SettingWindow()
    {
        InitializeComponent();
        _localizationService = App.GetService<LocalizationService>();
        var model = new SettingWindowViewModel();
        DataContext = model;
        model.WindowCloseRequested += (s, e) => Close();
    }
    
    public SettingWindow(SettingWindowViewModel model)
    {
        InitializeComponent();
        _localizationService = App.GetService<LocalizationService>();
        model.WindowCloseRequested += (s, e) => Close();
        DataContext = model;
    }

    // Avatar change button click handler - wire this up in XAML
    public async void OnChangeAvatarClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (GeneralSettings == null || _localizationService == null)
            return;
            
        GeneralSettings.ResetAvatarMessage();
            
        // Create storage provider
        var storageProvider = StorageProvider;
        if (storageProvider == null)
            return;
            
        // Set up file picker options
        var options = new FilePickerOpenOptions
        {
            Title = _localizationService["settings.general.avatar.change"],
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType(_localizationService["settings.general.avatar.file.types"])
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" },
                    MimeTypes = new[] { "image/jpeg", "image/png", "image/bmp" }
                }
            }
        };
        
        // Show file picker
        var files = await storageProvider.OpenFilePickerAsync(options);
        if (files.Count == 0)
            return;
            
        var file = files[0];
        
        try
        {
            // Check file size
            var fileInfo = await file.GetBasicPropertiesAsync();
            if (fileInfo.Size > 64 * 1024 * 1024) // 64MB limit
            {
                // Show error message in UI
                GeneralSettings.ShowAvatarSizeError();
                return;
            }
            
            // Create avatar directory
            var configService = App.GetService<ConfigureService>();
            var avatarDir = ConfigureService.GetAvatarDirectory();
            if (!Directory.Exists(avatarDir))
                Directory.CreateDirectory(avatarDir);
                
            // Generate target file path
            var extension = Path.GetExtension(file.Name);
            var avatarFileName = $"avatar_{Guid.NewGuid()}{extension}";
            var avatarPath = Path.Combine(avatarDir, avatarFileName);
            
            // Copy the file
            await using (var sourceStream = await file.OpenReadAsync())
            await using (var destinationStream = File.Create(avatarPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
            
            // Update the avatar in the view model
            await GeneralSettings.UpdateAvatarAsync(avatarPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling avatar: {ex.Message}");
        }
    }
    
    // Avatar remove button click handler
    public async void OnRemoveAvatarClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (GeneralSettings == null)
            return;
            
        var configService = App.GetService<ConfigureService>();
        // Get current avatar path
        var oldPath = configService.AppConfig.AvatarFilePath;
        
        // Update the view model
        await GeneralSettings.UpdateAvatarAsync(null);
        
        // Delete the old avatar file if it exists
        if (!string.IsNullOrEmpty(oldPath) && File.Exists(oldPath))
        {
            try
            {
                File.Delete(oldPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting avatar file: {ex.Message}");
            }
        }
        
        await ConfigureService.ClearAvatars(null);
    }
}
