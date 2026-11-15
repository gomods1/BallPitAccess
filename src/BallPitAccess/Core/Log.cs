using System;
using System.IO;
using MelonLoader;

namespace BallPitAccess.Core
{
    /// <summary>MelonLoader log plus the mod's own files in UserData/BallPitAccess.</summary>
    internal static class Log
    {
        internal static string Dir { get; private set; }
        private static StreamWriter _speech;
        private static StreamWriter _events;
        private static StreamWriter _cues;
        private static readonly object Gate = new object();

        internal static void Init()
        {
            Dir = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "BallPitAccess");
            Directory.CreateDirectory(Dir);
            _speech = Open("speech.log");
            _events = Open("events.log");
            _cues = Open("cues.log");
        }

        private static StreamWriter Open(string name)
        {
            try { return new StreamWriter(Path.Combine(Dir, name), false) { AutoFlush = true }; }
            catch { return null; }
        }

        internal static void Info(string msg) => MelonLogger.Msg(msg);
        internal static void Warn(string msg) => MelonLogger.Warning(msg);
        internal static void Error(string msg) => MelonLogger.Error(msg);

        internal static void Debug(string msg)
        {
            if (Config.Debug) MelonLogger.Msg("[debug] " + msg);
        }

        internal static void Speech(string text, bool interrupt)
        {
            lock (Gate) _speech?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {(interrupt ? "!" : " ")} {text}");
        }

        internal static void Event(string text)
        {
            lock (Gate) _events?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {text}");
        }

        internal static void Cue(string text)
        {
            lock (Gate) _cues?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {text}");
        }

        internal static void WriteFile(string name, string content)
        {
            try { File.WriteAllText(Path.Combine(Dir, name), content); }
            catch (Exception ex) { Warn("Could not write " + name + ": " + ex.Message); }
        }
    }
}
