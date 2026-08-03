namespace BookSmith.UI.ViewModels.Main;

public class ProcessingViewModel : ViewModelBase
{
    private string _fileName = string.Empty;
    private double _progressValue = 20;
    private string _statusText = "Processing PDF...";

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
}
