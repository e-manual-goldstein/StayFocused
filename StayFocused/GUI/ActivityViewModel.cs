using StayFocused;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StayFocused
{

    public class ActivityViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ActivitySummary> activities = new ObservableCollection<ActivitySummary>();

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
    }

}