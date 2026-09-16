// <copyright file="LogUtils.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License.
// See LICENSE-MIT in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Utils/LogUtils.cs
// Version: 0.7.2 based on River-Mochi shared CS2 utilities.
// Purpose: reduce Colossal logger NRE popups with direct .NET file logging.
// Why: routine mod logs bypass Colossal's logger write path.
//
// Setup in Mod.cs:
//   public static readonly ILog s_Log =
//       LogManager.GetLogger(kModId).SetShowsErrorsInUI(
//   #if DEBUG
//           true
//   #else
//           false
//   #endif
//       );
//
//   public void OnLoad(UpdateSystem updateSystem)
//   {
//       LogUtils.Configure(kModId, s_Log);
//       LogUtils.Info("Mod loaded.");
//   }
//
// Usage:
//   Simple one-time logs:       LogUtils.Info("message");
//   Warnings/errors:            LogUtils.Warn("message", ex); / LogUtils.Error("message", ex);
//   Lazy message construction:  LogUtils.Debug(() => $"message {value}");
//   Warn once:                  LogUtils.WarnOnce("key", () => "message");
//
// Helpers: Info/Warn/Error/Debug/Trace. TryLog accepts any Colossal Level.
    using System;
    using System.Collections.Generic;
    using System.Globalization;     // stable timestamp format
    using System.IO;
    using Colossal.Logging;

namespace CS2Shared.RiverMochi
{
    public static class LogUtils
    {
        private static readonly object s_WarnOnceLock = new();
        private static readonly object s_FileWriteLock = new();

        // Remembers WarnOnce keys so repeated calls do not rewrite the same warning.
        private static readonly HashSet<string> s_WarnOnceKeys = new(StringComparer.Ordinal);

        // Safety cap only; the HashSet starts empty and grows as unique WarnOnce keys are used.
        private const int kMaxWarnOnceKeys = 2048;

        // Used only if the passed ILog is null or its metadata throws during early startup/shutdown.
        private static string s_FallbackLogName = string.Empty;

        // Optional default logger for short calls such as LogUtils.Info("message").
        // It is remembered when a mod calls Configure(kModId, s_Log) or SetDefaultLog(s_Log).
        private static ILog? s_DefaultLog = null;

        // Optional one-time setup: pass your mod id so fallback writes can still find kModId.log.
        public static void Configure(string fallbackLogName)
        {
            if (string.IsNullOrWhiteSpace(fallbackLogName))
            {
                return;
            }

            string cleaned = Path.GetFileNameWithoutExtension(fallbackLogName.Trim());
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                s_FallbackLogName = cleaned;
            }
        }

        // Optional one-time setup with a default logger for concise LogUtils.Info("message") calls.
        public static void Configure(string fallbackLogName, ILog? defaultLog)
        {
            Configure(fallbackLogName);
            s_DefaultLog = defaultLog;
        }

        // Sets or replaces the remembered logger used by short calls.
        public static void SetDefaultLog(ILog? log)
        {
            s_DefaultLog = log;
        }

        // Test/mod-reload helper: lets a mod reset once-only warnings without restarting the game.
        public static void ClearWarnOnceKeys()
        {
            lock (s_WarnOnceLock)
            {
                s_WarnOnceKeys.Clear();
            }
        }

        // Simple one-time info log.
        public static void Info(string message)
        {
            TryLog(s_DefaultLog, Level.Info, () => message);
        }

        // Simple one-time info log with explicit logger.
        public static void Info(ILog? log, string message)
        {
            TryLog(log, Level.Info, () => message);
        }

        // Lazy info log; delays message construction until Info is enabled.
        public static void Info(Func<string> messageFactory)
        {
            TryLog(s_DefaultLog, Level.Info, messageFactory);
        }

        // Lazy info log with explicit logger.
        public static void Info(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Info, messageFactory);
        }

        // Simple recoverable warning.
        public static void Warn(string message, Exception? exception = null)
        {
            TryLog(s_DefaultLog, Level.Warn, () => message, exception);
        }

        // Simple recoverable warning with explicit logger.
        public static void Warn(ILog? log, string message, Exception? exception = null)
        {
            TryLog(log, Level.Warn, () => message, exception);
        }

        // Lazy recoverable warning.
        public static void Warn(Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(s_DefaultLog, Level.Warn, messageFactory, exception);
        }

        // Lazy recoverable warning with explicit logger.
        public static void Warn(ILog? log, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(log, Level.Warn, messageFactory, exception);
        }

        // Simple serious error.
        public static void Error(string message, Exception? exception = null)
        {
            TryLog(s_DefaultLog, Level.Error, () => message, exception);
        }

        // Simple serious error with explicit logger.
        public static void Error(ILog? log, string message, Exception? exception = null)
        {
            TryLog(log, Level.Error, () => message, exception);
        }

        // Lazy serious error.
        public static void Error(Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(s_DefaultLog, Level.Error, messageFactory, exception);
        }

        // Lazy serious error with explicit logger.
        public static void Error(ILog? log, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(log, Level.Error, messageFactory, exception);
        }

        // Simple debug log.
        public static void Debug(string message)
        {
            TryLog(s_DefaultLog, Level.Debug, () => message);
        }

        // Simple debug log with explicit logger.
        public static void Debug(ILog? log, string message)
        {
            TryLog(log, Level.Debug, () => message);
        }

        // Lazy debug log.
        public static void Debug(Func<string> messageFactory)
        {
            TryLog(s_DefaultLog, Level.Debug, messageFactory);
        }

        // Lazy debug log with explicit logger.
        public static void Debug(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Debug, messageFactory);
        }

        // Simple trace log.
        public static void Trace(string message)
        {
            TryLog(s_DefaultLog, Level.Trace, () => message);
        }

        // Simple trace log with explicit logger.
        public static void Trace(ILog? log, string message)
        {
            TryLog(log, Level.Trace, () => message);
        }

        // Lazy trace log.
        public static void Trace(Func<string> messageFactory)
        {
            TryLog(s_DefaultLog, Level.Trace, messageFactory);
        }

        // Lazy trace log with explicit logger.
        public static void Trace(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Trace, messageFactory);
        }

        // Logs once per default logger+key; later calls with the same key are ignored (reduce spam).
        public static bool WarnOnce(string key, Func<string> messageFactory, Exception? exception = null)
        {
            return WarnOnce(s_DefaultLog, key, messageFactory, exception);
        }

        // Logs once per logger+key; later calls with the same key are ignored (reduce spam).
        public static bool WarnOnce(ILog? log, string key, Func<string> messageFactory, Exception? exception = null)
        {
            if (string.IsNullOrEmpty(key) || messageFactory == null)
            {
                return false;
            }

            if (!IsLevelEnabled(log, Level.Warn))
            {
                return false;
            }

            string logName = GetLogName(log);
            string fullKey = string.IsNullOrEmpty(logName) ? key : logName + "|" + key;

            lock (s_WarnOnceLock)
            {
                if (s_WarnOnceKeys.Count >= kMaxWarnOnceKeys)
                {
                    s_WarnOnceKeys.Clear();
                }

                if (!s_WarnOnceKeys.Add(fullKey))
                {
                    return false;
                }
            }

            TryLog(log, Level.Warn, messageFactory, exception);
            return true;
        }

        // Central safe entrypoint using the remembered logger.
        public static void TryLog(Level level, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(s_DefaultLog, level, messageFactory, exception);
        }

        // Central safe entrypoint: checks level first, builds message safely, then direct-appends.
        public static void TryLog(ILog? log, Level level, Func<string> messageFactory, Exception? exception = null)
        {
            if (messageFactory == null)
            {
                return;
            }

            if (!IsLevelEnabled(log, level))
            {
                return;
            }

            string message;
            try
            {
                message = messageFactory() ?? string.Empty;
            }
            catch (Exception ex)
            {
                SafeLogNoException(log, Level.Warn, "Log message factory threw: " + ex.GetType().Name + ": " + ex.Message);
                return;
            }

            try
            {
                AppendDirect(log, level, message, exception);
            }
            catch
            {
            }
        }

        // Last-chance warning path used when the original message factory itself throws.
        private static void SafeLogNoException(ILog? log, Level level, string message)
        {
            try
            {
                if (IsLevelEnabled(log, level))
                {
                    AppendDirect(log, level, message, null);
                }
            }
            catch
            {
            }
        }

        // Writes directly to the mod log using .NET, bypassing Colossal's logger write path.
        private static void AppendDirect(ILog? log, Level level, string message, Exception? exception)
        {
            string logPath = GetLogPath(log);
            if (string.IsNullOrEmpty(logPath))
            {
                return;
            }

            lock (s_FileWriteLock)
            {
                // Direct append keeps routine mod diagnostics out of Colossal's UI-log path.
                // ShareReadWrite keeps the file readable while the game is running.
                string? dir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using FileStream stream = new( logPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite);
                using StreamWriter writer = new(stream);
                writer.Write('[');

                writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture));
                writer.Write("] [");
                writer.Write(level?.name ?? "INFO");    // Colossal's Level is a class and already supplies its name.
                writer.Write("]  ");
                writer.WriteLine(message ?? string.Empty);
                if (exception != null)
                {
                    writer.WriteLine(exception);
                }
            }
        }

        // Prefer Colossal's assigned file path; fallback to Logs/FallbackName.log if needed.
        private static string GetLogPath(ILog? log)
        {
            try
            {
                if (log != null && !string.IsNullOrEmpty(log.logPath))
                {
                    return log.logPath;
                }

                string logName = GetLogName(log);
                if (!string.IsNullOrEmpty(logName))
                {
                    return Path.Combine(LogManager.kDefaultLogPath, logName + ".log");
                }

                return string.Empty;
            }
            catch
            {
                if (string.IsNullOrEmpty(s_FallbackLogName))
                {
                    return string.Empty;
                }

                return Path.Combine(LogManager.kDefaultLogPath, s_FallbackLogName + ".log");
            }
        }

        // Keeps the logger name lookup isolated because ILog metadata can be fragile during startup.
        private static string GetLogName(ILog? log)
        {
            try
            {
                if (log != null && !string.IsNullOrEmpty(log.name))
                {
                    return log.name;
                }

                return s_FallbackLogName;
            }
            catch
            {
                return s_FallbackLogName;
            }
        }

        // If level checks fail because logging is in flux, keep direct-file logging available.
        private static bool IsLevelEnabled(ILog? log, Level level)
        {
            try
            {
                return log == null || log.isLevelEnabled(level);
            }
            catch
            {
                // If Colossal logging state is in flux, prefer keeping direct-file logging alive.
                return true;
            }
        }
    }
}
