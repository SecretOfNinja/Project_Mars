using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mars_Restaurant
{
    // This is the Program class. It was created by default by Visual Studio
    // It holds the static Main() function, which is the entry point for the whole program.
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        // The Main() function is the entry point to the program
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new DoubleBufferedWindow());
            Application.Run(new LoginForm()); // This is where the LoginFrom is launched.
        }
    }
}
