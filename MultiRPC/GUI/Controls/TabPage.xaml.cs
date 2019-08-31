using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MultiRPC.Functions;

namespace MultiRPC.GUI.Controls
{
    /// <summary>
    /// Interaction logic for TabPage.xaml
    /// </summary>
    public partial class TabPage : Page
    {
        private Grid _selectedGrid;
        public TabItem[] Tabs;

        public TabPage(TabItem[] tabs)
        {
            var gridToPress = 0;

            InitializeComponent();
            Tabs = tabs;
            for (var i = 0; i < tabs.LongLength; i++)
            {
                if (tabs[i].IsActive && gridToPress == 0)
                {
                    gridToPress = i;
                }

                Grid grid = new Grid
                {
                    Margin = new Thickness(10, 0, 0, 0),
                    Background = Brushes.Transparent,
                    RowDefinitions =
                    {
                        new RowDefinition
                        {
                            Height = new GridLength(0, GridUnitType.Auto)
                        },
                        new RowDefinition
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        }
                    },
                    Tag = tabs[i].Page
                };
                TextBlock textBlock = new TextBlock
                {
                    Text = tabs[i].TabName
                };
                Rectangle rectangle = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Height = 0,
                    Margin = new Thickness(-2, 3, -2, 0),
                    Name = $"rec{i}",
                };
                Grid.SetRow(rectangle, 1);
                rectangle.SetResourceReference(Shape.FillProperty, "AccentColour3SCBrush");
                RegisterName($"rec{i}", rectangle);

                grid.Children.Add(textBlock);
                grid.Children.Add(rectangle);
                grid.MouseEnter += Grid_MouseEnter;
                grid.MouseLeave += Grid_MouseLeave;
                grid.MouseDown += Grid_MouseDown;
                spTabContainer.Children.Add(grid);
            }

            var mouseDownEvent =
                new MouseButtonEventArgs(Mouse.PrimaryDevice, (int) DateTime.Now.Ticks, MouseButton.Left)
                {
                    RoutedEvent = MouseDownEvent,
                    Source = this
                };
            spTabContainer.Children[gridToPress].RaiseEvent(mouseDownEvent);
        }

        /// <summary>
        /// Updates Tabs Menu Text
        /// </summary>
        /// <param name="tabNames">New tab Names (give them in the order you gave the tabs or the tabs will have the wrong text!!!)</param>
        /// <returns></returns>
        public Task UpdateText(params string[] tabNames)
        {
            for (var i = 0; i < spTabContainer.Children.Count; i++)
            {
                var grid = (Grid) spTabContainer.Children[i];
                ((TextBlock) grid.Children[0]).Text = tabNames[i];
            }

            return Task.CompletedTask;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDownLogic((Grid) sender);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseEnterLeaveLogic((Grid) sender, 0);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnterLeaveLogic((Grid) sender, 1);
        }

        private async Task MouseEnterLeaveLogic(Grid grid, double to)
        {
            if (_selectedGrid != grid)
            {
                var rec = (Rectangle) grid.Children[1];
                Animations.DoubleAnimation(rec, to, rec.Height, propertyPath: new PropertyPath(HeightProperty),
                    duration: new Duration(TimeSpan.FromSeconds(0.15)));
            }
        }

        private async Task MouseDownLogic(Grid grid)
        {
            Rectangle rec;
            if (_selectedGrid != null)
            {
                rec = (Rectangle) _selectedGrid.Children[1];
                Animations.DoubleAnimation(rec, 0, rec.Height, propertyPath: new PropertyPath(HeightProperty),
                    duration: new Duration(TimeSpan.FromSeconds(0.15)));
            }

            _selectedGrid = grid;

            rec = (Rectangle) grid.Children[1];
            Animations.DoubleAnimation(rec, 3, rec.Height, propertyPath: new PropertyPath(HeightProperty),
                duration: new Duration(TimeSpan.FromSeconds(0.3)));
            frmContent.Navigate(grid.Tag);
        }
    }

    public class TabItem
    {
        public string TabName;
        public object Page;
        public bool IsActive;
    }
}