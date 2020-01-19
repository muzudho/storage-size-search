using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace storage_size_search
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string GetPresentableByteText(long size)
        {
            if (1023 < size)
            {
                size /= 1024;

                if (1023 < size)
                {
                    size /= 1024;

                    if (1023 < size)
                    {
                        size /= 1024;

                        if (1023 < size)
                        {
                            size /= 1024;

                            return $"{size} TB";
                        }
                        else
                        {
                            return $"{size} GB";
                        }
                    }
                    else
                    {
                        return $"{size} MB";
                    }
                }
                else
                {
                    return $"{size} KB";
                }
            }
            else
            {
                return $"{size} Bytes";
            }
        }

        public static string GetPresentableMillisecondsText(long milliseconds)
        {
            if (999 < milliseconds)
            {
                milliseconds /= 1000;

                if (59 < milliseconds)
                {
                    milliseconds /= 59;

                    if (59 < milliseconds)
                    {
                        milliseconds /= 59;

                        if (24 < milliseconds)
                        {
                            milliseconds /= 24;

                            return $"{milliseconds} Days";
                        }
                        else
                        {
                            return $"{milliseconds} Hours";
                        }
                    }
                    else
                    {
                        return $"{milliseconds} Minutes";
                    }
                }
                else
                {
                    return $"{milliseconds} Seconds";
                }
            }
            else
            {
                return $"{milliseconds} Milli-seconds";
            }
        }

        /// <summary>
        /// フォルダのサイズを取得する
        /// </summary>
        /// <param name="dirInfo">サイズを取得するフォルダ</param>
        /// <returns>フォルダのサイズ（バイト）</returns>
        public static (long, bool) GetDirectorySize(DirectoryInfo dirInfo, Stopwatch stopwatch)
        {
            try
            {
                long size = 0;
                bool timeUp = false;

                //フォルダ内の全ファイルの合計サイズを計算する
                foreach (FileInfo fi in dirInfo.GetFiles())
                {
                    size += fi.Length;
                }

                //サブフォルダのサイズを合計していく
                foreach (DirectoryInfo di in dirInfo.GetDirectories())
                {
                    if (10 * 1000 < stopwatch.ElapsedMilliseconds)
                    {
                        // Time up.
                        return (size, true);
                    }

                    var (size2, timeUp2) = GetDirectorySize(di, stopwatch);
                    size += size2;
                    timeUp = timeUp2;
                }

                //結果を返す
                return (size, timeUp);
            }
            catch (Exception e)
            {
                // 例外は無視します。
                Trace.WriteLine($"Error   | {e.Message}");
                return (-1, false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var location = LocationTextBox.Text;
            Trace.WriteLine($"Info    | Start into [{location}].");

            // TODO 別スレッドでストレージを探索。
            var stopwatch = new Stopwatch();
            foreach (var entry in Directory.EnumerateFileSystemEntries(location))
            {
                stopwatch.Start();
                // Trace.WriteLine($"Info    | Search=[{entry}]");
                var info = new DirectoryInfo(entry);
                var (size2, timeUp) = GetDirectorySize(info, stopwatch);

                DisplayInfo(entry, size2, timeUp, stopwatch);
                stopwatch.Stop();
            }

            // TODO ファイル書き出し

            Trace.WriteLine("Info    | Finished.");
        }

        private static void DisplayInfo(string entry, long size, bool timeUp, Stopwatch stopwatch)
        {
            var textB = new StringBuilder();
            textB.Append($" entry=[{entry}]");
            textB.Append($" size=[{GetPresentableByteText(size)}]");

            if (timeUp)
            {
                textB.Append($" Time-up");
            }

            textB.Append($" time=[{GetPresentableMillisecondsText(stopwatch.ElapsedMilliseconds)}]");
            Trace.WriteLine($"Info    |{textB.ToString()}");
        }
    }
}
