using System;
using System.Collections.Generic;
using System.Text;

namespace _04_RotatingTriangle
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var mainWindow = new MainWindow())
            {
                mainWindow.Run(60, 60);
            }
        }
    }
}
