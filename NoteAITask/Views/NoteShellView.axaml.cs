using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NoteAITask.Views
{
    public partial class NoteShellView : UserControl
    {
        public NoteShellView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}