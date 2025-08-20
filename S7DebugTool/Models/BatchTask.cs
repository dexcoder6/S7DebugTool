using CommunityToolkit.Mvvm.ComponentModel;

namespace S7DebugTool.Models
{
    public partial class BatchTask : ObservableObject
    {
        [ObservableProperty]
        private bool isEnabled = true;

        [ObservableProperty]
        private bool isSelected = false;

        [ObservableProperty]
        private string operation = "读取";

        [ObservableProperty]
        private string area = "DB";

        [ObservableProperty]
        private int dbNumber = 1;

        [ObservableProperty]
        private int address = 0;

        [ObservableProperty]
        private int length = 10;

        [ObservableProperty]
        private string data = "";

        [ObservableProperty]
        private string result = "";
    }
}