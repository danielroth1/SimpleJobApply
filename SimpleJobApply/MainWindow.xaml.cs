using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleJobApply
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _dragStartPoint;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        // Store the starting point of the drag operation
        private void ItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        // Handle the dragging operation
        private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            // Start drag-and-drop if the mouse has moved sufficiently
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged item
                ItemsControl itemsControl = sender as ItemsControl;
                var draggedItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

                if (draggedItem == null) return;

                // Begin drag-and-drop
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }
        }

        // Handle the drop operation
        private void ItemsControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Model.Paragraph)))
            {
                Model.Paragraph droppedData = e.Data.GetData(typeof(Model.Paragraph)) as Model.Paragraph;
                ItemsControl itemsControl = sender as ItemsControl;

                Point dropPosition = e.GetPosition(itemsControl);
                var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

                // Find the item to drop onto
                Model.Paragraph targetData = targetItem?.DataContext as Model.Paragraph;

                if (droppedData != null && targetData != null)
                {
                    int oldIndex = ((MainViewModel)DataContext).ParagraphDetails.IndexOf(droppedData);
                    int newIndex = ((MainViewModel)DataContext).ParagraphDetails.IndexOf(targetData);

                    // Move the item within the collection
                    if (oldIndex != newIndex)
                    {
                        ((MainViewModel)DataContext).ParagraphDetails.Move(oldIndex, newIndex);
                    }
                }
            }
        }

        // Helper method to find the parent ListBoxItem
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                if (current is Run)
                    current = ((Run)current).Parent;
                else if (current is Paragraph)
                    current = ((Paragraph)current).Parent;
                else if (current is FlowDocument)
                    current = ((FlowDocument)current).Parent;
                else
                    current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        private void mw_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit? Unsaved changes will be lost.", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            e.Cancel = result != MessageBoxResult.Yes;
        }
    }
}