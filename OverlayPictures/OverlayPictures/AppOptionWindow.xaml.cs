using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using static OverlayPictures.OverlayPicturesContentController;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace OverlayPictures
{
    /// <summary>
    /// AppOptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AppOptionWindow : Window
    {
        /* 緊急対応策
         新規windowを開いた時点で始まるAppConfigItemsContainerからの読み込み中に
         値を変更したコントロールがSetPicConfigでAppConfigItemsContainerのデータを変える
         ことを阻止する
         */
        private bool _isConfigLoaded = true;

        public AppOptionWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// パラメータの初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppOptionWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            _isConfigLoaded = false;
            
            ReadConfigIni();

            OpenAppConfigCheckBox.IsChecked =
                AppConfigItemsContainer.IsOpenConfigAtStart;
            ShowPictureFromFolderCheckBox.IsChecked =
                AppConfigItemsContainer.IsShowBySlideShow;
            PicFolderDirectoryTextBox.Text =
                AppConfigItemsContainer.PicFolderDirectory;
            PicFilePathTextBox.Text =
                AppConfigItemsContainer.PicPath;
            XPosAllTextBox.Text =
                AppConfigItemsContainer.PositionX.ToString();
            YPosAllTextBox.Text =
                AppConfigItemsContainer.PositionY.ToString();
            LimitWidthAllTextBox.Text
                = AppConfigItemsContainer.SizeLimitX.ToString();
            LimitHeightAllTextBox.Text
                = AppConfigItemsContainer.SizeLimitY.ToString();
            EnlargetoLimitCheckBox.IsChecked
                = AppConfigItemsContainer.IsEnlargePictureToLimit;
            OpacityAllSlider.Value =
                AppConfigItemsContainer.Opacity;
            TimeSpanTextBox.Text =
                AppConfigItemsContainer.TimeSpan.ToString();

            UseHotKeyCheckBox.IsChecked =
                AppConfigItemsContainer.IsUseHotKey;
            HotKeyMinCtrlRadioButton.IsChecked =
                AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl;
            HotKeyMinShiftRadioButton.IsChecked =
                AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift;
            HotKeyMinAltRadioButton.IsChecked =
                AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt;
            HotKeyMinKeyTextBox.Text =
                AppConfigItemsContainer.MinHotKeyConfigContainer.KeyChar.ToString();
            HotKeyOpenConfigCtrlRadioButton.IsChecked =
                AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl;
            HotKeyOpenConfigShiftRadioButton.IsChecked =
                AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift;
            HotKeyOpenConfigAltRadioButton.IsChecked =
                AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt;
            HotKeyOpenConfigKeyTextBox.Text =
                AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.KeyChar.ToString();

            _isConfigLoaded = true;
        }

        private void AppOptionWindow1_Closed(object sender, EventArgs e)
        {
            SetPicConfig();
            SaveConfigIni();
        }

        /// <summary>
        /// http://kagasu.hatenablog.com/entry/2017/02/14/155824 様より
        /// テキストボックスに対する入力を0~9のみ受け付ける
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlyNumTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        /// <summary>
        /// http://kagasu.hatenablog.com/entry/2017/02/14/155824 様より
        /// 貼り付けを無効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlyNumTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void PicFolderRefButton_Click(object sender, RoutedEventArgs e)
        {
            var path = FileController.OpenFolderDialog("", "画像フォルダを開いてください");
            if (path == string.Empty) return;

            PicFolderDirectoryTextBox.Text = path;

            var timeSpan = TimeSpanTextBox.Text;
            SetPicConfig();
            if (Regex.IsMatch(timeSpan, @"^[0-9]+$"))
            {
                StartSlideShow();
            }
        }

        private void PicFileRefButton_Click(object sender, RoutedEventArgs e)
        {
            var path = FileController.OpenFileDialog(
                Extensions,
                "画像ファイルを開いてください",
                ExtensionDescription);

            if (path == string.Empty) return;

            PicFilePathTextBox.Text = path;
            StopSlideShow();
            SetPicConfig();
            SetSinglePicture();
        }

        private static void EnableFrameworkElements(params FrameworkElement[] fe)
        {
            foreach (var element in fe)
            {
                element.IsEnabled = true;
            }
        }

        private static void DisableFrameworkElements(params FrameworkElement[] fe)
        {
            foreach (var element in fe)
            {
                element.IsEnabled = false;
            }
        }

        private void ShowPictureFromFolderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            EnableFrameworkElements(
                PicFolderPathGrid,
                PicFolderShowTimeGrid);
            DisableFrameworkElements(PicFilePathGrid);
            
            SetPicConfig();

            StartSlideShow();
        }

        private void ShowPictureFromFolderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            EnableFrameworkElements(PicFilePathGrid);
            DisableFrameworkElements(
                PicFolderPathGrid,
                PicFolderShowTimeGrid);
            
            StopSlideShow();
            SetPicConfig();
            SetSinglePicture();
        }

        private void UseHotKeyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            EnableFrameworkElements(HotKeyMinGrid, HotKeyOpenConfigGrid);
            SetPicConfig();
        }

        private void UseHotKeyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            DisableFrameworkElements(HotKeyMinGrid, HotKeyOpenConfigGrid);
            SetPicConfig();
        }

        private void TimeSpanTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPicConfig();
            ChangeTimeSpanofTimer();
        }

        private void TimeSpanTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            if (e.Key != Key.Enter) return;

            SetPicConfig();
            ChangeTimeSpanofTimer();
        }

        private void OpacityAllSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            UnifyTexttoSliderValue(OpacityAllSlider, OpacityAllTextBox);
            SetPicConfig(false, false, true);
            SetPictureAgain();

            // 単純に処理回数を下げ、CPU使用率を減少させる
            System.Threading.Thread.Sleep(3);
        }

        private void OpacityAll_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            UnifySlidertoTextValue(OpacityAllSlider, OpacityAllTextBox);
        }

        private void LimitWidthAllSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            UnifyTexttoSliderValue(LimitWidthAllSlider, LimitWidthAllTextBox);
            SetPicConfig(false, true, false);
            SetPictureAgain();
            
            System.Threading.Thread.Sleep(3);
        }

        private void LimitHeightAllSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            UnifyTexttoSliderValue(LimitHeightAllSlider, LimitHeightAllTextBox);
            SetPicConfig(false,true,false);
            SetPictureAgain();
            
            System.Threading.Thread.Sleep(3);
        }

        private void LimitWidthAllTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            UnifySlidertoTextValue(LimitWidthAllSlider, LimitWidthAllTextBox);
        }

        private void LimitHeightAllTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            UnifySlidertoTextValue(LimitHeightAllSlider, LimitHeightAllTextBox);
        }

        private static void UnifySlidertoTextValue(RangeBase sli, TextBox tb)
        {
            if (!Regex.IsMatch(tb.Text, @"^[0-9]+$")) return;

            var tbNum = int.Parse(tb.Text);

            if (tbNum > sli.Maximum)
            {
                tb.Text = ((int)sli.Maximum).ToString();
                sli.Value = sli.Maximum;

                return;
            }

            if (tbNum < sli.Minimum)
            {
                tb.Text = ((int)sli.Minimum).ToString();
                sli.Value = sli.Minimum;

                return;
            }

            sli.Value = int.Parse(tb.Text);
        }

        private static void UnifyTexttoSliderValue(RangeBase sli, TextBox tb)
        {
            sli.Value = Math.Round(sli.Value);
            tb.Text = sli.Value.ToString(CultureInfo.InvariantCulture);
        }

        private void GetPosButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            var win = new Window
            {
                Owner = GetWindow(this),
                AllowsTransparency = true,
                Topmost = true,
                Opacity = 0.3,
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false
            };

            win.MouseDown += GetPosWindow_MouseDown;
            win.MouseMove += GetPosWindow_MouseMove;
            win.MouseUp += GetWindow_MouseUp;
            win.KeyDown += GetWindow_KeyDown;
            win.ShowDialog();
        }

        private void GetPosWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var win = (Window)sender;

            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                win.Close();
                return;
            }

            var pos = GetAculate2DPosition(
                new Point(e.GetPosition(win).X, e.GetPosition(win).Y),
                new Size(win.Width, win.Height));

            SetPosAllTextBox(pos.X, pos.Y);
        }

        // メモリ使用量がやばい(GC発生回数が多い)
        private void GetPosWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var win = (Window)sender;

            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                win.Close();
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var pos = GetAculate2DPosition(
                new Point(e.GetPosition(win).X, e.GetPosition(win).Y),
                new Size(win.Width, win.Height));

            SetPosAllTextBox(pos.X, pos.Y);

            // 苦肉の策 CPU使用率が15%~5%未満に減少
            System.Threading.Thread.Sleep(3);
        }

        private static void GetWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Released)
            {
                return;
            }

            var win = (Window)sender;

            win.Close();
        }

        private static void GetWindow_KeyDown(object sender, KeyEventArgs e)
        {
            var win = (Window)sender;

            win.Close();
        }

        private static Point GetAculate2DPosition(Point pos, Size windowSize)
        {
            // 謎の余白を削除した座標位置
            var rt = new Point(
            pos.X - (windowSize.Width - Screen.PrimaryScreen.Bounds.Width) / 2,
            pos.Y - (windowSize.Height - Screen.PrimaryScreen.Bounds.Height) / 2);

            // 念のため、min,maxの調整
            rt.X = Math.Max(Math.Min(rt.X, Screen.PrimaryScreen.Bounds.Width), 0);
            rt.Y = Math.Max(Math.Min(rt.Y, Screen.PrimaryScreen.Bounds.Height), 0);

            return rt;
        }

        private void SetPosAllTextBox(double x, double y)
        {
            XPosAllTextBox.Text = ((int)Math.Round(x)).ToString();
            YPosAllTextBox.Text = ((int)Math.Round(y)).ToString();
        }

        /// <summary>
        /// AppConfigItemsContainerの要素を更新する
        /// </summary>
        private void SetPicConfig()
        {
            if (!_isConfigLoaded)
                return;

            AppConfigItemsContainer.IsOpenConfigAtStart = GetConfigValue(OpenAppConfigCheckBox, AppConfigItemsContainer.IsOpenConfigAtStart);
            AppConfigItemsContainer.IsShowBySlideShow = GetConfigValue(ShowPictureFromFolderCheckBox, AppConfigItemsContainer.IsShowBySlideShow);
            AppConfigItemsContainer.IsUseHotKey = GetConfigValue(UseHotKeyCheckBox, AppConfigItemsContainer.IsUseHotKey);
            AppConfigItemsContainer.IsEnlargePictureToLimit = GetConfigValue(EnlargetoLimitCheckBox, AppConfigItemsContainer.IsEnlargePictureToLimit);
            AppConfigItemsContainer.PositionX = GetConfigValue(XPosAllTextBox, AppConfigItemsContainer.PositionX);
            AppConfigItemsContainer.PositionY = GetConfigValue(YPosAllTextBox, AppConfigItemsContainer.PositionY);
            AppConfigItemsContainer.SizeLimitX = GetConfigValue(LimitWidthAllTextBox, AppConfigItemsContainer.SizeLimitX);
            AppConfigItemsContainer.SizeLimitY = GetConfigValue(LimitHeightAllTextBox, AppConfigItemsContainer.SizeLimitY);
            AppConfigItemsContainer.Opacity = GetConfigValue(OpacityAllSlider, OpacityAllTextBox);
            AppConfigItemsContainer.TimeSpan = GetConfigValue(TimeSpanTextBox, DefaultTimeSpan);
            AppConfigItemsContainer.PicFolderDirectory = PicFolderDirectoryTextBox.Text;
            AppConfigItemsContainer.PicPath = PicFilePathTextBox.Text;

            AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl =
                GetConfigValue(HotKeyMinCtrlRadioButton, AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl);
            AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift =
                GetConfigValue(HotKeyMinShiftRadioButton, AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift);
            AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt =
                GetConfigValue(HotKeyMinAltRadioButton, AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt);
            AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl =
                GetConfigValue(HotKeyOpenConfigCtrlRadioButton, AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl);
            AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift =
                GetConfigValue(HotKeyOpenConfigShiftRadioButton, AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift);
            AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt =
                GetConfigValue(HotKeyOpenConfigAltRadioButton, AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt);
        }

        private void SetPicConfig(bool isPosition, bool isSize, bool isOpacity)
        {
            if (!_isConfigLoaded)
                return;

            if (isPosition)
            {
                AppConfigItemsContainer.PositionX = GetConfigValue(XPosAllTextBox, AppConfigItemsContainer.PositionX);
                AppConfigItemsContainer.PositionY = GetConfigValue(YPosAllTextBox, AppConfigItemsContainer.PositionY);
            }

            if (isSize)
            {
                AppConfigItemsContainer.SizeLimitX = GetConfigValue(LimitWidthAllTextBox, AppConfigItemsContainer.SizeLimitX);
                AppConfigItemsContainer.SizeLimitY = GetConfigValue(LimitHeightAllTextBox, AppConfigItemsContainer.SizeLimitY);
            }

            if (isOpacity)
            {
                AppConfigItemsContainer.Opacity = GetConfigValue(OpacityAllSlider, OpacityAllTextBox);
            }
        }

        private static int GetConfigValue(TextBox txtBox, int defaultValue)
        {
            var value = defaultValue;

            // 正の数限定
            if (Regex.IsMatch(txtBox.Text, @"^[0-9]+$"))
            {
                value = int.Parse(txtBox.Text);
            }
            else
            {
                txtBox.Text = defaultValue.ToString();
            }

            return value;
        }

        private static int GetConfigValue(RangeBase sli, TextBox txtBox)
        {
            var value = (int)Math.Round(sli.Value);
            if (!Regex.IsMatch(txtBox.Text, value.ToString()))
            {
                txtBox.Text = value.ToString();
            }

            return value;
        }

        private static bool GetConfigValue(ToggleButton cb, bool defaultValue)
        {
            if (cb.IsChecked != null) return (bool)cb.IsChecked;

            cb.IsChecked = defaultValue;
            return defaultValue;
        }

        private void AllNumTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPicConfig();
            SetPictureAgain();
        }

        private void EnlargetoLimitCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            SetPicConfig();
            SetPictureAgain();
        }

        private void EnlargetoLimitCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            SetPicConfig();
            SetPictureAgain();
        }

        private void AllNumTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;

            SetPicConfig(true, true, false);
            SetPictureAgain();
        }

        private void AllHotKeyRadioButton_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (!AppOptionWindow1.IsLoaded)
                return;
            
            SetPicConfig();
        }
    }
}
