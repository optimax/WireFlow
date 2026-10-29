using System;

namespace WireFlow
{
    public abstract class BaseDialogViewModel : BaseViewModel
    {

        private bool? _dialogResult;

        public event EventHandler Closing;

        public string Title { get; set; }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(ref _dialogResult, value); }
        }

        public void Close()
        {
            Closing?.Invoke(this, EventArgs.Empty);
        }
    }

}
