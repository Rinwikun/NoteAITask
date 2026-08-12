using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NoteAITask.Views
{
    public partial class NoteAIView : UserControl
    {
        public NoteAIView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}