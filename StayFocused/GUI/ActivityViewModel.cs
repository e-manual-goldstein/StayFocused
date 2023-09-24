using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StayFocused;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace StayFocused
{

    public class ActivityViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ActivitySummary> activities = new ObservableCollection<ActivitySummary>();
        private List<ActivitySummary> _allActivitySummaries = new();
        public ObservableCollection<ActivitySummary> Activities
        {
            get { return activities; }
            set
            {
                activities = value;
                OnPropertyChanged();
            }
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Dictionary<string, Func<ActivitySummary, bool>> _filters = new();

        public void UpdateFilter(string columnName, Func<ActivitySummary, bool> filter)
        {
            _filters[columnName] = filter;
            ApplyFilters();
        }

        public void ApplyFilters()
        {
            var visibleActivities = Activities.Where(a => !IsFiltered(a));
            ReloadActivities(visibleActivities);
        }

        private void ReloadActivities(IEnumerable<ActivitySummary> activities)
        {
            Activities.Clear();
            foreach (var a in activities)
            {
                Activities.Add(a);
            }
        }

        private bool IsFiltered(ActivitySummary summary)
        {
            return _filters.Values.Any(filter => !filter(summary));
        }

        internal void AddNewActivitySummary(string processName, string windowTitle, int recordCount)
        {
            var summary = new ActivitySummary()
            {
                ProcessName = processName,
                WindowTitle = windowTitle,
                TotalDuration = TimeSpan.FromMilliseconds(recordCount * Constants.MonitoringIntervalMilliseconds)
            };
            _allActivitySummaries.Add(summary);
            Activities.Add(summary);
        }

        internal void UpdateSorting(GridViewColumn column, bool sortDescending)
        {
            var sortColumnName = column.Header is GridViewColumnHeader gridViewColumnHeader ? gridViewColumnHeader.Content : column.Header;
            Func<ActivitySummary, object> sortFunc = sortColumnName switch
            {
                "Process Name" => (a) => a.ProcessName,
                "Window Title" => (a) => a.WindowTitle,
                "Total Duration" => (a) => a.TotalDuration,
                _ => (a) => true
            };
            var newOrder = sortDescending ? Activities.OrderByDescending(sortFunc) : Activities.OrderBy(sortFunc);
            ReloadActivities(newOrder.ToArray());
        }

        internal void SelectAllVisible()
        {
            foreach (var activity in Activities)
            {
                activity.IsSelected = true;
            };
        }

        internal void UnselectAllVisible()
        {
            foreach (var activity in Activities)
            {
                activity.IsSelected = false;
            };
        }
    }

}