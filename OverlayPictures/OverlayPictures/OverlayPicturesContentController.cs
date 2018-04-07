using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace OverlayPictures
{
    public static class OverlayPicturesContentController
    {
        public static string[] Extensions = new string[]
        {
            @"jpg",
            @"jpeg",
            @"png",
            @"bmp"
        };

        public static string ExtensionDescription = "Image File";
        public const int DefaultTimeSpan = 30;

        private static string[] PicPaths { get; set; } = new string[0];
        private static string PicPathNow { get; set; } = string.Empty;
        private static List<string> NotShowedPicPaths { get; } = new List<string>();
        private static Timer PicChangeTimer { get; } = new Timer();

        public static class AppConfigItemsContainer
        {
            public static bool IsOpenConfigAtStart { get; set; } = true;
            public static bool IsShowBySlideShow { get; set; } = false;
            public static bool IsEnlargePictureToLimit { get; set; } = true;
            public static bool IsUseHotKey { get; set; } = false;
            public static string PicFolderDirectory { get; set; } = string.Empty;
            public static string PicPath { get; set; } = string.Empty;
            public static int PositionX { get; set; } = Screen.PrimaryScreen.Bounds.Width / 8 * 5;
            public static int PositionY { get; set; } = Screen.PrimaryScreen.Bounds.Height / 8 * 3;
            public static int SizeLimitX { get; set; } = Screen.PrimaryScreen.Bounds.Width / 8 * 3;
            public static int SizeLimitY { get; set; } = Screen.PrimaryScreen.Bounds.Height / 8 * 5;
            public static int Opacity { get; set; } = 80;
            public static int TimeSpan { get; set; } = 30;

            public static HotKeyConfigContainer MinHotKeyConfigContainer { get; set; } = new HotKeyConfigContainer('M', Keys.M);
            public static HotKeyConfigContainer OpenConfigHotKeyConfigContainer { get; set; } = new HotKeyConfigContainer('O', Keys.O);

            public class HotKeyConfigContainer
            {
                public bool IsUseCtrl { get; set; } = false;
                public bool IsUseShift { get; set; } = false;
                public bool IsUseAlt { get; set; } = true;
                public char KeyChar { get; private set; }
                public Keys Key { get; private set; }

                public HotKeyConfigContainer(char keyChar = '0', Keys key = Keys.D0)
                {
                    KeyChar = keyChar;
                    Key = key;
                }
            }
        }

        private static MainWindow _mainWindow;

        public static void SetMainWindow(MainWindow window)
        {
            _mainWindow = window;
        }

        public static void SetSinglePicture()
        {
            SetPicture(AppConfigItemsContainer.PicPath, AppConfigItemsContainer.PositionX, AppConfigItemsContainer.PositionY,
                AppConfigItemsContainer.IsEnlargePictureToLimit, AppConfigItemsContainer.SizeLimitX,
                AppConfigItemsContainer.SizeLimitY, (double)AppConfigItemsContainer.Opacity / 100);
        }

        public static void SetPictureAgain()
        {
            SetPicture(PicPathNow, AppConfigItemsContainer.PositionX, AppConfigItemsContainer.PositionY,
                AppConfigItemsContainer.IsEnlargePictureToLimit, AppConfigItemsContainer.SizeLimitX,
                AppConfigItemsContainer.SizeLimitY, (double)AppConfigItemsContainer.Opacity / 100);
        }

        public static string GetActivePictureName()
        {
            return PicPathNow == string.Empty ? string.Empty : Path.GetFileName(PicPathNow);
        }

        public static void ActivateWindow()
        {
            _mainWindow?.Activate();
        }

        public static void StartSlideShow()
        {
            StopTimer();

            if (!Directory.Exists(AppConfigItemsContainer.PicFolderDirectory))
                return;

            PicPaths = FileController.GetFilePaths(AppConfigItemsContainer.PicFolderDirectory, Extensions);

            if (PicPaths.Length == 0)
            {
                return;
            }

            ResetNotShowedPics();

            StartTimer();

            SetNewPictureofFolder();
        }

        public static void StopSlideShow()
        {
            StopTimer();
        }

        public static void ChangeTimeSpanofTimer()
        {
            PicChangeTimer.Interval = DefaultTimeSpan * 1000;

            if (AppConfigItemsContainer.TimeSpan > 0)
                PicChangeTimer.Interval = AppConfigItemsContainer.TimeSpan * 1000;
        }

        public static void ReadConfigIni()
        {
            AppConfigItemsContainer.IsOpenConfigAtStart =
                FileController.ReadIniDataBool("AppSetting", "OpenConfigAtStart", AppConfigItemsContainer.IsOpenConfigAtStart);
            AppConfigItemsContainer.IsShowBySlideShow =
                FileController.ReadIniDataBool("AppSetting", "ShowBySlideShow", AppConfigItemsContainer.IsShowBySlideShow);
            AppConfigItemsContainer.IsEnlargePictureToLimit =
                FileController.ReadIniDataBool("DrawSetting", "EnlargePictureToLimit", AppConfigItemsContainer.IsEnlargePictureToLimit);
            AppConfigItemsContainer.IsUseHotKey =
                FileController.ReadIniDataBool("AppSetting", "UseHotKey", AppConfigItemsContainer.IsUseHotKey);
            AppConfigItemsContainer.PicFolderDirectory =
                FileController.ReadIniData("AppSetting", "PictureFolderDirectory");
            AppConfigItemsContainer.PicPath =
                FileController.ReadIniData("AppSetting", "PictureFilePath");
            AppConfigItemsContainer.PositionX =
                FileController.ReadIniDataInt("DrawSetting", "DrawPositionHorizontalValue", AppConfigItemsContainer.PositionX);
            AppConfigItemsContainer.PositionY =
                FileController.ReadIniDataInt("DrawSetting", "DrawPositionVerticalValue", AppConfigItemsContainer.PositionY);
            AppConfigItemsContainer.SizeLimitX =
                FileController.ReadIniDataInt("DrawSetting", "DrawSizeLimitHorizontalValue", AppConfigItemsContainer.SizeLimitX);
            AppConfigItemsContainer.SizeLimitY =
                FileController.ReadIniDataInt("DrawSetting", "DrawSizeLimitVerticalValue", AppConfigItemsContainer.SizeLimitY);
            AppConfigItemsContainer.Opacity =
                FileController.ReadIniDataInt("DrawSetting", "DrawOpacityValue", AppConfigItemsContainer.Opacity);
            AppConfigItemsContainer.TimeSpan =
                FileController.ReadIniDataInt("AppSetting", "SlideShowTimeSpan", AppConfigItemsContainer.TimeSpan);

            AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl =
                FileController.ReadIniDataBool("HotKeySetting", "MinimizeUseCtrl", AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl);
            AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift =
                FileController.ReadIniDataBool("HotKeySetting", "MinimizeUseShift", AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift);
            AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt =
                FileController.ReadIniDataBool("HotKeySetting", "MinimizeUseAlt", AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt);

            AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl =
                FileController.ReadIniDataBool("HotKeySetting", "OpenConfigimizeUseCtrl", AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl);
            AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift =
                FileController.ReadIniDataBool("HotKeySetting", "OpenConfigimizeUseShift", AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift);
            AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt =
                FileController.ReadIniDataBool("HotKeySetting", "OpenConfigimizeUseAlt", AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt);
        }

        public static void SaveConfigIni()
        {
            FileController.SaveIniData("AppSetting", "OpenConfigAtStart", AppConfigItemsContainer.IsOpenConfigAtStart.ToString());
            FileController.SaveIniData("AppSetting", "ShowBySlideShow", AppConfigItemsContainer.IsShowBySlideShow.ToString());
            FileController.SaveIniData("DrawSetting", "EnlargePictureToLimit", AppConfigItemsContainer.IsEnlargePictureToLimit.ToString());
            FileController.SaveIniData("AppSetting", "UseHotKey", AppConfigItemsContainer.IsUseHotKey.ToString());
            FileController.SaveIniData("AppSetting", "PictureFolderDirectory", AppConfigItemsContainer.PicFolderDirectory);
            FileController.SaveIniData("AppSetting", "PictureFilePath", AppConfigItemsContainer.PicPath);
            FileController.SaveIniData("DrawSetting", "DrawPositionHorizontalValue", AppConfigItemsContainer.PositionX.ToString());
            FileController.SaveIniData("DrawSetting", "DrawPositionVerticalValue", AppConfigItemsContainer.PositionY.ToString());
            FileController.SaveIniData("DrawSetting", "DrawSizeLimitHorizontalValue", AppConfigItemsContainer.SizeLimitX.ToString());
            FileController.SaveIniData("DrawSetting", "DrawSizeLimitVerticalValue", AppConfigItemsContainer.SizeLimitY.ToString());
            FileController.SaveIniData("DrawSetting", "DrawOpacityValue", AppConfigItemsContainer.Opacity.ToString());
            FileController.SaveIniData("AppSetting","SlideShowTimeSpan",AppConfigItemsContainer.TimeSpan.ToString());

            FileController.SaveIniData("HotKeySetting", "MinimizeUseCtrl", AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl.ToString());
            FileController.SaveIniData("HotKeySetting", "MinimizeUseShift", AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift.ToString());
            FileController.SaveIniData("HotKeySetting", "MinimizeUseAlt", AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt.ToString());

            FileController.SaveIniData("HotKeySetting", "OpenConfigimizeUseCtrl", AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl.ToString());
            FileController.SaveIniData("HotKeySetting", "OpenConfigimizeUseShift", AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift.ToString());
            FileController.SaveIniData("HotKeySetting", "OpenConfigimizeUseAlt", AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt.ToString());
        }

        private static bool SetPicture(string filePath, int xPos, int yPos, bool isEnlargetoMax = true, double maxWidth = 0, double maxHeight = 0, double opacity = 0.8, double magnification = 1, double angle = 0)
        {
            if (_mainWindow == null) return false;

            _mainWindow.OverlayCanvas.Children.Clear();

            PicPathNow = string.Empty;

            if (!File.Exists(filePath)) return false;

            try
            {
                var bi = new BitmapImage(new Uri(filePath, UriKind.Relative));
                var ib = new ImageBrush(bi);

                var width = bi.Width * magnification;
                var height = bi.Height * magnification;

                // 幅だけmaxWidthに合わせるが、この後の流れでUniformFillと変わらない挙動を行う
                if (isEnlargetoMax)
                {
                    var mag = maxWidth / width;
                    width *= mag;
                    height *= mag;
                }

                // FillOption = Uniform のような挙動
                if (maxWidth > 0 && maxHeight > 0)
                {
                    if (width > maxWidth || height > maxHeight)
                    {
                        magnification = Math.Min(maxWidth / width, maxHeight / height);
                        width *= magnification;
                        height *= magnification;
                    }
                }

                var cav = new Canvas
                {
                    Background = ib,
                    Height = height,
                    Width = width,
                    Opacity = opacity,
                    RenderTransform = new RotateTransform(
                        angle,
                        bi.Height * magnification / 2,
                        bi.Width * magnification / 2)
                };

                /*
                RenderOptions.SetEdgeMode(cav, EdgeMode.Aliased);
                RenderOptions.SetBitmapScalingMode(cav, BitmapScalingMode.NearestNeighbor);
                */

                Canvas.SetLeft(cav, xPos - width / 2);
                Canvas.SetTop(cav, yPos - height / 2);

                _mainWindow.OverlayCanvas.Children.Add(cav);

                PicPathNow = filePath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show(@"ファイル " + filePath + @" が正常に読めこめませんでした");

                return false;
            }

            return true;
        }

        private static void SetPicture(string picPath)
        {
            SetPicture(picPath, AppConfigItemsContainer.PositionX, AppConfigItemsContainer.PositionY,
                AppConfigItemsContainer.IsEnlargePictureToLimit, AppConfigItemsContainer.SizeLimitX,
                AppConfigItemsContainer.SizeLimitY, (double)AppConfigItemsContainer.Opacity / 100);
        }

        /// <summary>
        /// 未表示画像リストをリセットする
        /// </summary>
        private static void ResetNotShowedPics()
        {
            NotShowedPicPaths.Clear();
            NotShowedPicPaths.AddRange(PicPaths);
        }

        /// <summary>
        /// 時間間隔とイベントハンドラーをセットし、タイマーをスタートする
        /// </summary>
        private static void StartTimer()
        {
            ChangeTimeSpanofTimer();
            PicChangeTimer.Tick -= SetNewPictureofFolder;
            PicChangeTimer.Tick += SetNewPictureofFolder;
            PicChangeTimer.Start();
        }

        /// <summary>
        /// タイマーをストップする
        /// </summary>
        private static void StopTimer()
        {
            PicChangeTimer.Stop();
        }

        /// <summary>
        /// NotShowedPicPathsリストの中からランダムで画像を選び、オーバーレイとしてセットし、リストから削除する
        /// </summary>
        /// <param name="sender">Nothing to do.</param>
        /// <param name="e">Nothing to do.</param>
        private static void SetNewPictureofFolder(object sender = null, EventArgs e = null)
        {
            if (NotShowedPicPaths.Count == 0)
            {
                ResetNotShowedPics();
            }

            var rnd = new Random();
            var index = rnd.Next(NotShowedPicPaths.Count);
            SetPicture(NotShowedPicPaths[index]);
            NotShowedPicPaths.RemoveAt(index);
        }
    }
}
