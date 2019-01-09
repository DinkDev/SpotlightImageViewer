namespace SpotlightImageViewer.ViewModels
{
    using System;
    using System.Windows.Media.Imaging;
    using Atalasoft.Imaging;
    using Atalasoft.Imaging.Wpf;
    using Caliburn.Micro;

    public class ImageButtonViewModel
    {
        private Action<string> DisplayCallback { get; }

        public ImageButtonViewModel(AtalaImage image, string path, Action<string> displayCallback)
        {
            if (image == null)
    throw new ArgumentNullException(nameof(image));

            Image = WpfConverter.AtalaImageToBitmapSource(image);

    Path = path ?? throw new ArgumentNullException(nameof(path));
            DisplayCallback = displayCallback ?? throw new ArgumentNullException(nameof(displayCallback));
        }

        public BitmapSource Image { get; }
        public string Path { get; }

        public void Clicked()
        {
            try
            {
                Execute.OnUIThread(() => DisplayCallback(Path));
            }
            catch (Exception)
            {
                // swallow
            }
        }
    }
}
