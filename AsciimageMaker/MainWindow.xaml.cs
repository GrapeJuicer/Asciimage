using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
// include Asciimage libraries
using Asciimage.Brushes;
using Asciimage.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using Microsoft.UI.Xaml.Documents;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AsciimageMaker
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private GridSegmentedFontBrush FontBrushInstance;
        private SegmentCount[] SegmentCountGroup = [
            SegmentCount.OneByOne,
            SegmentCount.TwoByOne,
            SegmentCount.TwoByTwo,
            SegmentCount.FourByTwo,
            SegmentCount.FourByFour
        ];

        private string CachedUsingCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+-,.:;<=>?@[/\\] ^_`{|}~";
        private int CachedColorModeIndex = 1; // Grayscale
        private int CachedSegmentCountIndex = 0; // OneByOne
        private string CachedFontFamilyName = "Cascadia Code";

        private string? FilePath;
        private AsciiMat? GeneratedAsciiMat;

        public MainWindow()
        {
            this.InitializeComponent();
            
            UsingCharactersTextBox.Text = CachedUsingCharacters;
            UsingCharactersTextBox.PlaceholderText = CachedUsingCharacters;

            FontBrushInstance = new GridSegmentedFontBrush(
                FontFamilyNameTextBox.Text,
                CachedUsingCharacters.ToList(),
                SegmentCountGroup
            );
        }
        private async void InputImageSelectButton_Click(object _, RoutedEventArgs __)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();
            if (file is not null)
            {
                FilePath = file.Path;
                using var stream = await file.OpenAsync(FileAccessMode.Read);
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                DispatcherQueue.TryEnqueue(() =>
                {
                    InputImage.Source = bitmapImage;
                    OpenedFileName.Text = file.Name;
                });
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FontBrushInstance is null || IsChangedBrushConfig())
                {
                    // re-generate brush
                    FontBrushInstance = new GridSegmentedFontBrush(
                        FontFamilyNameTextBox.Text,
                        UsingCharactersTextBox.Text.ToList(),
                        SegmentCountGroup
                    );
                }

                SegmentCount segmentCount = GetSelectedSegmentCount();
                ColorMode colorMode = (ColorMode)((ColorModeComboBox.SelectedItem as ComboBoxItem)?.Tag ?? throw new Exception("Unknown Color Mode."));

                using var stream = File.OpenRead(FilePath ?? throw new Exception("No file path is specified."));
                SKBitmap bitmap = SKBitmap.Decode(stream);

                int w = (int)WidthSlider.Value;
                int h = (int)HeightSlider.Value;

                // generate ascii image
                GeneratedAsciiMat = Asciimage.Core.Asciimage.Generate(
                    bitmap,
                    FontBrushInstance,
                    segmentCount,
                    new AsciimageConfig(width: w == 0 ? -1 : w, height: h == 0 ? -1 : h, colorMode: colorMode)
                );

                // update caches
                CachedUsingCharacters = UsingCharactersTextBox.Text;
                CachedColorModeIndex = ColorModeComboBox.SelectedIndex;
                CachedSegmentCountIndex = SegmentCountComboBox.SelectedIndex;
                CachedFontFamilyName = FontFamilyNameTextBox.Text;

                // display result

                GeneratedRichTextBox.FontFamily = new FontFamily(FontFamilyNameTextBox.Text);
                GeneratedRichTextBox.Text = GeneratedAsciiMat.ToString();
                GeneratedRichTextBox.UpdateLayout();
                GeneratedRichTextBox.Width = GeneratedRichTextBox.ActualHeight * GeneratedAsciiMat.Width * FontBrushInstance.CharacterRatio.Width / (FontBrushInstance.CharacterRatio.Height * GeneratedAsciiMat.Height);
                OutputInfoTextBlock.Text = "Success";
            }
            catch (Exception ex)
            {
                GeneratedRichTextBox.FontFamily = new FontFamily("Cascadia Code");
                GeneratedRichTextBox.Text = null;
                GeneratedRichTextBox.Width = 0;
                GeneratedAsciiMat = null;
                OutputInfoTextBlock.Text = $"Error : {ex.Message}";
            }

            CopyButton.IsEnabled = GeneratedAsciiMat is not null;
        }

        private bool IsChangedBrushConfig()
        {
            return CachedUsingCharacters != UsingCharactersTextBox.Text ||
                CachedFontFamilyName != FontFamilyNameTextBox.Text;
        }

        private SegmentCount GetSelectedSegmentCount()
        {
            return (SegmentCountComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() switch
            {
                "1;1" => SegmentCount.OneByOne,
                "2;1" => SegmentCount.TwoByOne,
                "2;2" => SegmentCount.TwoByTwo,
                "4;2" => SegmentCount.FourByTwo,
                "4;4" => SegmentCount.FourByFour,
                _ => throw new ArgumentException("Invalid segment count")
            };
        }

        private void CopyResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (GeneratedAsciiMat is not null)
            {
                var package = new DataPackage();
                package.SetText(GeneratedAsciiMat.ToString());
                Clipboard.SetContent(package);
            }
        }

        private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                var otherSlider = slider == WidthSlider ? HeightSlider : WidthSlider;
                if (slider.Value == 0)
                {
                    otherSlider.Value = 20;
                }
            }
        }
    }
}
