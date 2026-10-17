using System;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Uygulama açılırken veritabanını ve tabloları hazırla
            DatabaseInitializer.EnsureDatabaseAndTables();

            Application.Run(new Form1());
        }
    }
}