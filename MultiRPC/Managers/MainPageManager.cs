using System;
using System.Windows.Controls;
using MultiRPC.GUI;
using System.Windows;
using System.Windows.Media.Animation;
using MultiRPC.Core.Managers;
using MultiRPC.Core;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.Managers
{
    /// <inheritdoc cref="IMainPageManager{TPage}"/>
    class MainPageManager : IMainPageManager<Page>
    {
        private Button activeButton;

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="page">The page to go to when clicking the button that will be made</param>
        /// <returns>If we was able to make the button for the main page</returns>
        public bool AddMainPage(Page page)
        {
            //ToDo: Cleanup
            if ((App.Current.MainWindow as MainWindow).frmContent.Content is MainPage mainPage)
            {
                var image = new Image();
                image.SetResourceReference(Image.SourceProperty, App.Manager.MultiRPCIcon.EnumToString((page as PageWithIcon)?.IconName ?? Core.Enums.MultiRPCIcons.Unknown));

                var button = new Button
                {
                    Content = image,
                    Name = "btn" + page.GetType().Name,
                    Tag = page,
                };
                if (page is PageWithIcon pageWithIcon && !string.IsNullOrWhiteSpace(pageWithIcon.JsonContent))
                {
                    Settings.Current.NavigationTooltipsStatusChanged += (_, __) => NavigationTooltipsNeedsChanging(button);
                    Settings.Current.LanguageChanged += (_, __) => NavigationTooltipsNeedsChanging(button);
                    NavigationTooltipsNeedsChanging(button);
                }
                mainPage.RegisterName(button.Name, button);
                button.SetResourceReference(FrameworkElement.StyleProperty, "NavButton");
                button.Click += (_, __) =>
                {
                    if (activeButton != button)
                    {
                        activeButton?.SetResourceReference(FrameworkElement.StyleProperty, "NavButton");
                        (activeButton?.Content as Image)?.SetResourceReference(Image.SourceProperty, App.Manager.MultiRPCIcon.EnumToString((activeButton?.Tag as PageWithIcon)?.IconName ?? Core.Enums.MultiRPCIcons.Unknown));
                    }
                    button.SetResourceReference(FrameworkElement.StyleProperty, "NavButtonSelected");
                    activeButton = button;
                    image.SetResourceReference(Image.SourceProperty, (App.Manager.MultiRPCIcon as MultiRPCIcons).EnumToString((page as PageWithIcon)?.IconName ?? Core.Enums.MultiRPCIcons.Unknown, true));

                    mainPage.frmContent.Navigate(page);
                };
                button.PreviewMouseLeftButtonDown += (sender, args) => _ = button.ThicknessAnimation(new Thickness(2), button.Margin, fillBehavior: FillBehavior.HoldEnd);
                button.PreviewMouseLeftButtonUp += (sender, args) => _ = button.ThicknessAnimation(new Thickness(0), button.Margin, ease: new BounceEase(), fillBehavior: FillBehavior.HoldEnd);
                mainPage.spMainPages.Children.Add(button);
            }
            else
            {
                throw new Exception($"{nameof(MainWindow)} hasn't loaded {nameof(MainPage)} yet!!!");
            }

            return true;
        }

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="mainPage">The page to go to when clicking the button that will be made</param>
        /// <param name="subPages">The pages that the main page will allow you to go to</param>
        /// <returns>If we was able to make the button for the main page</returns>
        public bool AddPages(Page mainPage, params Page[] subPages)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="mainPageType">The pages type that has been added before</param>
        /// <param name="subPages">The pages that the main page will allow you to go to</param>
        /// <returns>If we was able to add the pages</returns>
        public bool AddSubPages(Type mainPageType, params Page[] subPages)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="pageType">The pages type that needs to get removed</param>
        public bool RemoveMainPage(Type pageType)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="mainPageType">The pages type that contains the subpages to remove</param>
        /// <returns>If we was able to remove the sub pages</returns>
        public bool RemoveSubPages(Type mainPageType, params Page[] subPages)
        {
            throw new NotImplementedException();
        }

        public bool AddOverlay(Page page)
        {
            throw new NotImplementedException();
        }

        private void NavigationTooltipsNeedsChanging(Button btn) =>        
            btn.ToolTip = Settings.Current.ShowNavigationTooltips && btn.Tag is PageWithIcon page && !string.IsNullOrWhiteSpace(page.JsonContent) ? 
            new ToolTip($"{LanguagePicker.GetLineFromLanguageFile(page.JsonContent)} {LanguagePicker.GetLineFromLanguageFile("Page")}")
            : null;
    }
}
