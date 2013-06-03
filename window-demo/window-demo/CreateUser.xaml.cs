 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace window_demo
{
    /// <summary>
    /// Interaction logic for CreateUser.xaml
    /// </summary>
    public partial class CreateUser : Window
    {
        public CreateUser()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
        private void cancelClick(object sender, RoutedEventArgs e) //if user clicks cancel button
        {
            Close();
        }
        private void okClick(object sender, RoutedEventArgs e) //if user clicks ok button
        {
            if (userNameBox.Text.Length == 0)
            {
                errormessage.Text = "Please enter an email.";
                userNameBox.Focus();
            }
            if (passwordBox.Password.Length == 0)
            {
                errormessage.Text = "Please enter your password.";
                passwordBox.Focus();
            }

            if (passwordConfirmBox.Password.Length == 0)
            {
                errormessage.Text = "Please enter your confirmed password.";
                passwordConfirmBox.Focus();
            }

            if (emailBox.Text.Length == 0)
            {
                errormessage.Text = "Please enter an email.";
                emailBox.Focus();
            }
            else if (!Regex.IsMatch(emailBox.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                errormessage.Text = "Please enter a valid email.";
                emailBox.Select(0, emailBox.Text.Length);
                emailBox.Focus();
            }
            else
            {
                string userName = userNameBox.Text;
                string email = emailBox.Text;
                string password = passwordBox.Password;

                if (passwordBox.Password.Length == 0)
                {
                    errormessage.Text = "Please enter password.";
                    passwordBox.Focus();
                }

                else if (passwordConfirmBox.Password.Length == 0)
                {
                    errormessage.Text = "Enter Confirm password.";
                    passwordConfirmBox.Focus();
                }

                else if (passwordBox.Password != passwordConfirmBox.Password)
                {
                    errormessage.Text = "Confirm password must be the same as password.";
                    passwordConfirmBox.Focus();
                }

                else
                {
                    MySqlConnection con = null;
                    try
                    {
                        errormessage.Text = "";
                        string address = emailBox.Text;
                        Random random = new Random();
                        int EmployerId = random.Next(); //to create a random ID to be inserted into database table
                        con = new MySqlConnection(@"server=localhost;database=users;userid=root;password=;");
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("Insert into employeetable(EmployeeId,UserName,EmailAddress, Password,isAdmin) values('" + EmployerId + "','" + userName + "','" + email + "','" + password + "','" + 0 + "')", con);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("You have registered successfully.");
                        Close();
                    }
                    catch (MySqlException err) //capture and display any MySql errors that will occur
                    {

                        MessageBox.Show("Error: " + err.ToString());
                    }
                    finally
                    {
                        if (con != null)
                        {
                            con.Close(); //safely close the connection
                        }
                    }
                }
            }
        }
    }
}