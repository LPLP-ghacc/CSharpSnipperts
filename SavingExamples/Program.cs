using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SavingExamples;

public abstract class ObservableBase : INotifyPropertyChanged
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected ObservableBase(string path)
    {
        _path = path;
        PropertyChanged += OnPropertyChanged;
    }

    protected void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            var json = JsonSerializer.Serialize(this, Options);
            await File.WriteAllTextAsync(_path, json);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
}

public class Settings(string path) : ObservableBase(path)
{
    private bool _fieldOne;
    public bool FieldOne
    {
        get => _fieldOne;
        set => Set(ref _fieldOne, value);
    }

    private bool _fieldTwo;
    public bool FieldTwo
    {
        get => _fieldTwo;
        set => Set(ref _fieldTwo, value);
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
        var path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        var settings = new Settings(path);
        settings.PropertyChanged += (sender, args) =>
        {
            Console.WriteLine(args.PropertyName + " property was changed.");
        };
        
        settings.FieldOne = false;

        await Task.Delay(100);

        settings.FieldTwo = true;

        await Task.Delay(100);
    }
}