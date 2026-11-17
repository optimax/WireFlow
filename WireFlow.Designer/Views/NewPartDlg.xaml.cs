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

namespace WireFlow.Views
{
    /// <summary>
    /// Interaction logic for NewPartDlg.xaml
    /// </summary>
    public partial class NewPartDlg : Window
    {
        public NewPartDlg(Window owner, string partName = null)
        {
            InitializeComponent();
            SourceInitialized += (s, e) => MaxServices.Core.Themes.AppTheme.ApplyTitleBar(this);
            Owner = owner;
            this.SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
            Loaded += (sender, e) => {
                Name = partName;
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            };
        }

        /// <summary>
        /// Checks the name typed as OK is pressed: the reason the part cannot be made (a name another has), or null. A reason
        /// is shown over the dialog, which stays open for the name to be corrected.
        /// </summary>
        public Func<string, string> CheckName { get; set; }

        /// <summary>Shows the reason a name was refused, over the dialog: a message box, which a script hosting the dialog replaces.</summary>
        public Action<Window, string> ShowRefusal { get; set; } =
            (dialog, reason) => MessageBox.Show(dialog, reason, dialog.Title, MessageBoxButton.OK, MessageBoxImage.Warning);


        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;
            // A name that cannot be used is refused here, while the dialog is open, and the dialog stays for it to be corrected
            string refusal = CheckName?.Invoke(Name);
            if (refusal != null)
            {
                ShowRefusal(this, refusal);
                NameBox.Focus();
                NameBox.SelectAll();
                return;
            }
            this.DialogResult = true;
        }

        public new string Name
        {
            get { return NameBox.Text.Trim(); }
            set { NameBox.Text = value; }
        }


    }
}
