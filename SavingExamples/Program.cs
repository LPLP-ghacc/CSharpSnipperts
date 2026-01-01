using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SavingExamples;

// A simple implementation of the automatic settings saving system:
// whenever a property is changed (via INotifyPropertyChanged), the settings are immediately serialized into a JSON file.
// This ensures that the current values are preserved even in the event of an emergency program shutdown.
public class Settings : INotifyPropertyChanged
{
    public bool FieldOne
    {
        get;
        set => Set(ref field, value);
    }
    
    public bool FieldTwo
    {
        get;
        set => Set(ref field, value);
    }
    
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    public async Task SaveAsync(string path)
    {
        var json = JsonSerializer.Serialize(this, Options);
        await File.WriteAllTextAsync(path, json);
    }

    public override string ToString()
    {
        var result = string.Empty;
        var props = typeof(Settings).GetProperties();

        return props.Aggregate(result, (current, propertyInfo) => current + $"{propertyInfo.Name} - {propertyInfo.GetValue(this)}\n");
    }
}
    
public class Program
{
    public static async Task Main()
    {
        var settings = new Settings();

        settings.PropertyChanged += async (_, _) =>
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            await settings.SaveAsync(path);
            
            Console.WriteLine($"Settings saved to {path}\n{settings}");
        };
        
        settings.FieldOne = false;
        
        await Task.Delay(100);
        
        settings.FieldTwo = true;
        
        await Task.Delay(100);
    }
}