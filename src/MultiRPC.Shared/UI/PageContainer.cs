using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Input;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MultiRPC.Core;
using Microsoft.UI.Xaml.Media.Animation;

namespace MultiRPC.Shared.UI
{
    public partial class PageContainer : Page
    {
        public PageContainer(TabbedPage[] pages)
        {
            Pages = pages;

            var tabToPress = 0;
            for (var i = 0; i < pages.LongLength; i++)
            {
                pages[i].SetContainer(this);

                if (pages[i].IsDefaultPage && tabToPress == 0)
                {
                    tabToPress = i;
                }

                var grid = new Grid
                {
                    Margin = new Thickness(10, 0, 0, 0),
                    Background = new SolidColorBrush(Colors.Transparent),
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
                    Tag = pages[i]
                };
                var textBlock = new TextBlock
                {
                    Text = LanguagePicker.GetLineFromLanguageFile(pages[i].LocalizableName)
                };
                var rectangle = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Height = 0,
                    Margin = new Thickness(-2, 3, -2, 0),
                    Name = $"rec{i}",
                };
                Grid.SetRow(rectangle, 1);
                rectangle.Fill = new SolidColorBrush(Colors.White);

                grid.Children.Add(textBlock);
                grid.Children.Add(rectangle);
                grid.PointerEntered += Grid_PointerEntered;
                grid.PointerExited += Grid_PointerExited;
                grid.PointerPressed += Grid_PointerPressed;
                Tabs.Children.Add(grid);
            }

            MouseDownLogic(Tabs.Children[tabToPress] as Grid);
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            MouseDownLogic((Grid)sender);
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            MouseEnterLeaveLogic((Grid)sender, 0);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            MouseEnterLeaveLogic((Grid)sender, 1);
        }

        private async Task MouseEnterLeaveLogic(Grid grid, double to)
        {
            if (_activeGrid != grid)
            {
                var rec = (Rectangle) grid.Children[1];
                rec.Height = to;
            }
        }

        private async Task MouseDownLogic(Grid grid)
        {
            Rectangle rec;
            if (_activeGrid != null)
            {
                rec = (Rectangle)_activeGrid.Children[1];
                rec.Height = 0;
            }

            _activeGrid = grid;

            rec = (Rectangle) grid.Children[1];
            rec.Height = 3;
            ActiveContent.Content = grid.Tag;
        }

        public ScrollViewer MakeCorneredFrame() => new()
        {
            Background = (Brush)Application.Current.Resources["Colour1"],
            Content = new Border()
            {
                Background = (Brush)Application.Current.Resources["Colour2"],
                Child = this.ActiveContent,
                CornerRadius = new CornerRadius(15, 0, 0, 0)
            }
        };

        public TabbedPage[] Pages { get; }

        //TODO: Change to WrapPanel when it's a thing in WinUI
        public StackPanel Tabs { get; } = new StackPanel();

        public Frame ActiveContent = new();

        private Grid _activeGrid;
    }
}
