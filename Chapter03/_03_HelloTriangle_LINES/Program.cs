namespace _03_HelloTriangle_LINES
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
