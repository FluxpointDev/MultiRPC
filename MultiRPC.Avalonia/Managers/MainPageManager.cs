using System;
using MultiRPC.Core.Managers;
using MultiRPC.Core;
using Avalonia.Controls;
using MultiRPC.Avalonia;
using ToolTip = MultiRPC.Avalonia.GUI.Controls.ToolTip;

namespace MultiRPC.Managers
{
    /// <inheritdoc cref="IMainPageManager{TPage}"/>
    class MainPageManager : IMainPageManager<UserControl>
    {
        private Button activeButton;

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="page">The page to go to when clicking the button that will be made</param>
        /// <returns>If we was able to make the button for the main page</returns>
        public bool AddMainPage(UserControl page) => true;

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="mainPage">The page to go to when clicking the button that will be made</param>
        /// <param name="subPages">The pages that the main page will allow you to go to</param>
        /// <returns>If we was able to make the button for the main page</returns>
        public bool AddPages(UserControl mainPage, params UserControl[] subPages)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMainPageManager{TPage}"/>
        /// <param name="mainPageType">The pages type that has been added before</param>
        /// <param name="subPages">The pages that the main page will allow you to go to</param>
        /// <returns>If we was able to add the pages</returns>
        public bool AddSubPages(Type mainPageType, params UserControl[] subPages)
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
        public bool RemoveSubPages(Type mainPageType, params UserControl[] subPages)
        {
            throw new NotImplementedException();
        }

        public bool AddOverlay(UserControl page)
        {
            throw new NotImplementedException();
        }
    }
}
