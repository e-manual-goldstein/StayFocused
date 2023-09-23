using StayFocused.Activities;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace StayFocused.GUI
{
    /// <summary>
    /// Interaction logic for DailySummary.xaml
    /// </summary>
    public partial class DailySummary : Window
    {
        public DailySummary()
        {
            InitializeComponent();

            InitializeComponent();

            DataContext = _activityViewModel = new ActivityViewModel();

            //// Add sample activities
            //var viewModel = DataContext as ActivityViewModel;


        }

        private ActivityViewModel _activityViewModel;
        private ICollection<ActivityRecord> _activityRecords;

        internal void AddRecords(SFDbContext sfDbContext)
        {
            _activityRecords = sfDbContext.ActivityRecords.Where(d => d.TimeStamp.Date == DateTime.Today.Date).ToList();
            foreach (var summary in _activityRecords.GroupBy(a => new { a.ProcessName, a.WindowTitle }))                
            {
                _activityViewModel.Activities.Add(new ActivitySummary() 
                { 
                    ProcessName = summary.Key.ProcessName, 
                    WindowTitle = summary.Key.WindowTitle,
                    TotalDuration = TimeSpan.FromMilliseconds(summary.Count() * Constants.MonitoringIntervalMilliseconds)
                });
            }           
        }        
    }
}
