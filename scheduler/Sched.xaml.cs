﻿using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using OutlookCalendar.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace scheduler
{
    /// <summary>
    /// Interaction logic for Sched.xaml
    /// </summary>
    public partial class Sched : UserControl
    {
        List<DataAccessLayer.Models.Appointment> allAppts;
        CalendarAccess dbAccess;
        bool superuser = false;
     //   ObservableCollection<DataAccessLayer.Models.Appointment> events;
        public Sched()
        {
            InitializeComponent();
            Loaded += Sched_Loaded;
        }

        private void Sched_Loaded(object sender, RoutedEventArgs e)
        {
            // Register the Bubble Event Handler when calendar closes
            AddHandler(OutlookCalendar.Controls.Calendar.CalCosing,
                new RoutedEventHandler(CalendarClosed));
            dbAccess = new CalendarAccess();
            string error;
            //get all appointments
            allAppts = dbAccess.GetAppointments(out error);
            
            //filter those for today
            List<DataAccessLayer.Models.Appointment> today = allAppts.Where(a => a.StartTime.ToShortDateString() == DateTime.Now.ToShortDateString() ).ToList();
            lvDataBinding.ItemsSource = today;


            //populate the right pane with button for the next 6 days and highlite
            //any that have appointments
            populateRightPane();

        }

        void populateRightPane()
        {
            DateTime now = DateTime.Now;
            future.Children.Clear();
            //populate the right pane with button for the next 6 days and highlite
            //any that have appointments
            for (int i = 1; i < 7; i++)
            {
                DateTime t1 = now.AddDays(i);
                mybutton dayevent = new mybutton();
                dayevent.Date = t1;
                dayevent.Content = t1.DayOfWeek.ToString() + " " + t1.Day.ToString();
                dayevent.Click += dayevent_Click;

                List<DataAccessLayer.Models.Appointment> thisday = allAppts.Where(a => a.StartTime.ToShortDateString() == t1.ToShortDateString()).ToList();

                if (thisday.Count() > 0)
                    dayevent.Foreground = new SolidColorBrush(Colors.Red);
                future.Children.Add(dayevent);
            }
            //if (superuser)
            {
                mybutton dayevent = new mybutton();
               
                dayevent.Content = "Date Picker";
                dayevent.Click += ShowCalendar_Click; ;
                future.Children.Add(dayevent);
            }

        }

        private void ShowCalendar_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Calendar MonthlyCalendar = new System.Windows.Controls.Calendar();
          //  MonthlyCalendar.CalendarStyle = Microsoft.Windows.Controls.CalendarMode.Month;

         //   MonthlyCalendar.SelectionMode = CalendarSelectionMode.SingleRange;

            future.Children.Add(MonthlyCalendar);
        }

        private void CalendarClosed(object sender,
              RoutedEventArgs e)
        {
            populateRightPane();
            future.Visibility = Visibility.Visible;
        }
        //the user has clicked on a day in the right grid. open a day scheuduler
        //for the selected day and show appointments if there are any
        private void dayevent_Click(object sender, RoutedEventArgs e)
        {
           
            DateTime thisDate = (DateTime)(sender as mybutton).Date;
            cal.Appointments = allAppts;   //set control appointments to all as it will filter
            //currentdate is a dependency property on the calendar that will cause filtering by CurrentDate
            cal.CurrentDate = thisDate;
            cal.Visibility = Visibility.Visible;
            future.Visibility = Visibility.Collapsed;
    
        }

        private void Calendar_AddAppointment(object sender, RoutedEventArgs e)
        {
            if (!superuser)
                return;
            DataAccessLayer.Models.Appointment appointment = new DataAccessLayer.Models.Appointment();
            appointment.Subject = "Subject?";
            double Hour = (e.OriginalSource as CalendarTimeslotItem).Hour;
            appointment.StartTime = new DateTime(cal.CurrentDate.Year, cal.CurrentDate.Month, cal.CurrentDate.Day, (int)Hour, 0, 0);
            appointment.EndTime = new DateTime(cal.CurrentDate.Year, cal.CurrentDate.Month, cal.CurrentDate.Day, (int)Hour+1, 0, 0); ;

            AddAppointmentWindow aaw = new AddAppointmentWindow();
            aaw.DataContext = appointment;
            aaw.ShowDialog();

            allAppts.Add(appointment);
            dbAccess.SaveAppts(appointment);
            cal.Appointments = Filters.ByDate(allAppts, cal.CurrentDate).ToList();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Dialog about1 = new Dialog();
            about1.ShowDialog();
            superuser = false;
            if (about1.worked == true)
            {
                superuser = true;
                login.Source =  new BitmapImage(new Uri("Superman_shield.svg.png", UriKind.Relative));
                populateRightPane();
            }
        }
    }
    class mybutton:Button
    {
        public mybutton()
        {
            this.SetResourceReference(StyleProperty, typeof(Button));
        }
        public DateTime Date { get; set; }
    }
}