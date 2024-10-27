using System.Configuration;
using System.Data;
using System.Windows;

namespace SimpleJobApply
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Args = e.Args;
        }
    }

}
