// <copyright file="LocaleUtils.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: LocaleUtils.cs
// Version: 0.1.0

namespace CS2Shared.RiverMochi
{
    using Colossal.Localization;
    using Game.SceneFlow;
    using System;
    using System.Globalization;

    public static class LocaleUtils
    {
        public static string Localize(string entryId, string fallback)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return fallback;
            }

            try
            {
                // During early load the active dictionary may be unavailable; fallback keeps UI safe.
                LocalizationDictionary? dict = GameManager.instance.localizationManager.activeDictionary;
                if (dict != null && dict.TryGetValue(entryId, out string value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            catch
            {
            }

            return fallback;
        }

        public static string SafeFormat(string entryId, string fallbackFormat, params object[] args)
        {
            string format = Localize(entryId, fallbackFormat);

            try
            {
                // Locale strings are hand-edited, so tolerate a bad placeholder count.
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            catch (FormatException)
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, fallbackFormat, args);
                }
                catch
                {
                    return fallbackFormat;
                }
            }
            catch
            {
                return fallbackFormat;
            }
        }

        public static string FormatN0(long value)
        {
            // Shared number formatting keeps UI rows and log reports visually consistent.
            return value.ToString("N0", CultureInfo.CurrentCulture);
        }
    }
}
