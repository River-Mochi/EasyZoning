// <copyright file="ShellOpen.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License.
// See LICENSE-MIT in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Utils/ShellOpen.cs
// Version: 0.3.5
// Purpose: File/folder opening helpers for CS2 Options UI buttons.
// Based on River-Mochi shared CS2 utilities.
    using System;
    using System.Diagnostics;
    using System.IO;
    using Colossal.Logging;
    using UnityEngine;

namespace CS2Shared.RiverMochi
{
    public static class ShellOpen
    {
        private static ILog? s_Log;
        private static string s_ModId = string.Empty;
        private static string s_ModTag = "[CS2Shared]";
        private static readonly char[] s_ArgumentQuoteChars = { ' ', '\t', '"' };

        public static void Configure(ILog log, string modId, string modTag)
        {
            s_Log = log;

            if (!string.IsNullOrWhiteSpace(modId))
            {
                s_ModId = Path.GetFileNameWithoutExtension(modId.Trim());

                // Also configures LogUtils default logger, so short LogUtils.Info(...) calls work.
                LogUtils.Configure(s_ModId, log);
            }

            if (!string.IsNullOrWhiteSpace(modTag))
            {
                s_ModTag = modTag.Trim();
            }
        }

        public static void OpenModLogOrLogsFolder()
        {
            string logsFolder = GetLogsFolder();
            string logPath = string.Empty;

            if (!string.IsNullOrEmpty(logsFolder) && !string.IsNullOrEmpty(s_ModId))
            {
                logPath = Path.Combine(logsFolder, s_ModId + ".log");
            }

            // Prefer the exact mod log; fall back to the Logs folder before the first log exists.
            if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
            {
                OpenPathSafe(logPath, isFolder: false, logLabel: "OpenLogFile");
                return;
            }

            OpenPathSafe(logsFolder, isFolder: true, logLabel: "OpenLogsFolder");
        }

        public static string GetLogsFolder()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LogManager.kDefaultLogPath))
                {
                    return LogManager.kDefaultLogPath;
                }

                // Fallback for unusual environments where Colossal has not initialized the log path yet.
                string consoleLogPath = Application.consoleLogPath;
                if (string.IsNullOrEmpty(consoleLogPath))
                {
                    return string.Empty;
                }

                string? rootFolder = Path.GetDirectoryName(consoleLogPath);
                if (string.IsNullOrEmpty(rootFolder))
                {
                    return string.Empty;
                }

                string logsFolder = Path.Combine(rootFolder, "Logs");
                return Directory.Exists(logsFolder) ? logsFolder : rootFolder;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void OpenFolder(string folderPath, string logLabel = "OpenFolder")
        {
            OpenPathSafe(folderPath, isFolder: true, logLabel: logLabel);
        }

        public static void OpenFile(string filePath, string logLabel = "OpenFile")
        {
            OpenPathSafe(filePath, isFolder: false, logLabel: logLabel);
        }

        private static void OpenPathSafe(string path, bool isFolder, string logLabel)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    LogInfo(logLabel, "path is empty.");
                    return;
                }

                string fullPath = Path.GetFullPath(path);

                if (isFolder)
                {
                    if (!Directory.Exists(fullPath))
                    {
                        LogInfo(logLabel, "folder not found: " + fullPath);
                        return;
                    }
                }
                else if (!File.Exists(fullPath))
                {
                    LogInfo(logLabel, "file not found: " + fullPath);
                    return;
                }

                if (TryOpenWithOsShell(fullPath))
                {
                    return;
                }

                if (TryOpenWithUnityFileUrl(fullPath, isFolder))
                {
                    return;
                }

                LogInfo(logLabel, "could not open path: " + fullPath);
            }
            catch (Exception ex)
            {
                LogWarn(logLabel, "failed opening path: " + ex.GetType().Name + ": " + ex.Message, ex);
            }
        }

        private static bool TryOpenWithUnityFileUrl(string fullPath, bool isFolder)
        {
            try
            {
                string path = fullPath;

                if (isFolder &&
                    !path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) &&
                    !path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    path += Path.DirectorySeparatorChar;
                }

                Application.OpenURL(new Uri(path).AbsoluteUri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryOpenWithOsShell(string fullPath)
        {
            try
            {
                RuntimePlatform platform = Application.platform;

                if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start(new ProcessStartInfo(fullPath)
                    {
                        UseShellExecute = true,
                        ErrorDialog = false,
                        Verb = "open",
                    });

                    return true;
                }

                if (platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start(new ProcessStartInfo("open", QuoteArg(fullPath))
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });

                    return true;
                }

                if (platform == RuntimePlatform.LinuxPlayer || platform == RuntimePlatform.LinuxEditor)
                {
                    Process.Start(new ProcessStartInfo("xdg-open", QuoteArg(fullPath))
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });

                    return true;
                }

                Process.Start(new ProcessStartInfo(fullPath)
                {
                    UseShellExecute = true,
                    ErrorDialog = false,
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void LogInfo(string logLabel, string message)
        {
            ILog? log = s_Log;
            if (log == null)
            {
                return;
            }

            LogUtils.Info(log, () => s_ModTag + " " + logLabel + ": " + message);
        }

        private static void LogWarn(string logLabel, string message, Exception exception)
        {
            ILog? log = s_Log;
            if (log == null)
            {
                return;
            }

            LogUtils.Warn(log, () => s_ModTag + " " + logLabel + ": " + message, exception);
        }

        private static string QuoteArg(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            return value.IndexOfAny(s_ArgumentQuoteChars) >= 0
                ? "\"" + value.Replace("\"", "\\\"") + "\""
                : value;
        }
    }
}
