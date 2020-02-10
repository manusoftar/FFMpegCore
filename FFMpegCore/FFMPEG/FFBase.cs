﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RunProcessAsTask;

namespace FFMpegCore.FFMPEG
{
    public abstract class FFBase : IDisposable
    {
        protected Process Process;

        protected FFBase()
        {
        }

        /// <summary>
        ///     Is 'true' when an exception is thrown during process kill (for paranoia level users).
        /// </summary>
        public bool IsKillFaulty { get; private set; }

        /// <summary>
        ///     Returns true if the associated process is still alive/running.
        /// </summary>
        public bool IsWorking
        {
            get
            {
                bool processHasExited;

                try
                {
                    processHasExited = Process.HasExited;
                }
                catch
                {
                    processHasExited = true;
                }

                return !processHasExited && Process.GetProcesses().Any(x => x.Id == Process.Id);
            }
        }

        public void Dispose()
        {
            Process?.Dispose();
        }

        protected void CreateProcess(string args, string processPath, bool rStandardInput = false,
            bool rStandardOutput = false, bool rStandardError = false)
        {
            if (IsWorking)
                throw new InvalidOperationException(
                    "The current FFMpeg process is busy with another operation. Create a new object for parallel executions.");

            Console.WriteLine("Se va a llamar al proceso con los siguientes parámetros -> {0}", args);
            Process = new Process();
            IsKillFaulty = false;
            Process.StartInfo.FileName = processPath;
            Process.StartInfo.Arguments = args;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.CreateNoWindow = true;

            Process.StartInfo.RedirectStandardInput = rStandardInput;
            Process.StartInfo.RedirectStandardOutput = rStandardOutput;
            Process.StartInfo.RedirectStandardError = rStandardError;
        }

        public void Kill()
        {
            try
            {
                if (IsWorking)
                    Process.Kill();
            }
            catch
            {
                IsKillFaulty = true;
            }
        }
        protected async Task<string> RunProcessAsync(string filePath, string arguments)
        {
            var result = await ProcessEx.RunAsync(filePath, arguments);
            return string.Join("", result.StandardOutput);
        }
    }
}