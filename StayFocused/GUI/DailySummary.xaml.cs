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
        private ContextMenu processNameMenu;
        private ContextMenu timespanColumnContextMenu;
        private Grid grid;
        private GridViewColumn[] gridViewColumns;

        public DailySummary()
        {
            InitializeComponent();
            SetUpViewComponents();
            //SetupColumnContextMenus();
            DataContext = _activityViewModel = new ActivityViewModel();
        }

        private void SetUpViewComponents()
        {
            grid = Content as Grid;
            foreach (var child in grid.Children)
            {
                if (child is ListView listView)
                {
                    if (listView.View is GridView gridView)
                    {
                        foreach (var column in gridView.Columns)
                        {
                            SetContextMenuForColumn(column);
                        }
                    }
                }
            }
        }

        private void SetContextMenuForColumn(GridViewColumn column)
        {
            switch (column.Header)
            {
                case "Process Name":
                    AddOrderByMenuItem(column);
                    AddFilterByMenuItem(column);
                    break;
                case "Window Title":
                    AddOrderByMenuItem(column);
                    AddFilterByMenuItem(column);
                    break;
                case "Total Duration":
                    AddOrderByMenuItem(column);
                    break;
                case "Is Selected":
                    AddSelectAllMenuItem(column);
                    AddUnselectAllMenuItem(column);
                    break;
                default:
                    break;
            }
        }

        private void AddSelectAllMenuItem(GridViewColumn column)
        {
            throw new NotImplementedException();
        }

        private void AddUnselectAllMenuItem(GridViewColumn column)
        {
            throw new NotImplementedException();
        }

        private void AddOrderByMenuItem(GridViewColumn column)
        {
            if (!(column.Header is GridViewColumnHeader columnHeader))
            {
                column.Header = new GridViewColumnHeader() { Content = column.Header };
            }
            var contextMenu = (column.Header as GridViewColumnHeader).ContextMenu ??= new ContextMenu();
            var filterByMenuItem = new MenuItem { Header = "Filter By" };
            filterByMenuItem.Click += FilterByMenuItem_Click;
            contextMenu.Items.Add(filterByMenuItem);
        }

        private void AddFilterByMenuItem(GridViewColumn column)
        {
            if (!(column.Header is GridViewColumnHeader columnHeader))
            {
                column.Header = new GridViewColumnHeader() { Content = column.Header };
            }
            var contextMenu = (column.Header as GridViewColumnHeader).ContextMenu ??= new ContextMenu();
            var orderByMenuItem = new MenuItem { Header = "Order By" };
            orderByMenuItem.Click += OrderByMenuItem_Click;
            contextMenu.Items.Add(orderByMenuItem);
        }

        private void SetupColumnContextMenus()
        {
            processNameMenu = new ContextMenu();
            timespanColumnContextMenu = new ContextMenu();

            var filterByMenuItem = new MenuItem { Header = "Filter By" };
            filterByMenuItem.Click += FilterByMenuItem_Click;

            var orderByMenuItem = new MenuItem { Header = "Order By" };
            orderByMenuItem.Click += OrderByMenuItem_Click;

            processNameMenu.Items.Add(filterByMenuItem);
            processNameMenu.Items.Add(orderByMenuItem);

            //timespanColumnContextMenu.Items.Add(orderByMenuItem);
            

            //// Assign the context menus to their respective columns
            //NameColumnHeader.ContextMenu = nameColumnContextMenu;
            //TimespanColumnHeader.ContextMenu = timespanColumnContextMenu;
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
        private void OrderByMenuItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FilterByMenuItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
