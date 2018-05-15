using System;
using System.Text;
using UnityEngine;
using CutscenePlanner.Editor.Resources;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CutscenePlanner.Editor.Utils;

namespace CutscenePlanner.Editor.Controls
{
    /// <summary> Represents the time text field control.</summary>
    /// <remarks>
    /// - As input, use TimeSpan
    /// - Complete validation for the TimeSpan
    /// - Can add or substract value of the selected time span's part by buttons
    /// - Can add or substract value of the selected time span's part by arrow keys
    /// - text height autoscale to rect height.
    /// </remarks>
    public class TimeTextField
    {
        /// <summary> Rect of the control. </summary>
        public Rect Rect { get { return _rect; } }

        private Rect _rect;
        private TimeSpan _time;
        private bool _keyFlag;
        private int _lastCursorIndex;
        private GUIStyle _textFieldStyle;
        private string _id;

        /// <summary> Create new TimeTextField control.</summary>
        public TimeTextField()
        {
            _rect = new Rect();
            _time = TimeSpan.Zero;
            _textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter
            };
            _id = "TimeTextField_" + Guid.NewGuid().ToString();
        }

        /// <summary> Draw control.</summary>
        /// <remarks> Have to be used in OnGUI() signal.</remarks>
        /// <param name="rect">Rect of the component.</param>
        /// <param name="time">Value of the component.</param>
        /// <param name="label">Label of the component.</param>
        /// <returns>New time, that has beed modified by user.</returns>
        public TimeSpan Draw(Rect rect, TimeSpan time, GUIContent label, ArrowsLayout arrowsLayout)
        {
            _rect = rect;
            _textFieldStyle.fontSize = (int)_rect.height - 6;
            float buttonWidth = 16;
            if (_rect.height / 2 > buttonWidth)
                buttonWidth = _rect.height / 2;
            Vector2 contentSize = GUI.skin.label.CalcSize(label);

            Rect labelRect = new Rect(new Vector2(5 + _rect.x, _rect.y + _rect.height / 2 - contentSize.y / 2), new Vector2(EditorGUIUtility.labelWidth - 1, contentSize.y - EditorGUIUtility.standardVerticalSpacing));
            Rect fieldRect = new Rect(labelRect.x + labelRect.width, _rect.y, _rect.width - labelRect.width - 10 - 2, _rect.height - EditorGUIUtility.standardVerticalSpacing); // for none
            Rect arrowUpRect = new Rect();
            Rect arrowDownRect = new Rect();

            if (arrowsLayout == ArrowsLayout.Vertical)
            {
                fieldRect = new Rect(labelRect.x + labelRect.width, _rect.y, _rect.width - labelRect.width - buttonWidth - 10 - 2, _rect.height - EditorGUIUtility.standardVerticalSpacing);
                arrowUpRect = new Rect(2 + fieldRect.x + fieldRect.width, _rect.y, buttonWidth, rect.height / 2 - EditorGUIUtility.standardVerticalSpacing / 2);
                arrowDownRect = new Rect(2 + fieldRect.x + fieldRect.width, _rect.y + rect.height / 2 - EditorGUIUtility.standardVerticalSpacing, buttonWidth, rect.height / 2 - EditorGUIUtility.standardVerticalSpacing / 2);
            }
            else if (arrowsLayout == ArrowsLayout.Horizontal)
            {
                arrowDownRect = new Rect(labelRect.x + labelRect.width , _rect.y, buttonWidth, rect.height - EditorGUIUtility.standardVerticalSpacing);                
                fieldRect = new Rect(2 + arrowDownRect.x + arrowDownRect.width, _rect.y, _rect.width - labelRect.width - buttonWidth*2 - 2*2 - 10, _rect.height - EditorGUIUtility.standardVerticalSpacing);
                arrowUpRect = new Rect(2 + fieldRect.x + fieldRect.width, _rect.y, buttonWidth, rect.height - EditorGUIUtility.standardVerticalSpacing);
            }
            

            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            int cursorIndex = textEditor.cursorIndex;
            
            int selectedPart = -1;
            if (textEditor.text != string.Empty)
            {
                
              /*  if ((Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown) &&
                    (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace))
                    Event.current.Use();*/

                string[] parts = ExtensionMethods.SplitTime(textEditor.text);
                int charsSum = 0;
                for (int i = 0; i < parts.Length; i++)
                {
                    if (cursorIndex >= charsSum && cursorIndex <= charsSum + parts[i].Length)
                    {
                        selectedPart = i;
                        break;
                    }
                    charsSum += parts[i].Length+1; // adding one cause split char (. or :)
                }
            }
            string name = GUI.GetNameOfFocusedControl();

            if (_id == name && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.DownArrow))
            {
                int addTimeStep = 0;
                if (Event.current.keyCode == KeyCode.UpArrow)
                    addTimeStep = 1;
                else if (Event.current.keyCode == KeyCode.DownArrow)
                    addTimeStep = -1;

                time = HandleAddTime(time, addTimeStep, selectedPart);
                _keyFlag = true;
                _lastCursorIndex = textEditor.cursorIndex;
            }
            if (_keyFlag && textEditor.cursorIndex != _lastCursorIndex)
            {              
                textEditor.cursorIndex = _lastCursorIndex;
                textEditor.selectIndex = _lastCursorIndex;
            }
            if (_keyFlag && Event.current.type == EventType.KeyUp)
                _keyFlag = false;

            GUI.Label(labelRect, label);
            if (GUI.Button(arrowUpRect, arrowsLayout == ArrowsLayout.Horizontal ? EditorResources.ArrowRight.GUIContent : EditorResources.ArrowUp.GUIContent))
                time = HandleAddTime(time, 1, selectedPart);
            if (GUI.Button(arrowDownRect, arrowsLayout == ArrowsLayout.Horizontal ? EditorResources.ArrowLeft.GUIContent : EditorResources.ArrowDown.GUIContent ))
                time = HandleAddTime(time, -1, selectedPart);

            GUI.SetNextControlName(_id);
            string timeStringRaw = time.ToStringSpecial();
            string timeString = GUI.TextField(fieldRect, timeStringRaw, _textFieldStyle);
            if (timeString != timeStringRaw)
                _time = Validate(time, timeString);            
            else
                _time = time;

            return _time;
        }
       
        /// <summary> Validate given string as TimeSpan. If complete failure, originalTime will be returned. </summary>
        /// <param name="originalTime">TimeSpan, that will be returned if complete failure.</param>
        /// <param name="timeString">String to validate.</param>
        /// <returns>TimeSpan created from given string or originalTime if complete failure.</returns>
        private static TimeSpan Validate(TimeSpan originalTime, string timeString)
        {
            TimeSpan _returnTime = originalTime;
            string oldTime = _returnTime.ToStringSpecial();

            bool valid = true;
            string[] timeParts = ExtensionMethods.SplitTime(timeString);
            string[] oldTimeParts = ExtensionMethods.SplitTime(oldTime);

            if (timeParts.Length != 3 && timeParts.Length != 4)
                valid = false;
            else
            {
                for (int i = 0; i < timeParts.Length; i++)
                {
                    int parse;
                    if (i >0 && i <=2 && timeParts[i].Length != 2 && timeParts[i].Length != 3)
                        valid = false;
                    else if (!timeParts[i].Contains(".") && !int.TryParse(timeParts[i], out parse))
                        valid = false;
                }
            }

            if (valid)
            {
                for (int i = 0; i < timeParts.Length; i++)
                {
                    StringBuilder sb = new StringBuilder(timeParts[i]);
                    string[] minSplit = null;
                    if (i == 0 && timeParts[i].Contains("."))
                    {
                        minSplit = timeParts[i].Split('.');
                        if (minSplit.Length > 1)
                        {
                            sb = new StringBuilder(minSplit[1]);
                            string[] oldMinSplit = oldTimeParts[i].Split('.');
                            if (oldMinSplit.Length>1)
                                oldTimeParts[i] = oldMinSplit[1];
                        }

                    }
                    if (i <= 2 &&  sb.Length > 2)
                    {
                        if (sb[0] != oldTimeParts[i][0])
                            sb.Remove(1, 1);
                        else if (sb[1] != oldTimeParts[i][1])
                            sb.Remove(2, 1);
                        else
                            sb.Remove(2, 1);
                    }
                    else if (i == 3 && sb.Length > 3)
                    {
                        if (sb[0] != oldTimeParts[i][0])
                            sb.Remove(1, 1);
                        else if (sb[1] != oldTimeParts[i][1])
                            sb.Remove(2, 1);
                        else if (sb[2] != oldTimeParts[i][2])
                            sb.Remove(3, 1);
                        else
                            sb.Remove(3, 1);
                    }
                    timeParts[i] = sb.ToString();
                    if (minSplit!= null)
                        timeParts[i] = minSplit[0] + "." + sb.ToString();
                    string partToParse = timeParts[i];
                    string[] minParts = null;
                    if (i == 0 && partToParse.Contains("."))
                    {
                        minParts = partToParse.Split('.');
                        if (minParts.Length>1)
                            partToParse = minParts[1];
                    }
                    int parseResult;
                    if (int.TryParse(partToParse, out parseResult))
                    {
                        if (i == 0 && parseResult > 23)
                        {
                            if (minParts != null && minParts.Length > 1)
                                timeParts[i] = minParts[0] + ".";
                            timeParts[i] = "23";
                        }
                        if ((i == 1 || i == 2) && parseResult>59)
                            timeParts[i] = "59";
                    }
                }
                TimeSpan.TryParse(timeParts[0] + ":" + timeParts[1] + ":" + timeParts[2] + "." + timeParts[3], out _returnTime);
            }

            return _returnTime;
        }
        /// <summary> Adds the time to given time span part (hours, minutes or seconds).</summary>
        /// <param name="time">Time, to which should be added time. </param>
        /// <param name="addValue">Value to add.</param>
        /// <param name="selectedPart">Part of the timespan. 0 is hours, 1 is minutes and 2 is seconds.</param>
        /// <returns></returns>
        private static TimeSpan HandleAddTime(TimeSpan time, int addValue, int selectedPart)
        {
            TimeSpan addTime = TimeSpan.Zero;
            if (selectedPart == 0)
                addTime = new TimeSpan(addValue, 0, 0);
            else if (selectedPart == 1)
                addTime = new TimeSpan(0, addValue, 0);
            else if (selectedPart == 2)
                addTime = new TimeSpan(0, 0, addValue);
            else if (selectedPart == 2)
                addTime = new TimeSpan(0, 0, addValue);
            else if (selectedPart == -1 || selectedPart == 3)
                addTime = new TimeSpan(0, 0, 0,0, addValue);

            time += addTime;

            return time;
        }
    }
    /// <summary> Arrows layout types enumeration.</summary>
    public enum ArrowsLayout
    {
        /// <summary> No arrows.</summary>
        None,
        /// <summary> Horizontal layout.</summary>
        Horizontal,
        /// <summary> Vertical layout.</summary>
        Vertical,
    }
}