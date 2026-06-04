using System.Windows;
using BisectionApp.Database;

namespace BisectionApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseHelper.CreateTable();
        }
    }
}