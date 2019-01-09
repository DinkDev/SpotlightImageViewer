namespace SpotlightImageViewer.Views
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using Atalasoft.Annotate.Wpf;
    using Atalasoft.Imaging.Wpf;

    /// <summary>
    /// Interaction logic for AtalaImageViewer.xaml
    /// </summary>
    public partial class AtalaImageViewer : UserControl
    {
        public AtalaImageViewer()
        {
            InitializeComponent();
        }


        #region AnnotationsList DependencyProperty

        public IList AnnotationsList
        {
            get => (IList)GetValue(AnnotationsListProperty);
            set => SetValue(AnnotationsListProperty, value);
        }

        public static readonly DependencyProperty AnnotationsListProperty
            = DependencyProperty.Register(nameof(AnnotationsList), typeof(IList),
                typeof(AtalaImageViewer), new PropertyMetadata(OnAnnotationsListChanged));

        /// <summary>
        /// Rewire the CollectionChanged notification, if applicable.
        /// </summary>
        private static void OnAnnotationsListChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is AtalaImageViewer viewer)
            {
                if (e.OldValue is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= viewer.OnAnnotationsList_CollectionChanged;
                }

                if (e.NewValue is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += viewer.OnAnnotationsList_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Respond to CollectionChanged events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnnotationsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var layer = AnnotationViewer.Annotations.Layers.FirstOrDefault();
            if (layer == null)
            {
                layer = new WpfLayerAnnotation();
                AnnotationViewer.Annotations.Layers.Add(layer);
            }

            var newAnnotations = AnnotationsList as IList<WpfAnnotationUI> ?? new List<WpfAnnotationUI>();

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                layer.Items.Clear();
                layer.Items.AddRange(newAnnotations);
            }
            else
            {
                // remove annotations no longer in AnnotationsList
                foreach (var extra in layer.Items)
                {
                    if (!newAnnotations.Contains(extra))
                    {
                        layer.Items.Remove(extra);
                    }
                }

                // add new annotations in AnnotationsList
                foreach (var added in newAnnotations)
                {
                    if (!layer.Items.Contains(added))
                    {
                        layer.Items.Add(added);
                    }
                }
            }
        }

        #endregion

        #region ImageSource DependencyProperty

        /// <summary>
        /// ImageSource DependencyProperty - the background image being annotated.
        /// </summary>
        /// <remarks>
        /// Maps a BitmapSource to AnnotationViewer.ImageViewer.Image.
        /// </remarks>
        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(AtalaImageViewer),
                new PropertyMetadata(OnImageSourceChanged));

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AtalaImageViewer viewer
                && e.NewValue is BitmapSource newImage)
            {
                viewer.AnnotationViewer.ImageViewer.Image = WpfConverter.BitmapSourceToAtalaImage(newImage);
            }
        }

        #endregion

        #region Zoom DependencyProperty

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        // Using a DependencyProperty as the backing store for Zoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(AtalaImageViewer), new PropertyMetadata(ZoomChanged));

        private static void ZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AtalaImageViewer viewer)
            {
                viewer.AnnotationViewer.ImageViewer.Zoom = viewer.Zoom;
            }
        }

        #endregion
    }
}
