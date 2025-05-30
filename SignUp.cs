﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Mail;
using System.Net;
using System.Net.NetworkInformation;

namespace Activity_7
{
    public partial class SignUp : Form
    {
        public SignUp()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            SetRoundedRegion(20);
        }

        private void SetRoundedRegion(int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
            path.AddArc(new Rectangle(this.Width - radius, 0, radius, radius), 270, 90);
            path.AddArc(new Rectangle(this.Width - radius, this.Height - radius, radius, radius), 0, 90);
            path.AddArc(new Rectangle(0, this.Height - radius, radius, radius), 90, 90);
            path.CloseFigure();

            this.Region = new Region(path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRoundedRegion(30); // keeps it rounded on resize
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // No functionality
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // No functionality
        }

        private void SignUp_Load(object sender, EventArgs e)
        {
            // No functionality
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool VerifyEmailDomain(string email)
        {
            try
            {
                // Extract domain from email
                string domain = email.Split('@')[1];

                // Check if domain has valid MX records
                var mxRecords = Dns.GetHostAddresses(domain);
                if (mxRecords.Length == 0)
                {
                    MessageBox.Show("Invalid email domain. Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Try to connect to the SMTP server
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        // Try to connect to common SMTP servers for the domain
                        string[] commonSmtpServers = {
                            $"smtp.{domain}",
                            $"mail.{domain}",
                            $"smtp.gmail.com",  // For Gmail
                            $"smtp.office365.com",  // For Outlook
                            $"smtp.mail.yahoo.com"  // For Yahoo
                        };

                        bool connected = false;
                        foreach (var server in commonSmtpServers)
                        {
                            try
                            {
                                client.Connect(server, 587, SecureSocketOptions.StartTls);
                                connected = true;
                                break;
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if (!connected)
                        {
                            MessageBox.Show("Could not verify email domain. Please enter a valid email address.", "Email Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }

                        client.Disconnect(true);
                        return true;
                    }
                    catch
                    {
                        MessageBox.Show("Could not verify email domain. Please enter a valid email address.", "Email Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Invalid email format. Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(username.Text) ||
                string.IsNullOrWhiteSpace(email.Text) ||
                string.IsNullOrWhiteSpace(password.Text))
            {
                MessageBox.Show("Please fill in all fields!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verify email domain
            if (!VerifyEmailDomain(email.Text))
            {
                return;
            }

            // Database connection
            string connStr = "server=localhost;user=root;password=mykz;database=zeereal_artspace";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Check if email already exists
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @Email";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@Email", email.Text);

                    int emailExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (emailExists > 0)
                    {
                        MessageBox.Show("Email already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Hash the password
                    string hashedPassword = HashPassword(password.Text);

                    // Insert the new user into the database with the hashed password
                    string insertQuery = "INSERT INTO users (username, email, password) VALUES (@Username, @Email, @Password)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@Username", username.Text);
                    insertCmd.Parameters.AddWithValue("@Email", email.Text);
                    insertCmd.Parameters.AddWithValue("@Password", hashedPassword);

                    insertCmd.ExecuteNonQuery();
                    MessageBox.Show("Account created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Redirect to Login form
                    this.Hide();
                    Login loginForm = new Login();
                    loginForm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Login loginForm = new Login();
            loginForm.Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // No functionality
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            button2.FlatStyle = FlatStyle.Flat;
            button2.FlatAppearance.BorderSize = 0;
            button2.BackColor = Color.Transparent;
            button2.ForeColor = Color.Black; // or White
            button2.Font = new Font("Arial", 10, FontStyle.Bold);
        }
    }
}