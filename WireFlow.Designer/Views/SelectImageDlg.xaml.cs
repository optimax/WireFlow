using System.Windows;

namespace WireFlow.Views
{
    /// <summary>
    /// Interaction logic for SelectImageDlg.xaml
    /// </summary>
    public partial class SelectImageDlg : Window
    {
        SelectImageViewModel VM { get; }

        public SelectImageDlg(Window owner, SelectImageViewModel vm)
        {
            InitializeComponent();
            Owner = owner;
            VM = vm;
            DataContext = VM;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            VM.DialogResult = true;
            this.DialogResult = true;
        }


        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            VM.DialogResult = false;
            this.DialogResult = false;
        }


        private void ListBoxImages_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var source = e.Source;



            VM.DialogResult = true;
            this.DialogResult = true;
        }
    }
}
