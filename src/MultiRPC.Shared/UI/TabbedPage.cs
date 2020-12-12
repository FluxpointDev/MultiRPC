using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiRPC.Shared.UI
{
    public class TabbedPage : LocalizablePage
    {
        public TabbedPage(bool defaultPage = false)
            : base()
        {
            IsDefaultPage = defaultPage;
        }

        public virtual string LocalizableName { get; }

        public PageContainer Container { get; private set; }

        public bool IsDefaultPage { get; }

        internal void SetContainer(PageContainer pageContainer)
        {
            Container = pageContainer;
        }
    }
}
