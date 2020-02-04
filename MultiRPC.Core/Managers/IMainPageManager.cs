using System;

namespace MultiRPC.Core.Managers
{
    /// <summary>
    /// Manager for the main page that the MultiRPC will have
    /// </summary>
    /// <typeparam name="TPage">What type the page is</typeparam>
    public interface IMainPageManager<TPage> where TPage : class
    {
        /// <summary>
        /// Add a page as a main page
        /// </summary>
        /// <param name="page">Page to add</param>
        /// <returns>If we was successful at adding the main page</returns>
        bool AddMainPage(TPage page);

        /// <summary>
        /// Remove a main page
        /// </summary>
        /// <param name="pageType">Page that you want to remove from it's type</param>
        /// <returns>If we was successful at removing the main page</returns>
        bool RemoveMainPage(Type pageType);

        /// <summary>
        /// Add a page as a main page with sub pages
        /// </summary>
        /// <param name="mainPage">Page that you want to remove from it's type</param>
        /// <param name="subPages">pages to add</param>
        /// <returns>If we was successful at removing the main page and sub pages</returns>
        bool AddPages(TPage mainPage, params TPage[] subPages);

        /// <summary>
        /// Add a page to a main page
        /// </summary>
        /// <param name="mainPageType">Page that you want <see cref="subPages"/> to also contain from it's type</param>
        /// <param name="subPages">Pages to add</param>
        /// <returns>If we was successful at removing the sub page</returns>
        bool AddSubPages(Type mainPageType, params TPage[] subPages);

        /// <summary>
        /// Remove a page from a main page
        /// </summary>
        /// <param name="mainPageType">Page that you want <see cref="subPages"/> to be removed from it's type</param>
        /// <param name="subPages">Pages to remove</param>
        /// <returns>If we was successful at removing the sub page</returns>
        bool RemoveSubPages(Type mainPageType, params TPage[] subPages);

        /// <summary>
        /// Adds a overlay to the main page
        /// </summary>
        /// <param name="page">The page that will act as the overlay</param>
        /// <returns>If we was successful at making the overlay</returns>
        bool AddOverlay(TPage page);
    }
}
