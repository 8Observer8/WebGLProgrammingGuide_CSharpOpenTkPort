namespace _13_Shadow
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
