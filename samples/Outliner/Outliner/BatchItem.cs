using CommunityToolkit.Mvvm.ComponentModel;

namespace Outliner
{
    /// <summary>
    /// One row in the batch processing list — a single prose file with
    /// per-file status, cost, and error message.
    /// </summary>
    public sealed class BatchItem : ObservableObject
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;

        private string _status = "Pending";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private decimal _cost;
        public decimal Cost
        {
            get => _cost;
            set
            {
                if (SetProperty(ref _cost, value))
                    OnPropertyChanged(nameof(CostDisplay));
            }
        }

        public string CostDisplay => _cost > 0 ? $"${_cost:F4}" : string.Empty;

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
    }
}
