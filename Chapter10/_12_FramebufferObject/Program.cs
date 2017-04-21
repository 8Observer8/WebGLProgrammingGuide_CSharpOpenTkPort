namespace _12_FramebufferObject
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var mainWindow = new MainWindow())
            {
                mainWindow.Run(60);
            }
        }
    }
}
