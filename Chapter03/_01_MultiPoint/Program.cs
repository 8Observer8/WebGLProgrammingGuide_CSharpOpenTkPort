namespace _01_MultiPoint
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
