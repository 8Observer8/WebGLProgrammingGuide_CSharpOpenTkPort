namespace _03_HelloPoint2
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
