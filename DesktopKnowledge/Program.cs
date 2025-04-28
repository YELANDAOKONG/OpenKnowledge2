namespace DesktopKnowledge;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    internal static void Exit(int code, bool force = false)
    {
        if (force)
        {
            Environment.Exit(code);
        }
        else
        {
            Application.Exit();
        }
    }
}