using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MessageBox = System.Windows.MessageBox;

namespace OverlayPictures
{
    /// <summary>
    /// OverlayPictures ver.1.00
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
         * システムメニュー追加参考:かに太郎様 https://ameblo.jp/kani-tarou/entry-10240156672.html
         * 画面透過処理、クリックスルー参考:SUIMA様 https://qiita.com/SUIMA/items/ea9faeda750248d57306
        */

        private const int GwlExstyle = (-20);
        private const int WsExTransparent = 0x00000020;
        private HwndSource _hwndSource = null;
        private const int WmSyscommand = 0x112;
        private const int MfSeparator = 0x0800;
        private const int AppOptionMenu = 100;
        private HotKey _minHotKey;
        private HotKey _openConfigHotKey;

        [DllImport("user32")]
        protected static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        protected static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwLong);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, int bRevert);

        [DllImport("user32.dll")]
        private static extern int AppendMenu(IntPtr hMenu, int flagsw, int idNewItem, string lpNewItem);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 設定メニューを追加
            var hwnd = new WindowInteropHelper(this).Handle;
            var menu = GetSystemMenu(hwnd, 0);
            AppendMenu(menu, MfSeparator, 0, null);
            AppendMenu(menu, 0, AppOptionMenu, "アプリの設定");

            OverlayPicturesContentController.SetMainWindow(this);
            OverlayPicturesContentController.ReadConfigIni();

            if (OverlayPicturesContentController.AppConfigItemsContainer.IsShowBySlideShow)
            {
                OverlayPicturesContentController.StartSlideShow();
            }
            else if (File.Exists(OverlayPicturesContentController.AppConfigItemsContainer.PicPath))
            {
                OverlayPicturesContentController.SetSinglePicture();
            }

            SetHotKey();

            if (OverlayPicturesContentController.AppConfigItemsContainer.IsOpenConfigAtStart)
                OpenAppOptionWindow();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DisposeHotKey();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //WindowHandle(Win32) を取得
            var handle = new WindowInteropHelper(this).Handle;

            //クリックをスルー
            var extendStyle = GetWindowLong(handle, GwlExstyle);
            extendStyle |= WsExTransparent; //フラグの追加
            SetWindowLong(handle, GwlExstyle, extendStyle);

            base.OnSourceInitialized(e);
            // フックを追加
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            _hwndSource?.AddHook(HwndSourceHook);
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WmSyscommand) return IntPtr.Zero;

            switch (wParam.ToInt32())
            {
                case AppOptionMenu:
                    OpenAppOptionWindow();
                    return IntPtr.Zero;

                default:
                    return IntPtr.Zero;
            }
        }

        private void OpenAppOptionWindow()
        {
            if (OwnedWindows.Count > 0)
            {
                return;
            }

            var win = new AppOptionWindow
            {
                Owner = GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Closed += AppConfigWindowChangeHotKey;

            win.UseHotKeyCheckBox.Checked += AppConfigWindowChangeHotKey;
            win.UseHotKeyCheckBox.Unchecked += AppConfigWindowChangeHotKey;

            win.HotKeyMinCtrlRadioButton.Checked += AppConfigWindowChangeHotKey;
            win.HotKeyMinCtrlRadioButton.Unchecked += AppConfigWindowChangeHotKey;
            win.HotKeyMinShiftRadioButton.Checked += AppConfigWindowChangeHotKey;
            win.HotKeyMinShiftRadioButton.Unchecked += AppConfigWindowChangeHotKey;
            win.HotKeyMinAltRadioButton.Checked += AppConfigWindowChangeHotKey;
            win.HotKeyMinAltRadioButton.Unchecked += AppConfigWindowChangeHotKey;

            win.HotKeyOpenConfigCtrlRadioButton.Checked += AppConfigWindowChangeHotKey;
            win.HotKeyOpenConfigCtrlRadioButton.Unchecked += AppConfigWindowChangeHotKey;
            win.HotKeyOpenConfigShiftRadioButton.Checked += AppConfigWindowChangeHotKey;
            win.HotKeyOpenConfigShiftRadioButton.Unchecked += AppConfigWindowChangeHotKey;
            win.HotKeyOpenConfigAltRadioButton.Checked += AppConfigWindowChangeHotKey;
            win.HotKeyOpenConfigAltRadioButton.Unchecked += AppConfigWindowChangeHotKey;

            win.ShowDialog();
        }

        private void AppConfigWindowChangeHotKey(object sender, EventArgs e)
        {
            // AppOptionWindow側で定義しているイベントの方が先に実行される
            SetHotKey();
        }

        private void SetHotKey()
        {
            DisposeHotKey();

            if (!OverlayPicturesContentController.AppConfigItemsContainer.IsUseHotKey)
                return;

            var minSpecialKeyNum = 0;
            if (OverlayPicturesContentController.AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseCtrl)
                minSpecialKeyNum = (int)MOD_KEY.CONTROL;
            else if (OverlayPicturesContentController.AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseShift)
                minSpecialKeyNum = (int)MOD_KEY.SHIFT;
            else if (OverlayPicturesContentController.AppConfigItemsContainer.MinHotKeyConfigContainer.IsUseAlt)
                minSpecialKeyNum = (int)MOD_KEY.ALT;

            _minHotKey = new HotKey((MOD_KEY)minSpecialKeyNum, OverlayPicturesContentController.AppConfigItemsContainer.MinHotKeyConfigContainer.Key);

            var openConfigSpecialKeyNum = 0;
            if (OverlayPicturesContentController.AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseCtrl)
                openConfigSpecialKeyNum |= (int)MOD_KEY.CONTROL;
            if (OverlayPicturesContentController.AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseShift)
                openConfigSpecialKeyNum |= (int)MOD_KEY.SHIFT;
            if (OverlayPicturesContentController.AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.IsUseAlt)
                openConfigSpecialKeyNum |= (int)MOD_KEY.ALT;

            _openConfigHotKey = new HotKey((MOD_KEY)openConfigSpecialKeyNum, OverlayPicturesContentController.AppConfigItemsContainer.OpenConfigHotKeyConfigContainer.Key);

            _minHotKey.HotKeyPush += MinimizeWindowHotKey_HotKeyPush;
            _openConfigHotKey.HotKeyPush += OpenConfigWindowHotKey_HotKeyPush;
        }

        private void MinimizeWindowHotKey_HotKeyPush(object sender, EventArgs e)
        {
            WindowState = WindowState == WindowState.Minimized ? WindowState.Maximized : WindowState.Minimized;
        }

        private void OpenConfigWindowHotKey_HotKeyPush(object sender, EventArgs e)
        {
            OpenAppOptionWindow();
        }

        private void DisposeHotKey()
        {
            _minHotKey?.Dispose();
            _openConfigHotKey?.Dispose();
        }
    }

    /// <summary>
    /// https://anis774.net/codevault/hotkey.html 様より
    /// グローバルホットキーを登録するクラス。
    /// 使用後は必ずDisposeすること。
    /// </summary>
    public class HotKey : IDisposable
    {
        private readonly HotKeyForm form;
        /// <summary>
        /// ホットキーが押されると発生する。
        /// </summary>
        public event EventHandler HotKeyPush;

        /// <summary>
        /// ホットキーを指定して初期化する。
        /// 使用後は必ずDisposeすること。
        /// </summary>
        /// <param name="modKey">修飾キー</param>
        /// <param name="key">キー</param>
        public HotKey(MOD_KEY modKey, Keys key)
        {
            form = new HotKeyForm(modKey, key, RaiseHotKeyPush);
        }

        private void RaiseHotKeyPush()
        {
            HotKeyPush?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            form.Dispose();
        }

        private class HotKeyForm : Form
        {
            [DllImport("user32.dll")]
            private static extern int RegisterHotKey(IntPtr HWnd, int ID, MOD_KEY MOD_KEY, Keys KEY);

            [DllImport("user32.dll")]
            private static extern int UnregisterHotKey(IntPtr HWnd, int ID);

            private const int WM_HOTKEY = 0x0312;
            private readonly int id;
            private readonly ThreadStart proc;

            public HotKeyForm(MOD_KEY modKey, Keys key, ThreadStart proc)
            {
                this.proc = proc;
                for (int i = 0x0000; i <= 0xbfff; i++)
                {
                    if (RegisterHotKey(Handle, i, modKey, key) == 0) continue;

                    id = i;
                    break;
                }
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg != WM_HOTKEY) return;

                if ((int)m.WParam == id)
                {
                    proc();
                }
            }

            protected override void Dispose(bool disposing)
            {
                UnregisterHotKey(Handle, id);
                base.Dispose(disposing);
            }
        }
    }

    /// <summary>
    /// HotKeyクラスの初期化時に指定する修飾キー
    /// </summary>
    public enum MOD_KEY : int
    {
        ALT = 0x0001,
        CONTROL = 0x0002,
        SHIFT = 0x0004,
    }
}
