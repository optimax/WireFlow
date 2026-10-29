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


        public NewPageDlg(Window owner, ILayoutListProvider layoutsProvider, string pageName = null)
        {
            if (layoutsProvider == null)
                throw new ArgumentNullException(nameof(layoutsProvider));
            Owner = owner;

            this.layoutsProvider = layoutsProvider;
            InitializeComponent();
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
            this.DialogResult = true;
        }


        private void PopulateLayouts()
        {
            LayoutNameBox.ItemsSource = layoutsProvider.Layouts;
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
