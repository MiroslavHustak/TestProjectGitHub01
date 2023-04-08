using System;
using System.Text;
using System.Threading;

//Nezapomen to pridat jako class library, ne jako console app

namespace ProgressBarCSharp
{
    //Copyright 2017 Daniel Wolf
    //https://gist.github.com/DanielSWolf

    //Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
    //to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
    //and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    //The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 

    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 2;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(0.05);
        private const string animation = @"|/-\-";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);

            // A progress bar is only for a temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object? state)
        {
            lock (timer)
            {
                if (disposed) return;
                byte[] bytes = Encoding.GetEncoding(437).GetBytes("█");//437 je tzv. Extended ASCII
                char[] output = Encoding.GetEncoding(852).GetChars(bytes);
                int progressBlockCount = (int)(currentProgress * blockCount);
                //int percent = ((int)((currentProgress * 100) - 1) == -1)? 0: (int)((currentProgress * 100) - 1);
                int percent = (int)(currentProgress * 100);

                //string aux = (percent == 100) ? "téměř" : String.Empty;
                /*   
                 string text1 = string.Format("<< {0}{1} >> {2}{3,4}% {4}",
                     new string(output[0], progressBlockCount), new string('-', blockCount - progressBlockCount), aux,
                     percent,
                     animation[animationIndex++ % animation.Length]);
                */
                string text = string.Format("{0}{1} {2} {4}",
                   new string(output[0], progressBlockCount), new string('*', blockCount - progressBlockCount), "  Vydrz chvilu, robim na tom usilovne....",
                   percent,
                   animation[animationIndex++ % animation.Length]);
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }
    }
}