using Jmelosegui.Windows;

namespace WpfTestAplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : BorderlessWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Uncomment the following method to override the chrome buttons styles.
        //protected override Style GetChromeButtonStyle()
        //{
        //    var chromeButtonStyle = new ResourceDictionary
        //    {
        //        Source = new Uri("/WpfTestAplication;component/Assets/ChromeButtons.xaml", UriKind.RelativeOrAbsolute)
        //    };

        //    return chromeButtonStyle["ChromeButtonStyle"] as Style;
        //}
    }
}
