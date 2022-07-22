using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LatokenAPIWpfClient
{
    public class InputBox
    {

        Window Box = new Window();//window for the inputbox
        StackPanel sp1 = new StackPanel();// items container
        string title = "InputBox";//title as heading
        string boxcontent;//title
        string defaulttext = "Provide the input here...";//default textbox content
        string errormessage = "Invalid input";//error messagebox content
        string errortitle = "Error";//error messagebox heading title
        string okbuttontext = "OK";//Ok button content
        string cancelbuttontext = "Cancel";//Ok button content
        TextBox input = new TextBox();
        Button okButton = new Button();
        Button cancelButton = new Button();
        bool inputreset = false;

        public InputBox(string content)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            windowdef();
        }

        public InputBox(string content, string Htitle, string DefaultText)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            try
            {
                title = Htitle;
            }
            catch
            {
                title = "Error!";
            }
            try
            {
                defaulttext = DefaultText;
            }
            catch
            {
                DefaultText = "Error!";
            }
            windowdef();
        }

        public InputBox(string content, string Htitle)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            try
            {
                title = Htitle;
            }
            catch
            {
                title = "Error!";
            }
            windowdef();
        }

        private void windowdef()// window building - check only for window size
        {
            Box.Height = 175;// Box Height
            Box.Width = 300;// Box Width
            Box.Title = title;
            Box.Content = sp1;
            Box.ResizeMode = ResizeMode.NoResize;
            Box.Closing += Box_Closing;
            TextBlock content = new TextBlock();
            content.TextWrapping = TextWrapping.Wrap;
            content.Background = null;
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.Text = boxcontent;
            content.Margin = new Thickness(20, 5, 20, 5);
            sp1.Children.Add(content);

            input.HorizontalAlignment = HorizontalAlignment.Center;
            input.Text = defaulttext;
            input.MinWidth = 200;
            input.MouseEnter += input_MouseDown;
            input.Margin = new Thickness(20, 5, 20, 5);
            input.Height = 25;
            sp1.Children.Add(input);
            okButton.Width = 70;
            okButton.Height = 30;
            okButton.Click += ok_Click;
            okButton.Content = okbuttontext;
            okButton.HorizontalAlignment = HorizontalAlignment.Center;
            okButton.Margin = new Thickness(20, 20, 20, 20);
            cancelButton.Width = 70;
            cancelButton.Height = 30;
            cancelButton.Click += Cancel_Click;
            cancelButton.Content = cancelbuttontext;
            cancelButton.HorizontalAlignment = HorizontalAlignment.Center;
            cancelButton.Margin = new Thickness(20, 20, 20, 20);

            StackPanel buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            buttonStackPanel.Margin = new Thickness(20, 0, 20, 5);
            buttonStackPanel.Children.Add(okButton);
            buttonStackPanel.Children.Add(cancelButton);
            sp1.Children.Add(buttonStackPanel);

        }

        void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (input.Text == defaulttext)
                input.Text = string.Empty;
        }

        private void input_MouseDown(object sender, MouseEventArgs e)
        {
            if ((sender as TextBox).Text == defaulttext && inputreset == false)
            {
                (sender as TextBox).Text = null;
                inputreset = true;
            }
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            if (input.Text == defaulttext || input.Text == "")
                MessageBox.Show(errormessage, errortitle);
            else
            {
                Box.Close();
            }
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            input.Text = string.Empty;

            Box.Close();
        }

        public string ShowDialog()
        {
            Box.ShowDialog();
            return input.Text;
        }
    }
}
