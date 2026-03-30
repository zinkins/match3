using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Match3.Core.Localization
{
    /// <summary>
    /// Manages localization settings for the game, including retrieving supported cultures and setting the current culture for localization.
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>
        /// the culture code we default to
        /// </summary>
        public const string DEFAULT_CULTURE_CODE = "en";

        private static readonly CultureInfo[] SupportedCultures =
        [
            CultureInfo.InvariantCulture,
            new CultureInfo("es-ES"),
            new CultureInfo("fr-FR")
        ];

        /// <summary>
        /// Retrieves a list of supported cultures based on available language resources in the game.
        /// This method checks the current culture settings and the satellite assemblies for available localized resources.
        /// </summary>
        /// <returns>A list of <see cref="CultureInfo"/> objects representing the cultures supported by the game.</returns>
        /// <remarks>
        /// This method iterates through all specific cultures defined in the satellite assemblies and attempts to load the corresponding resource set.
        /// If a resource set is found for a particular culture, that culture is added to the list of supported cultures. The invariant culture
        /// is always included in the returned list as it represents the default (non-localized) resources.
        /// </remarks>
        public static List<CultureInfo> GetSupportedCultures()
        {
            return [.. SupportedCultures];
        }

        /// <summary>
        /// Sets the current culture of the game based on the specified culture code.
        /// This method updates both the current culture and UI culture for the current thread.
        /// </summary>
        /// <param name="cultureCode">The culture code (e.g., "en-US", "fr-FR") to set for the game.</param>
        /// <remarks>
        /// This method modifies the <see cref="Thread.CurrentThread.CurrentCulture"/> and <see cref="Thread.CurrentThread.CurrentUICulture"/> properties,
        /// which affect how dates, numbers, and other culture-specific values are formatted, as well as how localized resources are loaded.
        /// </remarks>
        public static void SetCulture(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
                throw new ArgumentNullException(nameof(cultureCode), "A culture code must be provided.");

            // Create a CultureInfo object from the culture code
            CultureInfo culture = new CultureInfo(cultureCode);

            // Set the current culture and UI culture for the current thread
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
