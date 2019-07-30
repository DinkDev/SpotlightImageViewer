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
    using Properties;

    public sealed class ShellViewModel : Screen, IShell
    {
        private BitmapSource _displayedImage;
        private double _zoom;
        private string _displayedImageFilename;
        private bool _canSave;
        private string _statusMessage;

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

                var intialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (!string.IsNullOrWhiteSpace(Settings.Default.MyPicturesSubdir))
                {
                    intialDirectory = Path.Combine(intialDirectory, Settings.Default.MyPicturesSubdir);
                    if (!Directory.Exists(intialDirectory))
                    {
                        Directory.CreateDirectory(intialDirectory);
                    }
                }

                saveDialog.InitialDirectory = intialDirectory;
                saveDialog.Filters.Add(new CommonFileDialogFilter(@"JPEG Files", "jpg, jpeg"));
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

                var dynamicThemeImagePath1 =
                    $@"C:\Users\{Environment.UserName}\AppData\Local\Packages\55888ChristopheLavalle.DynamicTheme_jdggxwd41xcr0\LocalState\Bing";

                var dynamicImageFiles1 = Directory.Exists(dynamicThemeImagePath1)
                    ? Directory.GetFiles(dynamicThemeImagePath1).ToList()
                    : new List<string>();

                potentialImageFiles.AddRange(dynamicImageFiles1);

                var dynamicThemeImagePath2 =
                    $@"C:\Users\{Environment.UserName}\AppData\Local\Packages\55888ChristopheLavalle.DynamicTheme_jdggxwd41xcr0\LocalState\WinSpotlight";

                var dynamicImageFiles2 = Directory.Exists(dynamicThemeImagePath2)
                    ? Directory.GetFiles(dynamicThemeImagePath2).ToList()
                    : new List<string>();

                potentialImageFiles.AddRange(dynamicImageFiles2);


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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (Equals(value, _statusMessage))
                    return;
                _statusMessage = value;
                NotifyOfPropertyChange();
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

                SetStatusMessage();

                NotifyOfPropertyChange();
            }
        }

        private void SetStatusMessage()
        {
            var filename = Path.GetFileName(_displayedImageFilename);

            StatusMessage =
                $"Image: {filename}, Resolution: {_displayedImage.PixelHeight} X {_displayedImage.PixelHeight}";
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
