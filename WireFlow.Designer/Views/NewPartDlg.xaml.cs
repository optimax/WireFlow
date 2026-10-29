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

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;
            this.DialogResult = true;
        }

        public new string Name
        {
            get { return NameBox.Text.Trim(); }
            set { NameBox.Text = value; }
        }


    }
}
