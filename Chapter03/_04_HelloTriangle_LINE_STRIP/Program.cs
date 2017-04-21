namespace _04_HelloTriangle_LINE_STRIP
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
