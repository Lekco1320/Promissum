using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : Page, INotifyPropertyChanged
    {
        private readonly static int[] _fontSizes = new int[]
        {
            18,
            18,
            18,
            17,
            17,
        };

        private readonly static FontFamily[] _fonts = new FontFamily[]
        {
            new FontFamily("Times New Roman"),
            new FontFamily("Times New Roman"),
            new FontFamily("Times New Roman"),
            new FontFamily("仿宋"),
            new FontFamily("MS Mincho"),
        };

        private readonly static FontStyle[] _styles = new FontStyle[]
        {
            FontStyles.Italic,
            FontStyles.Italic,
            FontStyles.Italic,
            FontStyles.Normal,
            FontStyles.Normal,
        };

        private readonly static string[][] _captions = new string[][]
        {
            new string[3] { "Backup est optimum ", "promissum", " ad data." },
            new string[3] { "Backup is the best ", "promise", " to data."},
            new string[3] { "Sicherungskopie ist das beste ", "Versprechen", " für Daten."},
            new string[3] { "备份是对数据最好的", "承诺", "。"},
            new string[3] { "バックアップはデータにとって最高の", "約束", "です。" }
        };

        private int _id = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int ThisFontSize
        {
            get => _thisFontSize;
            set
            {
                _thisFontSize = value;
                OnPropertyChanged(nameof(ThisFontSize));
            }
        }
        private int _thisFontSize;

        public FontFamily ThisFont
        {
            get => _thisFont;
            set
            {
                _thisFont = value;
                OnPropertyChanged(nameof(ThisFont));
            }
        }
        private FontFamily _thisFont;

        public FontStyle ThisFontStyle
        {
            get => _thisFontStyle;
            set
            {
                _thisFontStyle = value;
                OnPropertyChanged(nameof(ThisFontStyle));
            }
        }
        private FontStyle _thisFontStyle;

        public string Text1
        {
            get => _text1;
            set
            {
                _text1 = value;
                OnPropertyChanged(nameof(Text1));
            }
        }
        private string _text1;

        public string Text2
        {
            get => _text2;
            set
            {
                _text2 = value;
                OnPropertyChanged(nameof(Text2));
            }
        }
        private string _text2;

        public string Text3
        {
            get => _text3;
            set
            {
                _text3 = value;
                OnPropertyChanged(nameof(Text3));
            }
        }
        private string _text3;

        public StartPage()
        {
            InitializeComponent();

            _thisFontSize = _fontSizes[0];
            _thisFont = _fonts[0];
            _thisFontStyle = _styles[0];
            _text1 = _captions[0][0];
            _text2 = _captions[0][1];
            _text3 = _captions[0][2];
            DataContext = this;
            Task runningTask = new Task(RunText, TaskCreationOptions.LongRunning);
            runningTask.Start();
        }

        public static void FadeInAnimate(TextBlock textBlock)
        {
            var animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(1)));
            textBlock.BeginAnimation(OpacityProperty, animation);
        }

        public static void FadeOutAnimate(TextBlock textBlock)
        {
            var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(1)));
            textBlock.BeginAnimation(OpacityProperty, animation);
        }

        public void RunText()
        {
            while (true)
            {
                Dispatcher.BeginInvoke(() => FadeInAnimate(TextBlock1));
                Dispatcher.BeginInvoke(() => FadeInAnimate(TextBlock2));
                Dispatcher.BeginInvoke(() => FadeInAnimate(TextBlock3));
                Task.Delay(7500).Wait();
                Dispatcher.BeginInvoke(() => FadeOutAnimate(TextBlock1));
                Dispatcher.BeginInvoke(() => FadeOutAnimate(TextBlock2));
                Dispatcher.BeginInvoke(() => FadeOutAnimate(TextBlock3));
                Task.Delay(1000).Wait();
                _id = (_id + 1) % _fonts.Length;
                ThisFontSize = _fontSizes[_id];
                ThisFont = _fonts[_id];
                ThisFontStyle = _styles[_id];
                Text1 = _captions[_id][0];
                Text2 = _captions[_id][1];
                Text3 = _captions[_id][2];
            }
        }
    }
}
