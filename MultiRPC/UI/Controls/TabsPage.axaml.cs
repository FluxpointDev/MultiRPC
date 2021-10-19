﻿using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace MultiRPC.UI.Controls
{
    public partial class TabsPage : UserControl
    {
        private TabPage[] _pages = null!;
        public void AddTabs(params TabPage[] pages) => _pages = pages;
        
        protected virtual void Initialize()
        {
            InitializeComponent();
            stpTabs.Children.AddRange(_pages.Select(MakeTab));
        }

        private Rectangle? _activePageRectangle;
        private Control MakeTab(TabPage page)
        {
            var text = new TextBlock
            {
                [!TextBlock.TextProperty] = page.TabName.TextObservable.ToBinding(),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var rec = new Rectangle
            {
                Height = 0,
                Fill = Brushes.White
            };

            var stc = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    text,
                    rec
                }
            };

            stc.PointerLeave += (sender, args) =>
            {
                if (!Equals(rec, _activePageRectangle))
                {
                    rec.Height = 0;
                }
            };
            stc.PointerEnter += (sender, args) =>
            {
                if (!Equals(rec, _activePageRectangle))
                {
                    rec.Height = 1;
                }
            };
            stc.PointerPressed += (sender, args) =>
            {
                if (_activePageRectangle is not null)
                {
                    _activePageRectangle.Height = 0;
                }

                _activePageRectangle = rec;
                _activePageRectangle.Height = 3;
                content.Content = page;
            };
            return stc;
        }
    }

    public abstract class TabPage : UserControl
    {
        public abstract Language TabName { get; }
        
        public abstract void Initialize();
    }
}