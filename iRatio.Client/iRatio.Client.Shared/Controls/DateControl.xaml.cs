using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UnoTest.Client.Controls
{
    public sealed partial class DateControl : UserControl
    {
        public static readonly DependencyProperty DateProperty = DependencyProperty.Register("Date", typeof(DateTime), typeof(DateControl), new PropertyMetadata(DateTime.MinValue, OnDateChanged));

        public DateTime Date {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public DateControl()
        {
            InitializeComponent();

            var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            YearBox.ItemsSource = Enumerable.Range(cal.GetYear(DateTime.Now) - 110, 96);
            MonthBox.ItemsSource = DateTimeFormatInfo.CurrentInfo.MonthNames.Where(n => !string.IsNullOrEmpty(n));

            YearBox.SelectionChanged += OnYearMonthDaySelectionChanged;
            MonthBox.SelectionChanged += OnYearMonthDaySelectionChanged;
            DayBox.SelectionChanged += OnYearMonthDaySelectionChanged;
        }

        private void OnYearMonthDaySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YearBox.SelectedItem != null && MonthBox.SelectedItem != null) {
                var cal = DateTimeFormatInfo.CurrentInfo.Calendar;

                if (sender != DayBox) {
                    int numDays = cal.GetDaysInMonth((int)YearBox.SelectedItem, MonthBox.SelectedIndex + 1);

                    if (DayBox.Items.Count != numDays) {
                        int index = DayBox.SelectedIndex;
                        DayBox.ItemsSource = Enumerable.Range(1, numDays);
                        DayBox.SelectedIndex = Math.Clamp(index, 0, numDays - 1);
                    }
                }

                if (DayBox.SelectedItem != null) {
                    try {
                        Date = new DateTime((int)YearBox.SelectedItem, MonthBox.SelectedIndex + 1, (int)DayBox.SelectedItem, DateTimeFormatInfo.CurrentInfo.Calendar);
                    }
                    catch {
                        Date = DateTime.MinValue;
                    }
                }
            }
        }

        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (DateTime)e.NewValue;

            if (value != (DateTime)e.OldValue && value != DateTime.MinValue) {
                var control = (DateControl)d;

                control.YearBox.SelectedItem = value.Year;
                control.MonthBox.SelectedIndex = value.Month - 1;
                control.DayBox.SelectedItem = value.Day;
            }
        }
    }
}
