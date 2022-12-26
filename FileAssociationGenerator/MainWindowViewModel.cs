using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace FileAssociationGenerator;

internal class MainWindowViewModel : ObservableObject
{
    ObservableCollection<string>? _itemsLeft;
    ObservableCollection<string>? _itemsRight;
    List<string>? _selectedItemsLeft;
    List<string>? _selectedItemsRight;
    string? _leftFilter;
    string? _rightFilter;
    readonly List<Association> association = new();
    bool _syncFilterCheckbox;
    bool _rightFilterEnabled;

    public ObservableCollection<string>? ItemsLeft {
        get => _itemsLeft;
        set => SetProperty(ref _itemsLeft, value);
    }

    public ObservableCollection<string>? ItemsLeftCache { get; set; }

    public ObservableCollection<string>? ItemsRight
    {
        get => _itemsRight;
        set => SetProperty(ref _itemsRight, value);
    }

    public ObservableCollection<string>? ItemsRightCache { get; set; }

    public List<string>? SelectedItemsLeft
    {
        get => _selectedItemsLeft;
        set => SetProperty(ref _selectedItemsLeft, value);
    }

    public List<string>? SelectedItemsRight
    {
        get => _selectedItemsRight;
        set => SetProperty(ref _selectedItemsRight, value);
    }

    public string? LeftFilter
    {
        get => _leftFilter;
        set {
            SetProperty(ref _leftFilter, value);

            if (SyncFilterCheckbox)
            {
                SetProperty(ref _rightFilter, value);
                FilterRight();
            }

            FilterLeft();
        }
    }

    public string? RightFilter
    {
        get => _rightFilter;
        set {
            SetProperty(ref _rightFilter, value);

            if (SyncFilterCheckbox)
            {
                SetProperty(ref _leftFilter, value);
                FilterLeft();
            }

            FilterRight();
        }
    }

    public bool RightFilterEnabled
    {
        get => _rightFilterEnabled;
        set => SetProperty(ref _rightFilterEnabled, value);
    }

    public bool SyncFilterCheckbox
    {
        get => _syncFilterCheckbox;
        set {
            SetProperty(ref _syncFilterCheckbox, value);

            if (_syncFilterCheckbox)
            {
                RightFilterEnabled = false;
            } 
            else
            {
                RightFilterEnabled = true;
            }
        }
    }

    public IRelayCommand SelectLeftDirectoryButton { get; }
    public IRelayCommand SelectRightDirectoryButton { get; }
    public IRelayCommand AssociateButton { get; }
    public IRelayCommand SetOutputFileButton { get; }
    public IRelayCommand SetLeftItemsButton { get; }
    public IRelayCommand SetRightItemsButton { get; }

    private string? _outputFilePath;

    public MainWindowViewModel()
    {
        SelectLeftDirectoryButton = new RelayCommand(SelectLeftDirectory);
        SelectRightDirectoryButton = new RelayCommand(SelectRightDirectory);
        AssociateButton = new RelayCommand(Associate);
        SetOutputFileButton = new RelayCommand(SetOutputFile);
        RightFilterEnabled = true;
        SetLeftItemsButton = new RelayCommand<object>(SetLeftItems);
        SetRightItemsButton = new RelayCommand<object>(SetRightItems);
    }

    public void SelectLeftDirectory()
    {
        using FolderBrowserDialog dialog = new();

        dialog.Description = "Select Left Directory";

        var result = dialog.ShowDialog();

        var inputDirectory = "";

        if (result != DialogResult.OK) return;

        inputDirectory = dialog.SelectedPath.Replace("\\", "/");

        ItemsLeft = GetFiles(inputDirectory);
        ItemsLeftCache = ItemsLeft;
    }

    public void SelectRightDirectory()
    {
        using FolderBrowserDialog dialog = new();

        dialog.Description = "Select Right Directory";

        var result = dialog.ShowDialog();

        var inputDirectory = "";

        if (result != DialogResult.OK) return;

        inputDirectory = dialog.SelectedPath.Replace("\\", "/");

        ItemsRight = GetFiles(inputDirectory);
        ItemsRightCache = ItemsRight;
    }

    public void SetOutputFile()
    {
        var dialog = new OpenFileDialog();

        if (dialog.ShowDialog() != DialogResult.OK) return;

        _outputFilePath = dialog.FileName;
    }

    public void Associate()
    {
        if (SelectedItemsLeft is null)
        {
            MessageBox.Show("No left selected items!");
            return;
        }

        if (SelectedItemsRight is null)
        {
            MessageBox.Show("No right selected items!");
            return;
        }

        if (SelectedItemsLeft.Count != SelectedItemsRight.Count)
        {
            MessageBox.Show("Left selections is not equal to right selections!");
            return;
        }

        for (int i = 0; i < SelectedItemsLeft.Count; i++)
        {
            association.Add(new Association
            {
                SourceFile = SelectedItemsLeft[i],
                AssociatedFile= SelectedItemsRight[i]
            });
        }

        if (_outputFilePath is null) return;

        File.WriteAllText(_outputFilePath, JsonSerializer.Serialize(association, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static ObservableCollection<string> GetFiles(string directory)
    {
        ObservableCollection<string> files = new();

        foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            files.Add(Path.GetFileName(file));
        }

        return files;
    }

    public void FilterLeft()
    {
        if (LeftFilter is null) return;

        ItemsLeft = ItemsLeftCache;

        if (ItemsLeft is null) return;

        ItemsLeft = new ObservableCollection<string>(ItemsLeft.Where(a => a.Contains(LeftFilter)).OrderBy(a => a));
    }

    public void FilterRight()
    {
        if (RightFilter is null) return;

        ItemsRight = ItemsRightCache;

        if (ItemsRight is null) return;

        ItemsRight = new ObservableCollection<string>(ItemsRight.Where(a => a.Contains(RightFilter)).OrderBy(a => a));
    }

    public void SetLeftItems(object? data)
    {
        if (data is null) return;

        System.Collections.IList listObject = (System.Collections.IList)data;

        List<string> strings = new();

        foreach (var item in listObject)
        {
            strings.Add((string)item);
        }

        SelectedItemsLeft = strings;
    }

    public void SetRightItems(object? data)
    {
        if (data is null) return;

        System.Collections.IList listObject = (System.Collections.IList)data;

        List<string> strings = new();

        foreach (var item in listObject)
        {
            strings.Add((string)item);
        }

        SelectedItemsRight = strings;
    }
}
