namespace _06_HelloQuad
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
