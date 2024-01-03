using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace DateTimePickerCustom
{    /// <summary>
     /// Converter for string of the textbox (second row) selectedTime (string) to textblock (first row) activetime (TimeOnly)
     /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime time)
                return time;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DateTime.TryParse(value.ToString(), out var time))
                return time;
            return value;
        }
    }

    /// <summary>
    /// Interaction logic for TimePickerWidget.xaml
    /// </summary>
    public partial class TimePickerWidget : UserControl, INotifyPropertyChanged
    {
        //===================   Properties  ===============================================================================================
        private DateTime _activeTime = DateTime.Now;  // Property to hold the Active Time (first row)
        public DateTime ActiveTime
        {
            get { return _activeTime; }
            set
            {
                if (_activeTime != value)
                {
                    _activeTime = value;
                    OnPropertyChanged(nameof(ActiveTime));// INotifyPropertyChanged, the string of the selectedDate (from changing month buttons)
                    AngleSet();
                }
            }
        }

        private DateTime _selectedTime = DateTime.Now; // Property to hold the selected Time (second row)
        public DateTime SelectedTime
        {
            get { return _selectedTime; }
            set
            {
                if (_selectedTime != value)
                {
                    _selectedTime = value;
                    OnPropertyChanged(nameof(SelectedTime));// INotifyPropertyChanged, the string of the selectedDate (from changing month buttons)
                    ActiveTime = value;
                }
            }
        }

        private double _currentHourAngle;  // Property for calculating the angle of the hour hand
        public double CurrentHourAngle
        {
            get { return _currentHourAngle; }
            set
            {
                if (_currentHourAngle != value)
                {
                    _currentHourAngle = value;
                    OnPropertyChanged(nameof(CurrentHourAngle));// INotifyPropertyChanged, the string of the selectedDate (from changing month buttons)
                    AngleSet();
                }
            }
        }

        private double _currentMinutesAngle;  // Property for calculating the angle of the minute hand
        public double CurrentMinutesAngle
        {
            get { return _currentMinutesAngle; }
            set
            {
                if (_currentMinutesAngle != value)
                {
                    _currentMinutesAngle = value;
                    OnPropertyChanged(nameof(CurrentMinutesAngle));
                    AngleSet();
                }
            }
        }

        private double _currentSecondsAngle; // Property for calculating the angle of the second hand
        public double CurrentSecondsAngle
        {
            get { return _currentSecondsAngle; }
            set
            {
                if (_currentSecondsAngle != value)
                {
                    _currentSecondsAngle = value;
                    OnPropertyChanged(nameof(CurrentSecondsAngle));
                    AngleSet();
                }
            }
        }

        private int selectionStart = 0; //store the textbox selection text start index
        private int selectionLength = 2; // store the textbox selection length
        private int PrEvTime = 0; // variable for storing the timestamp of the input event to textbox. If two events are triggered in a short time interval, the event handler should have a different behaviour (two digit-input)
        private bool isSecondCall;// variable for storing whether a second event was triggered in a short time interval --> see NumberInputTextbox()
        private int[] MaxTimeVals = { 12, 59, 59 }; // Array for organizing the max values of the time integers
        public event PropertyChangedEventHandler PropertyChanged;// INotifyPropertyChanged implementation

        //========================  Methods  =====================================================
        public TimePickerWidget()
        {
            InitializeComponent();
            AngleSet();
            DataContext = this;
        }

        public void AngleSet()
        {
            CurrentHourAngle = _activeTime.Hour / 12.0 * 360.0 - 180.0;
            CurrentMinutesAngle = _activeTime.Minute / 60.0 * 360.0 - 180.0;
            CurrentSecondsAngle = _activeTime.Second / 60.0 * 360.0 - 180.0;
        }

        //======================    EVENT HANDLERS    ===============================================
        private void Window_KeyDown(object sender, KeyEventArgs e) // Controls for textbox input, like BackSpace, Arrows, Tab
        {
            if (e.Key == Key.Back || e.Key == Key.Delete) //disable Backspace and delete buttons for textbox
            {
                e.Handled = true;
            }

            else if (e.Key == Key.Right || e.Key == Key.Tab) // Right arrow and tab changes the selected part of the textbox
            {
                if (sender is TextBox textBox)
                {
                    if (textBox.SelectionStart < 9)
                    {
                        textBox.SelectionStart = textBox.SelectionStart + 3;
                        e.Handled = true;
                        selectionStart = textBox.SelectionStart;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }

            else if (e.Key == Key.Left) // Left arrow changes the selected part of the textbox
            {
                if (sender is TextBox textBox)
                {
                    if (textBox.SelectionStart > 0)
                    {
                        textBox.SelectionStart = textBox.SelectionStart - 3;
                        e.Handled = true;
                        selectionStart = textBox.SelectionStart;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) //Disable letters and symbols for textbox input
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            if (!e.Handled)
            {
                NumberInputTextbox(sender, e); //Specific handling of the input numbers.
            }
        }

        private void NumberInputTextbox(object sender, TextCompositionEventArgs e)
        {
            if (PrEvTime == 0)
            {
                PrEvTime = e.Timestamp;
                isSecondCall = false;
            }
            else if (PrEvTime != 0)
            {
                if (e.Timestamp - PrEvTime < 800 && !isSecondCall) //the theshold for determining two close triggered events. (800 ms -> 0.8 seconds).
                {
                    isSecondCall = true;
                }
                else
                {
                    isSecondCall = false;
                }
                PrEvTime = e.Timestamp; //Update previous timestamp of the event triggered
            } //Determine whether a number input was happened in a short time interval. Therefore, is should be treated as a two-digit input.

            if (sender is TextBox textBox) //Access the textBox
            {
                selectionStart = textBox.SelectionStart; //Update the selected part 
                selectionLength = textBox.SelectionLength;

                if (selectionStart > 6) //The number input is only for hrs, min, and sec, not for AM/PM
                {
                    e.Handled = true;
                    return;
                }

                if (!isSecondCall)// If the input is a single digit, add a leading zero
                {
                    e.Handled = true;
                    textBox.Text = string.Concat(textBox.Text.Substring(0, selectionStart), "0", e.Text, textBox.Text.Substring(selectionStart + selectionLength));
                    textBox.SelectionStart = selectionStart;
                    textBox.SelectionLength = selectionLength;
                }

                else if (isSecondCall)// If the input is a two-digit (meaning two events triggred closely)
                {
                    e.Handled = true;
                    int timeInt = int.Parse(string.Concat(textBox.Text.Substring(selectionStart, selectionLength), e.Text)); //convert and store the selected part of the textbox as integer
                    if (timeInt < MaxTimeVals[selectionStart / 3])
                    {// write down the input of the keyboard to the textbox text that was selected, leaving the rest text in place
                        textBox.Text = string.Concat(textBox.Text.Substring(0, selectionStart), timeInt.ToString(), textBox.Text.Substring(selectionStart + selectionLength));
                    }
                    else
                    {// If the input was out of bounds, the input becomes the max value that is appropriate
                        textBox.Text = string.Concat(textBox.Text.Substring(0, selectionStart), MaxTimeVals[selectionStart / 3].ToString(), textBox.Text.Substring(selectionStart + selectionLength));
                    }
                    textBox.SelectionStart = selectionStart;
                    textBox.SelectionLength = selectionLength;
                }
            }
        }
        private void TimeSelector_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Handling theclick event inside the textBox
        {                                                                                           //Selecting only parts that correspond to Hrs, Mins, ,Seconds or AM/PM
            if (sender is TextBox textBox)
            {
                e.Handled = true;
                textBox.Focus();
                Point mouseClick = e.GetPosition(textBox); // Calculate the click position relative to the text
                double totalWidth = textBox.ActualWidth;
                int clickIndex = (int)((mouseClick.X / totalWidth) * textBox.Text.Length);

                while (clickIndex % 3 != 0)//Selection start index should be only the index of starting hour, minute, second or AM/PM part. Not in between.
                {
                    clickIndex--;
                }
                textBox.SelectionStart = clickIndex;
                textBox.SelectionLength = 2; // Select two positions

                selectionStart = clickIndex; //Update select properties
                selectionLength = 2;
            }
        }
        private void decreaseBtn(object sender, RoutedEventArgs e) //Handling the decrease button of the textBox, besides decreasing, itshould not make value out of bounds. Different for AM/PM
        {                                                          //When clicked, the textBox should not lose focus an d keep the selected text part
            e.Handled = true;
            TimeSelector.Focus();
            Keyboard.Focus(TimeSelector);
            TimeSelector.Select(selectionStart, selectionLength);
            TimeSelector.UpdateLayout();
            string timestr = TimeSelector.Text.Substring(TimeSelector.SelectionStart, TimeSelector.SelectionLength);

            if (selectionStart < 8) //Only for Hrs, Mins, Seconds
            {
                int timeInt = Int32.Parse(timestr);
                timeInt--;
                if (timeInt < 10 && timeInt > 0)
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "0", timeInt.ToString(), TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                }
                else if (timeInt >= 10)
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), timeInt.ToString(), TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                }
                else
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), MaxTimeVals[selectionStart / 3].ToString(), TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                }
            }
            else //For AM/PM
            {
                if (timestr == "AM")
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "PM");
                }
                else
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "AM");
                }
            }
            TimeSelector.Select(selectionStart, selectionLength);
            TimeSelector.UpdateLayout();
        }
        private void increaseBtn(object sender, RoutedEventArgs e) //Same logic for increase button
        {
            e.Handled = true;
            TimeSelector.Focus();
            Keyboard.Focus(TimeSelector);
            TimeSelector.Select(selectionStart, selectionLength);
            TimeSelector.UpdateLayout();
            string timestr = TimeSelector.Text.Substring(TimeSelector.SelectionStart, TimeSelector.SelectionLength);
            if (selectionStart < 8)
            {
                int timeInt = Int32.Parse(timestr);
                timeInt++;
                if (timeInt < 10)
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "0", timeInt.ToString(), TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                }
                else if (timeInt >= 10 && timeInt <= MaxTimeVals[selectionStart / 3])
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), timeInt.ToString(), TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                }
                else
                {
                    if (selectionStart == 0)
                    {
                        TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "01", TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                    }
                    else
                    {
                        TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "00", TimeSelector.Text.Substring(TimeSelector.SelectionStart + TimeSelector.SelectionLength));
                    }

                }
            }
            else
            {
                if (timestr == "AM")
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "PM");
                }
                else
                {
                    TimeSelector.Text = string.Concat(TimeSelector.Text.Substring(0, TimeSelector.SelectionStart), "AM");
                }
            }
            TimeSelector.Select(selectionStart, selectionLength);
            TimeSelector.UpdateLayout();
        }
        protected void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // This is a hack to make the RichTextBox think it did not lose focus.
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}




