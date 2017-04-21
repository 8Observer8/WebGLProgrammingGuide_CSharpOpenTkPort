using System;
using System.Collections.Generic;
using System.Text;

namespace _01_HelloCanvas
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MainWindow mainWindow = new MainWindow())
            {
                mainWindow.Run(30);
            }
        }
    }
}
