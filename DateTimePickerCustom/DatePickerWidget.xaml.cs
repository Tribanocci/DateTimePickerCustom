using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;



namespace DateTimePickerCustom
{
    /// <summary>
    /// Interaction logic for DatePickerWidget.xaml
    /// </summary>
    public partial class DatePickerWidget : UserControl, INotifyPropertyChanged
    {
        //==================================    PROPERTIES  ===================================================================================================
        private DateTime _selectedMonthAndYear = DateTime.Now;  // Property to hold the selected month and year (second row)
        public DateTime SelectedMonthAndYear
        {
            get { return _selectedMonthAndYear; }
            set
            {
                if (_selectedMonthAndYear != value)
                {
                    _selectedMonthAndYear = value;
                    OnPropertyChanged(nameof(SelectedMonthAndYear));// INotifyPropertyChanged, the string of the selectedDate (from changing month buttons)
                    InitializeCalendarData(); // MAke new list of buttons for the month days
                }
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        private DateTime _activeDate = DateTime.Now; //Property to hold the Active date (shown at the top row)
        public DateTime ActiveDate
        {
            get { return _activeDate; }
            set
            {
                if (_activeDate != value)
                {
                    _activeDate = value;
                    OnPropertyChanged(nameof(ActiveDate));// INotifyPropertyChanged, the string of the ActiveDAte (from pressing a calander button)
                }
            }
        }
        private Button _previousDateClickedButton; //Store the previous date that was clicked, so the button can reset its style
        public ObservableCollection<int> CalanderDataList { get; set; } = new ObservableCollection<int>(); // Property to hold the calendar days of the month,the observableCollection will notify when modified

        //==========================================    METHODS     ===========================================================================================================  
        public DatePickerWidget()   //Constructor
        {
            InitializeComponent();
            InitializeCalendarData(); //MAke the list of the day numbers (dates) of the selected month
            GenerateButtons(); //Assign the day numbers (dates) to button elements [Calander data]
            MainGrid.DataContext = this;
            PreviousButton.Click += OnPreviousButtonClick; //Event handler triggered when the prevbutton (to change the text of the selected month)
            NextButton.Click += OnNextButtonClick; //Event handler triggered when the nextbutton (to change the text of the selected month)
        }

        private void InitializeCalendarData()  //MAke the list of the dates of the month, at initialization and every time the selected month changes
        {
            CalanderDataList.Clear();
            DateTime firstDayOfMonth = new DateTime(SelectedMonthAndYear.Year, SelectedMonthAndYear.Month, 1); // Get the first day of the selected month
            DayOfWeek firstDayOfWeek = firstDayOfMonth.DayOfWeek; // Find the day of the week for the first day
            int daysInMonth = DateTime.DaysInMonth(SelectedMonthAndYear.Year, SelectedMonthAndYear.Month); // Get the number of days in the selected month
            for (int i = 1; i <= daysInMonth; i++) // Populate CalanderDataList with day numbers
            {
                CalanderDataList.Add(i);
            }
            CalanderDataList.Insert(0, 0); // The special value "zero" will help to change from the previous month dates to the current month
            DateTime prevMonth = new DateTime(SelectedMonthAndYear.Year, SelectedMonthAndYear.AddMonths(-1).Month, DateTime.DaysInMonth(SelectedMonthAndYear.AddMonths(-1).Year, SelectedMonthAndYear.AddMonths(-1).Month)); //Last day number of the prev month
            for (int i = 0; i < (int)firstDayOfWeek; i++) // Add placeholders for days before the first day of the week
            {
                CalanderDataList.Insert(0, prevMonth.Day - i);
            }
        }

        private void GenerateButtons() //Generate dynamically the buttons for the CalanderData (can manipalate style easier)
        {
            CalanderData.Children.Clear(); //Cleal previous buttons (from the previously selected month)
            CalanderData.RowDefinitions.Clear(); //Clear the rows that are dynamically created from this method.  [the columns are predefined in the .xaml (grid x:Name="CalanderData")]
            int columns = 7; // Set the number of columns (7 days), [the columns are predefined in the .xaml (grid x:Name="CalanderData")]
            int currentColumn = 0;
            int currentRow = 0;

            CalanderData.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Create first row

            int length = CalanderDataList.Count;
            int day = 0;
            bool isPrevMonth = true; // for distinguish the dates of the precious month , THE SPECIAL VALUE "ZERO" will help to identify the change
            while (day < length)  //iterate through the CalanderDataList
            {
                if (CalanderDataList[day] == 0) //check for month change
                {
                    isPrevMonth = false;
                    day++;
                }

                if (isPrevMonth == true) // the previous month dates will have different style
                {
                    Button CalanderDataButton_prev = new Button
                    {
                        Content = CalanderDataList[day],
                    };
                    CalanderDataButton_prev.Style = (Style)Resources["CalanderButtons_prev"];
                    day++;
                    Grid.SetRow(CalanderDataButton_prev, currentRow); // Set the row index 
                    Grid.SetColumn(CalanderDataButton_prev, currentColumn); // Set the column index                                 
                    CalanderData.Children.Add(CalanderDataButton_prev); // Add the Button to the Grid
                    currentColumn++;
                }

                else if (isPrevMonth == false) // the current month will have different style buttons
                {
                    Button CalanderDataButton = new Button // Create a Button for each item
                    {
                        Content = CalanderDataList[day],
                    };
                    CalanderDataButton.Style = (Style)Resources["CalanderButtons"];
                    if (CalanderDataList[day] == ActiveDate.Day && ActiveDate.Month == SelectedMonthAndYear.Month) // The active Date (the one at the top) will have different style
                    {
                        CalanderDataButton.Style = (Style)Resources["CalanderButtons_active"];
                        _previousDateClickedButton = CalanderDataButton;
                    }
                    CalanderDataButton.Click += Date_Click; // Attach the click event handler for each date of the current month
                    day++;
                    Grid.SetRow(CalanderDataButton, currentRow); // Set the row index (zero-based)
                    Grid.SetColumn(CalanderDataButton, currentColumn); // Set the column index (zero-based)                         
                    CalanderData.Children.Add(CalanderDataButton); // Attach the click event handler
                    currentColumn++;

                    if (currentColumn >= columns) // Check if we need to move to the next row
                    {
                        currentColumn = 0;
                        currentRow++;
                        CalanderData.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    }
                }
            }
        }
        // ===========================      Event Handling Methods    =======================================================================
        public event PropertyChangedEventHandler PropertyChanged;// INotifyPropertyChanged implementation
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void OnPreviousButtonClick(object sender, RoutedEventArgs e) // selected month -> prev
        {
            SelectedMonthAndYear = SelectedMonthAndYear.AddMonths(-1); //Will trigger the Setter of the SelectedMonthAndYear, which will cal the InitializeCalendarData method, making the new list of dates
            GenerateButtons(); // The new buttons wiil be created
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)// selected month -> next
        {
            SelectedMonthAndYear = SelectedMonthAndYear.AddMonths(1);
            GenerateButtons();
        }

        private void Date_Click(object sender, RoutedEventArgs e) //Clicking a button of the dates in a month will change the active date, making style changes also (the previousDatebutton is used here)
        {
            if (_previousDateClickedButton != null)// Reset the background color of the previous clicked button
            {
                _previousDateClickedButton.Style = (Style)Resources["CalanderButtons"];
            }

            Button clickedButton = (Button)sender; //style changes
            clickedButton.Style = (Style)Resources["CalanderButtons_active"];
            _previousDateClickedButton = clickedButton;
            int newDay = (int)clickedButton.Content; //store the new date

            ActiveDate = ActiveDate.AddDays(newDay - ActiveDate.Day); //change the active date,  the setter will be triggered
            ActiveDate = ActiveDate.AddMonths(SelectedMonthAndYear.Month - ActiveDate.Month);
            ActiveDate = ActiveDate.AddYears(SelectedMonthAndYear.Year - ActiveDate.Year);
        }

    }
}




