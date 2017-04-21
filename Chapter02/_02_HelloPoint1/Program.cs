namespace _02_HelloPoint1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var mainWindow = new MainWindow())
            {
                mainWindow.Run(30);
            }
        }
    }
}
