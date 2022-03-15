using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Size = System.Drawing.Size;

namespace oem_logo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        private ICommand _openPanelCommand;

        private ICommand _openRegistryCommand;

        private ICommand _undoImageCommand;

        public MainWindow()
        {
            // TODO: Add a way to interact from the command line with arguments
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            InitializeComponent();
        }

        public ICommand OpenRegistryCommand
        {
            get
            {
                if (_openRegistryCommand != null) return _openRegistryCommand;
                _openRegistryCommand = new RelayCommand(_ =>
                {
                    Registry.SetValue(
                        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Applets\Regedit",
                        "LastKey",
                        @"Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation");
                    Process.Start("regedit.exe");
                });
                return _openRegistryCommand;
            }
        }

        public ICommand OpenPanelCommand
        {
            get
            {
                if (_openPanelCommand != null) return _openPanelCommand;
                _openPanelCommand = new RelayCommand(_ =>
                {
                    Process.Start("explorer", @"shell:::{BB06C0E4-D293-4f75-8A90-CB05B6477EEE}");
                });

                return _openPanelCommand;
            }
        }

        public ICommand UndoImageCommand
        {
            get
            {
                if (_undoImageCommand != null) return _undoImageCommand;
                _undoImageCommand = new RelayCommand(_ => { WindowModel.OemIcon.ResetCommand.Execute(null); },
                    () => WindowModel.OemIcon.Changed);

                return _undoImageCommand;
            }
        }

        public SystemSettingsModel WindowModel { get; } = SystemSettingsModel.Instance();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void IconSelect_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter =
                    "Image files (*.bmp;*.jpg;*.jpeg;*.png;)|*.bmp;*.BMP;*.jpg;*.JPG;*.jpeg;*.jpeg;*.png;*.PNG",
                Title = "Select image",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true) return;
            var file = new FileInfo(dialog.FileName);
            HandleFile(file);
        }

        private void HandleFile(FileInfo file)
        {
            var programFilesPath = @"C:\Program Files\OEMChanger";
            if (!Directory.Exists(programFilesPath)) Directory.CreateDirectory(programFilesPath);

            var extension = file.Extension.ToLower();

            string[] supportedFormats = {".bmp", ".png", ".jpg", ".jpeg"};
            if (!supportedFormats.Contains(extension))
            {
                var formatText = string.Join(", ", supportedFormats);
                MessageBox.Show($"Image has to be one of the following formats: {formatText}", "Image format error");
                return;
            }

            var inputFile = file.FullName;

            string UniquePath(int i = 0)
            {
                var path = Path.Combine(programFilesPath, $"logo{i}.bmp");
                if (File.Exists(path))
                {
                    return UniquePath(i + 1);
                }

                return path;
            }

            var outputFile = UniquePath();
            //if (File.Exists(outputFile)) File.Delete(outputFile);

            var bitmap = new Bitmap(inputFile);
            bitmap = new Bitmap(bitmap, new Size(120, 120));
            bitmap.Save(outputFile, ImageFormat.Bmp);

            WindowModel.OemIcon.LocalValue = outputFile;
        }

        private void Import_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open backup",
                DefaultExt = "bak",
                Filter = "backup files (*.bak)|*.bak|All files (*.*)|*.*",
                CheckPathExists = true,
                CheckFileExists = true
            };

            var result = dialog.ShowDialog();
            if (result != true) return;

            var inFile = dialog.FileName;
            var json = File.ReadAllText(inFile);

            SettingsObject data;
            try
            {
                data = JsonSerializer.Deserialize<SettingsObject>(json);
            }
            catch
            {
                MessageBox.Show("Not a valid backup file.", "Backup format error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            WindowModel.Manufacturer.LocalValue = data?.Manufacturer;
            WindowModel.Model.LocalValue = data?.Model;
            WindowModel.OemIcon.LocalValue = data?.OemIconPath;
            WindowModel.SupportHours.LocalValue = data?.SupportHours;
            WindowModel.SupportPhone.LocalValue = data?.SupportPhone;
            WindowModel.SupportUrl.LocalValue = data?.SupportUrl;
        }

        private void Export_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                FileName = "oem.bak",
                Title = "Save backup",
                DefaultExt = "bak",
                Filter = "backup files (*.bak)|*.bak|All files (*.*)|*.*",
                CheckPathExists = true
            };
            var result = dialog.ShowDialog();
            if (result != true) return;
            var outPath = dialog.FileName;

            SaveSettingsFile(outPath);
        }

        private void SaveSettingsFile(string outPath)
        {
            var data = new SettingsObject
            {
                OemIconPath = WindowModel.OemIcon.LocalValue,
                Manufacturer = WindowModel.Manufacturer.LocalValue,
                Model = WindowModel.Model.LocalValue,
                SupportHours = WindowModel.SupportHours.LocalValue,
                SupportPhone = WindowModel.SupportPhone.LocalValue,
                SupportUrl = WindowModel.SupportUrl.LocalValue
            };
            var json = JsonSerializer.Serialize(data, _jsonSerializerOptions);

            File.WriteAllText(outPath, json);
        }
    }
}