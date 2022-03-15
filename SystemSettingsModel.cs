using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;

namespace oem_logo;

// Used for import / export
public record SettingsObject
{
    public string Manufacturer { get; init; }
    public string Model { get; init; }
    public string SupportHours { get; init; }
    public string SupportPhone { get; init; }
    public string SupportUrl { get; init; }
    public string OemIconImage { get; init; }
}

public class SystemSettingsModel
{
    private const string KeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation";
    private static readonly SystemSettingsModel _systemSettingsModel = new();

    private ICommand _setCommand;

    private ICommand _undoFieldsCommand;

    private SystemSettingsModel()
    {
        FieldPropList = new[] {Manufacturer, Model, SupportHours, SupportPhone, SupportUrl};
        PropList = new[] {OemIcon, Manufacturer, Model, SupportHours, SupportPhone, SupportUrl};
    }

    public ICommand SetCommand
    {
        get
        {
            if (_setCommand != null) return _setCommand;
            _setCommand = new RelayCommand(_ =>
            {
                foreach (var prop in PropList) prop.SetToLocal();
            }, () => PropList.Any(x => x.Changed));
            return _setCommand;
        }
    }

    public ICommand UndoFieldsCommand
    {
        get
        {
            if (_undoFieldsCommand != null) return _undoFieldsCommand;
            _undoFieldsCommand = new RelayCommand(_ =>
            {
                foreach (var prop in FieldPropList) prop.ResetCommand.Execute(null);
            }, () => PropList.Any(x => x.Changed));
            return _undoFieldsCommand;
        }
    }

    public ImageProp OemIcon { get; } = new("Logo");
    public Prop Manufacturer { get; } = new("Manufacturer");
    public Prop Model { get; } = new("Model");
    public Prop SupportHours { get; } = new("SupportHours");
    public Prop SupportPhone { get; } = new("SupportPhone");
    public Prop SupportUrl { get; } = new("SupportURL");
    public Prop[] FieldPropList { get; }
    public Prop[] PropList { get; }

    public static SystemSettingsModel Instance()
    {
        return _systemSettingsModel;
    }

    public class ImageProp : Prop
    {
        public ImageProp(string valueName) : base(valueName)
        {
        }

        public string Filename => new FileInfo(LocalValue).Name;
    }

    public class Prop : INotifyPropertyChanged
    {
        private string _localValue;

        private ICommand _resetCommand;

        public Prop(string valueName)
        {
            ValueName = valueName;
            LocalValue = SystemValue;
        }

        public string ValueName { get; }

        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand != null) return _resetCommand;
                _resetCommand = new RelayCommand(_ => { LocalValue = SystemValue; }, () => Changed);

                return _resetCommand;
            }
        }

        public string SystemValue
        {
            get => Registry.GetValue(KeyName, ValueName, "") as string;
            set
            {
                Registry.SetValue(KeyName, ValueName, value);
                OnPropertyChanged(nameof(Changed));
                OnPropertyChanged(nameof(SystemValue));
            }
        }

        public bool Changed => !SystemValue.Equals(LocalValue);

        public string LocalValue
        {
            get => _localValue;
            set
            {
                _localValue = value;
                OnPropertyChanged(nameof(LocalValue));
                OnPropertyChanged(nameof(Changed));
                OnPropertyChanged("CanSet");

                // TODO: Move to ImageProp definition
                OnPropertyChanged("Filename");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetToLocal()
        {
            SystemValue = LocalValue;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}