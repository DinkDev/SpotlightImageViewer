namespace SpotlightImageViewer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using Atalasoft.Annotate.Wpf;
    using Atalasoft.Imaging;
    using Atalasoft.Imaging.Codec;
    using Atalasoft.Imaging.Wpf;
    using Caliburn.Micro;
    using Microsoft.WindowsAPICodePack.Dialogs;

    public sealed class ShellViewModel : Screen, IShell
    {
        private BitmapSource _displayedImage;
        private double _zoom;
        private string _displayedImageFilename;
        private bool _canSave;

        public ShellViewModel()
        {
            DisplayName = @"Spotlight Image Viewer";

            Zoom = 0.3;
        }

        public BindableCollection<ImageButtonViewModel> Images { get; } = new BindableCollection<ImageButtonViewModel>();

        protected override void OnViewReady(object view)
        {
            LoadAsync();

            base.OnViewReady(view);
        }

        public bool CanSave
        {
            get => _canSave;
            set
            {
                if (Equals(value, _canSave))
                    return;
                _canSave = value;
                NotifyOfPropertyChange();
            }
        }

        public void Save()
        {
            try
            {
                var image = new AtalaImage(DisplayedImageFilename);
                var saveDialog = new CommonSaveFileDialog();
                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                saveDialog.Filters.Add(new CommonFileDialogFilter(@"JPEG Files", "jpg, jpeg" ));
                saveDialog.DefaultExtension = "jpg";

                if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var saveFile = saveDialog.FileName;

                    using (var fileStream = new FileStream(saveFile, FileMode.Create))
                    {
                        var encoder = new JpegEncoder();
                        
                        encoder.Save(fileStream, image, null);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async void LoadAsync()
        {
            var screen = this;

            await Task.Run(() =>
            {
                Execute.OnUIThreadAsync(() => Images.Clear());

                var spotlightImagePath =
                    $@"C:\Users\{Environment.UserName}\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";

                var potentialImageFiles = Directory.Exists(spotlightImagePath)
                    ? Directory.GetFiles(spotlightImagePath).ToList()
                    : new List<string>();

                var currentWallpaper = $@"C:\Users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Themes";
                if (File.Exists(currentWallpaper))
                {
                    potentialImageFiles.Add(currentWallpaper);
                }

                foreach (var file in potentialImageFiles)
                {
                    try
                    {
                        var image = new AtalaImage(file);

                        if (image.Height >= 500 && image.Width >= 500)
                        {
                            Execute.OnUIThreadAsync(() =>
                                Images.Add(new ImageButtonViewModel(image, file, img => DisplayFile(img))));
                        }
                    }
                    catch (Exception e)
                    {
                        var error = e;
                        // swallow
                    }
                }

                Execute.OnUIThreadAsync(() => screen.CanSave = true);
            });
        }

        public void DisplayFile(string file)
        {
            try
            {
                var image = new AtalaImage(file);

                DisplayedImage = WpfConverter.AtalaImageToBitmapSource(image);
                DisplayedImageFilename = file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #region Annotation Viewer bound Properties

        public BindableCollection<WpfAnnotationUI> ImageAnnotations { get; } =
            new BindableCollection<WpfAnnotationUI>();

        public BitmapSource DisplayedImage
        {
            get => _displayedImage;
            set
            {
                if (Equals(value, _displayedImage))
                    return;
                _displayedImage = value;
                NotifyOfPropertyChange();
            }
        }

        public string DisplayedImageFilename
        {
            get => _displayedImageFilename;
            set
            {
                if (Equals(value, _displayedImageFilename))
                    return;
                _displayedImageFilename = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Mouse Zoom support
        public void MouseZoom(object sender, MouseWheelEventArgs e)
        {
            if (DisplayedImage != null
                && (Keyboard.IsKeyDown(Key.LeftCtrl)
                    || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (e.Delta != 0)
                {
                    if (e.Delta > 0)
                    {
                        Zoom += (0.1 * Zoom);
                        if (Zoom > 10.0)
                        {
                            Zoom = 10.0;
                        }
                    }
                    else
                    {
                        Zoom -= (0.1 * Zoom);
                        if (Zoom < 0.10)
                        {
                            Zoom = 0.10;
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value.Equals(_zoom))
                    return;
                _zoom = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion
    }
}