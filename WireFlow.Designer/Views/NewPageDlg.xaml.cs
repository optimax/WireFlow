using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WireFlow.Views
{
    /// <summary>
    /// Interaction logic for NewPageDlg.xaml
    /// </summary>
    public partial class NewPageDlg : Window
    {
        private ILayoutListProvider layoutsProvider;


        public string PageName
        {
            get { return NameBox.Text.Trim(); }
            set { NameBox.Text = value; }
        }


        public string LayoutName => LayoutNameBox.Text.Trim();


        /// <summary>
        /// Checks the name typed, with the layout chosen, as OK is pressed: the reason the page or layout cannot be made (a
        /// name another has, say), or null. A reason is shown over the dialog, which stays open for the name to be corrected.
        /// </summary>
        public Func<string, string, string> CheckName { get; set; }

        /// <summary>Shows the reason a name was refused, over the dialog: a message box, which a script hosting the dialog replaces.</summary>
        public Action<Window, string> ShowRefusal { get; set; } =
            (dialog, reason) => MessageBox.Show(dialog, reason, dialog.Title, MessageBoxButton.OK, MessageBoxImage.Warning);


        public NewPageDlg(Window owner, ILayoutListProvider layoutsProvider, string pageName = null)
        {
            if (layoutsProvider == null)
                throw new ArgumentNullException(nameof(layoutsProvider));
            Owner = owner;

            this.layoutsProvider = layoutsProvider;
            InitializeComponent();
            SourceInitialized += (s, e) => MaxServices.Core.Themes.AppTheme.ApplyTitleBar(this);
            this.SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
            Loaded += (sender, e) => {
                PageName = pageName;
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            };
            PopulateLayouts();

        }


        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PageName))
                return;
            // A name that cannot be used is refused here, while the dialog is open, and the dialog stays for it to be corrected
            string refusal = CheckName?.Invoke(PageName, LayoutName);
            if (refusal != null)
            {
                ShowRefusal(this, refusal);
                NameBox.Focus();
                NameBox.SelectAll();
                return;
            }
            this.DialogResult = true;
        }


        private void PopulateLayouts()
        {
            LayoutNameBox.ItemsSource = layoutsProvider.LayoutNames;
        }

    }




    public class NameValidator : ValidationRule
    {
        public override ValidationResult Validate
          (object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "value cannot be empty.");
            else
            {
                if (value.ToString().Length > 3)
                    return new ValidationResult
                    (false, "Name cannot be more than 3 characters long.");
            }
            return ValidationResult.ValidResult;
        }
    }

}
