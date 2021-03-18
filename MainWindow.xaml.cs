using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace oem_logo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine(SystemSettings.OemIcon);
            Console.WriteLine(SystemSettings.OemIcon);
        }

        private void IconSelect_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != true) return;
            var file = new FileInfo(dialog.FileName);
            HandleFile(file);
        }

        private void HandleFile(FileInfo file)
        {
            Status.Content = file.Name;
            var extension = file.Extension.ToLower();

            if (extension == ".bmp" || extension == ".png") // Bitmap image
            {
                var image = new Image {Source = new BitmapImage(new Uri(file.FullName))};
                IconSelect.Content = image;
                IconSetButton.IsEnabled = true;
            }
        }

        private void IconSelect_OnDragEnter(object sender, DragEventArgs e)
        {
            IconSelect.Background = new SolidColorBrush(Colors.Bisque);
        }

        private void IconSelect_OnDragLeave(object sender, DragEventArgs e)
        {
            IconSelect.Background = new SolidColorBrush(Colors.Azure);
        }

        private void IconSelect_OnDragOver(object sender, DragEventArgs e)
        {
        }

        private void IconSelect_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (data?.Length > 1) // Multiple files dragged
                {
                    var error = MessageBox.Show("You can only drag one file.", "Error");
                }

                var file = new FileInfo(data[0]);
                HandleFile(file);
            }
        }

        private void IconSetButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}