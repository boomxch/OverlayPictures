using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using MessageBox = System.Windows.MessageBox;

namespace OverlayPictures
{
    internal class FileController
    {
        private const string FolderPath = @"OverlayPictures";
        private const string FilePath = @"OverlayPictures\config.ini";

        /// <summary>
        /// このアプリの設定ファイルを保存する　保存する内容はアプリ下部にて参照しているディレクトリパス
        /// </summary>
        /// <param name="pathData">
        /// 保存するデータ(itemPropertyNameをKeyにして中身を取得できるDictionary型)
        /// </param>
        /// <param name="itemNames">
        /// 項目名
        /// </param>
        /// <param name="initialConfigData">
        /// 初期状態の項目群
        /// </param>
        public static void SaveApplicationInfo(
            IReadOnlyDictionary<string, string> pathData,
            string[] itemNames,
            string[] initialConfigData)
        {
            // ファイルが存在しない場合新規作成
            if (!File.Exists(FilePath))
            {
                try
                {
                    // アプリの設定ファイルを保存するフォルダーが存在しなければ新規作成
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    // ファイルを新規作成
                    using (var hstream = File.Create(FilePath))
                    {
                        hstream.Close();
                    }

                    // ファイルに項目を記述
                    using (var sr = new StreamWriter(FilePath, false, Encoding.GetEncoding("shift_jis")))
                    {
                        foreach (var item in initialConfigData)
                        {
                            sr.WriteLine(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("エラーが発生しました\n設定ファイルが正常に保存できませんでした");
                    MessageBox.Show(e.ToString());
                }
            }

            // 設定ファイルを開き、対象となる項目を探し、書き換える
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var sr = new StreamReader(fs);
                var sw = new StreamWriter(fs);

                var isExitsts = new bool[itemNames.Length];
                var txtData = new List<string>();

                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();

                    foreach (var item in pathData)
                    {
                        if (s == null || !System.Text.RegularExpressions.Regex.IsMatch(s, "^" + item.Key + "=.*$"))
                        {
                            continue;
                        }

                        if (Array.IndexOf(itemNames, item.Key) > -1)
                        {
                            isExitsts[Array.IndexOf(itemNames, item.Key)] = true;
                        }

                        s = item.Key + "=" + item.Value;

                        break;
                    }

                    txtData.Add(s);
                }

                // 存在しない場合、項目別に新規作成(順不同)
                for (var i = isExitsts.Length - 1; i >= 0; i--)
                {
                    if (!isExitsts[i])
                    {
                        txtData.Insert(0, itemNames[i] + "=" + pathData[itemNames[i]]);
                    }
                }

                fs.Position = 0;
                fs.SetLength(0);

                foreach (var t in txtData)
                {
                    sw.WriteLine(t);
                }

                sw.Flush();

                sr.Close();
                if (sw.BaseStream.CanWrite)
                {
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// 設定ファイルからデータを読み取る
        /// *=* のように記述 // はコメントとして認識
        /// </summary>
        /// <param name="itemNames">
        /// 項目名
        /// </param>
        /// <returns>
        /// データをitemPropertyNameをKeyとしたDictionary形式で返す
        /// </returns>
        public static Dictionary<string, string> ReadApplicationInfo(string[] itemNames)
        {
            var data = itemNames.ToDictionary(item => item, item => string.Empty);

            // ファイルが存在しない場合、全ての項目をString.Emptyで返す
            if (!File.Exists(FilePath))
            {
                return data;
            }

            // 項目をそれぞれ探し、それに対応したValueを設定する
            using (var sr = new StreamReader(FilePath, Encoding.GetEncoding("shift_jis")))
            {
                while (sr.Peek() > -1)
                {
                    var s = sr.ReadLine();
                    if (s == null)
                    {
                        continue;
                    }

                    // "//"はコメントとして認識、同行のこの文字列以降は認知しない
                    if (s.IndexOf("//", StringComparison.Ordinal) >= 0)
                    {
                        s = s.Substring(0, s.IndexOf("//", StringComparison.Ordinal));
                    }

                    foreach (var t in itemNames)
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^" + t + @"=.+$"))
                        {
                            continue;
                        }

                        data[t] = s.Substring((t + "=").Length);
                        break;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// ファイルを取得するためのダイアログを開く
        /// </summary>
        /// <param name="fileExtensions">取得したいファイルの拡張子一覧 全ての拡張子が同時に表示されるように設定</param>
        /// <param name="description">ダイアログの説明</param>
        /// <param name="extensionDescription">取得したいファイル拡張子の説明</param>
        /// <param name="fileName"> 取得したいファイル名</param>
        /// <returns>
        /// ファイル取得に成功した場合、ファイルパスを返す
        /// 失敗した場合、空文字列を返す
        /// </returns>
        public static string OpenFileDialog(string[] fileExtensions, string description = "ファイルを開いてください", string extensionDescription = "", string fileName = "")
        {
            var filter = string.Empty;
            if (fileExtensions.Length > 0)
            {
                // Example : Image Files (*.bmp, *.jpg)|*.bmp;*.jpg
                filter = extensionDescription + @"(";
                for (var i = 0; i < fileExtensions.Length; i++)
                {
                    filter += @"*.";
                    filter += fileExtensions[i];

                    if (i != fileExtensions.Length - 1) filter += @", ";
                }

                filter += ")|";
                for (var i = 0; i < fileExtensions.Length; i++)
                {
                    filter += @"*.";
                    filter += fileExtensions[i];

                    if (i != fileExtensions.Length - 1) filter += @";";
                }

                filter += @"|";
            }
            filter += @"全てのファイル(*.*)|*.*";


            var ofd = new OpenFileDialog
            {
                FileName = fileName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = filter,
                FilterIndex = 0,
                Title = description,
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            return ofd.ShowDialog().ToString() == "OK" ? ofd.FileName : string.Empty;
        }

        /// <summary>
        /// フォルダーを取得するためのダイアログを開く
        /// </summary>
        /// <param name="selectedPath">ダイアログを開いた際に表示されるフォルダ</param>
        /// <param name="description"></param>
        /// <returns>
        /// フォルダ取得に成功した場合、フォルダパスを返す
        /// 失敗した場合、空文字列を返す
        /// </returns>
        public static string OpenFolderDialog(string selectedPath = "", string description = "フォルダを開いてください")
        {
            var fbd = new FolderBrowserDialog
            {
                Description = description,
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = selectedPath,
                ShowNewFolderButton = true
            };

            return fbd.ShowDialog().ToString() == "OK" ? fbd.SelectedPath : string.Empty;
        }

        /// <summary>
        /// 与えられたディレクトリ内のファイルの内、与えられた拡張子に基づくものだけすべて列挙する
        /// </summary>
        /// <param name="directory">ファイルを検索するディレクトリ</param>
        /// <param name="extensions">検索するファイルの拡張子</param>
        /// <returns>条件に適合するファイルの絶対パスの配列</returns>
        public static string[] GetFilePaths(string directory, string[] extensions)
        {
            if (!Directory.Exists(directory))
                return new string[0];

            var paths = new List<string>();

            foreach (var extension in extensions)
            {
                paths.AddRange(Directory.GetFiles(directory, "*." + extension, SearchOption.TopDirectoryOnly));
            }

            if (extensions.Length == 0)
            {
                paths.AddRange(Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly));
            }

            paths.Sort();

            return paths.ToArray();
        }

        public static void SaveIniData(string section, string key, string value = "")
        {
            // ファイルが存在しない場合新規作成
            if (!File.Exists(FilePath))
            {
                try
                {
                    // アプリの設定ファイルを保存するフォルダーが存在しなければ新規作成
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    // ファイルを新規作成
                    using (var hstream = File.Create(FilePath))
                    {
                        hstream.Close();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("エラーが発生しました\n設定ファイルが正常に保存できませんでした");
                    MessageBox.Show(e.ToString());
                }
            }

            var ini = new IniFile(FilePath)
            {
                [section, key] = value
            };
        }

        public static string ReadIniData(string section, string key)
        {
            if (!File.Exists(FilePath))
                return "";

            var ini = new IniFile(FilePath);
            return ini[section, key];
        }

        public static bool ReadIniDataBool(string section, string key, bool defaultValue = true)
        {
            if (!File.Exists(FilePath))
                return defaultValue;

            var ini = new IniFile(FilePath);

            if (string.Compare(ini[section, key], "true", StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            return string.Compare(ini[section, key], "false", StringComparison.OrdinalIgnoreCase) != 0 && defaultValue;
        }

        public static int ReadIniDataInt(string section, string key, int defaultValue = 0)
        {
            if (!File.Exists(FilePath))
                return defaultValue;

            var ini = new IniFile(FilePath);
            return !int.TryParse(ini[section, key], out var result) ? defaultValue : result;
        }
    }

    /// <summary>
    /// https://anis774.net/codevault/inifile.html 様より
    /// INIファイルを読み書きするクラス
    /// </summary>
    public class IniFile
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedstring,
            int nSize,
            string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpstring,
            string lpFileName);

        private readonly string _filePath;

        /// <summary>
        /// ファイル名を指定して初期化します。
        /// ファイルが存在しない場合は初回書き込み時に作成されます。
        /// </summary>
        public IniFile(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// sectionとkeyからiniファイルの設定値を取得、設定します。 
        /// </summary>
        /// <returns>指定したsectionとkeyの組合せが無い場合は""が返ります。</returns>
        public string this[string section, string key]
        {
            set => WritePrivateProfileString(section, key, value, _filePath);
            get
            {
                var sb = new StringBuilder(256);
                GetPrivateProfileString(section, key, string.Empty, sb, sb.Capacity, _filePath);
                return sb.ToString();
            }
        }

        /// <summary>
        /// sectionとkeyからiniファイルの設定値を取得します。
        /// 指定したsectionとkeyの組合せが無い場合はdefaultvalueで指定した値が返ります。
        /// </summary>
        /// <returns>
        /// 指定したsectionとkeyの組合せが無い場合はdefaultvalueで指定した値が返ります。
        /// </returns>
        public string GetValue(string section, string key, string defaultvalue)
        {
            StringBuilder sb = new StringBuilder(256);
            GetPrivateProfileString(section, key, defaultvalue, sb, sb.Capacity, _filePath);
            return sb.ToString();
        }
    }
}
