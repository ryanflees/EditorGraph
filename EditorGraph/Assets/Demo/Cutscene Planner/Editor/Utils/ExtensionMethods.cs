using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CutscenePlanner.Editor.Utils
{
    /// <summary> Collection of the extension methods. </summary>
    public static class ExtensionMethods
    {
        /// <summary> Fast array fill with given values.</summary>
        /// <typeparam name="T">Type of the array.</typeparam>
        /// <param name="destinationArray">Array that should be filled</param>
        /// <param name="value">Value which will be repeatedly used to fill array (or clamped if there is more values that array space)</param>
        public static void Fill<T>(this T[] destinationArray, params T[] value)
        {
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            int finalValueLength = value.Length;
            if (value.Length >= destinationArray.Length)
                finalValueLength = destinationArray.Length;

            // set the initial array value
            Array.Copy(value, destinationArray, finalValueLength);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = finalValueLength; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);
        }
        /// <summary> Rounds the timespan to miliseconds. </summary>
        /// <param name="t">Time span that should be rounded.</param>
        /// <returns>Timespan rounded to miliseconds.</returns>
        public static TimeSpan RoundToMiliseconds(this TimeSpan t)
        {
            int precision = 3;
            const int TIMESPAN_SIZE = 7;
            int factor = (int)Math.Pow(10, (TIMESPAN_SIZE - precision));

            return new TimeSpan(((long)Math.Round((1.0 * t.Ticks / factor)) * factor));
        }
        /// <summary> Make TimeSpan object based on given time in seconds.</summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>TimeSpan.</returns>
        public static TimeSpan MakeTimeSpan(double time)
        {
            int milis = (int)Math.Ceiling(time * 1000.0);
            return new TimeSpan(0, 0, 0, 0, milis);
        }
        /// <summary> Converts timespan to string in format "dd.hh:mm:ss.fff"</summary>
        /// <remarks> Operation is expensive!</remarks>
        /// <param name="time">Time to convert.</param>
        /// <returns>Converted string.</returns>
        public static string ToStringSpecial(this TimeSpan time)
        {
            StringBuilder s = new StringBuilder();
            int days = time.Days;
            int hours = time.Hours;
            int min = time.Minutes;
            int sec = time.Seconds;
            int milisInt = time.Milliseconds;

            string milis = milisInt.ToString();
            char milis1 = '0', milis2 = '0', milis3 = '0';
            if (milisInt > 100)
            {
                milis1 = milis[0];
                milis2 = milis[1];
                milis3 = milis[2];
            }
            else if (milisInt < 100 && milisInt >= 10)
            {
                milis1 = '0';
                milis2 = milis[0];
                milis3 = milis[1];
            }
            else if (milisInt < 10)
            {
                milis1 = '0';
                milis2 = '0';
                milis3 = milis[0];
            }
            if (days > 0) { s.Append(days); s.Append("."); }
            if (hours < 10) s.Append("0"); s.Append(hours); s.Append(":");
            if (min < 10) s.Append("0"); s.Append(min); s.Append(":");
            if (sec< 10) s.Append("0");  s.Append(sec); s.Append(".");
            s.Append(milis1); s.Append(milis2); s.Append(milis3);

            return s.ToString();
        }
        /// <summary> Split given time (as string) to 4 element string array wher elements are: days+hours, minutes, seconds and miliseconds.  </summary>
        /// <param name="time">Time to split</param>
        /// <returns> 4 element string array wher elements are: days+hours, minutes, seconds and miliseconds.</returns>
        public static string[] SplitTime(string time)
        {
            List<string> partsList = time.Split(':').ToList();
            if (partsList.Count == 0)
            {
                partsList.Add("00");
                partsList.Add("00");
                partsList.Add("00");
            }
            if (partsList.Count == 1)
            {
                partsList.Add("00");
                partsList.Add("00");
            }
            if (partsList.Count == 2)
            {
                partsList.Add("00");
            }

            string[] sec_mils = partsList[2].Split('.');
            partsList[2] = sec_mils[0];
            if (sec_mils.Length > 1)
                partsList.Add(sec_mils[1].PadLeft(3, '0'));
            else
                partsList.Add("000");

            return partsList.ToArray();
        }
        /// <summary> Change color brightness.</summary>
        /// <param name="color">Color which brightness should be changed.</param>
        /// <param name="v">Brightness value form 0 to 1.</param>
        /// <returns>Color with changed brightness.</returns>
        public static Color ChangeColorBrightness(Color color, float v)
        {
            float H;
            float S;
            float V;

            Color.RGBToHSV(color, out H, out S, out V);

            V = v;

            return Color.HSVToRGB(H, S, V);
        }
    }
}