using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;

namespace Activity_7
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize application configuration
            ApplicationConfiguration.Initialize();

            // Check if this is a password reset request
            if (args.Length > 0 && args[0].StartsWith("http://localhost/reset?token="))
            {
                string token = args[0].Split('=')[1];
                Application.Run(new ResetPassword(token));
                return;
            }

            // Define your MySQL connection string
            string connectionString = "server=localhost;user id=root;password=mykz;database=zeereal_artspace;";

            // Attempt to establish a connection to the MySQL database
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Silent success
                }
            }
            catch (MySqlException ex)
            {
                // Log error to a file instead of showing a message box
                try
                {
                    File.AppendAllText("error.log", $"{DateTime.Now}: Database connection failed - {ex.Message}{Environment.NewLine}");
                }
                catch
                {
                    // Optional: silently ignore if logging also fails
                }
                return; // Exit the application if the connection fails
            }

            // Run the main application form
            Application.Run(new Login());
        }
    }
}
