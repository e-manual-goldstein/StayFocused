using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace StayFocused.GUI
{
    /// <summary>
    /// Interaction logic for DailySummary.xaml
    /// </summary>
    public partial class DailySummary : Window
    {
        private Grid grid;

        public DailySummary()
        {
            InitializeComponent();
            SetUpViewComponents();
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

        IDictionary<MenuItem, GridViewColumn> _columnBindings = new Dictionary<MenuItem, GridViewColumn>();
        
        private void SetContextMenuForColumn(GridViewColumn column)
        {
            switch (column.Header)
            {
                case "Process Name":
                    AddSortMenuItems(column);
                    AddFilterMenuItem(column);
                    break;
                case "Window Title":
                    AddSortMenuItems(column);
                    AddFilterMenuItem(column);
                    break;
                case "Total Duration":
                    AddSortMenuItems(column);
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
            var contextMenu = InitialiseContextMenuColumn(column);
            var selectAllMenuItem = new MenuItem { Header = "Select All" };
            selectAllMenuItem.Click += SelectAllMenuItem_Click;
            contextMenu.Items.Add(selectAllMenuItem);
            _columnBindings[selectAllMenuItem] = column;
        }

        private void AddUnselectAllMenuItem(GridViewColumn column)
        {
            var contextMenu = InitialiseContextMenuColumn(column);
            var unselectAllMenuItem = new MenuItem { Header = "Unselect All" };
            unselectAllMenuItem.Click += UnselectAllMenuItem_Click;
            contextMenu.Items.Add(unselectAllMenuItem);
            _columnBindings[unselectAllMenuItem] = column;
        }

        private void AddFilterMenuItem(GridViewColumn column)
        {
            var contextMenu = InitialiseContextMenuColumn(column);

            var filterByMenuItem = new MenuItem { Header = "Filter" };
            filterByMenuItem.Click += FilterMenuItem_Click;
            contextMenu.Items.Add(filterByMenuItem);
            _columnBindings[filterByMenuItem] = column;
        }

        private void AddSortMenuItems(GridViewColumn column)
        {
            var contextMenu = InitialiseContextMenuColumn(column);
            
            var sortAscendingMenuItem = new MenuItem { Header = "Sort Ascending" };
            sortAscendingMenuItem.Click += SortAscendingMenuItem_Click;
            contextMenu.Items.Add(sortAscendingMenuItem);
            _columnBindings[sortAscendingMenuItem] = column;

            var sortdescendingMenuItem = new MenuItem { Header = "Sort Descending" };
            sortdescendingMenuItem.Click += SortDescendingMenuItem_Click;
            contextMenu.Items.Add(sortdescendingMenuItem);
            _columnBindings[sortdescendingMenuItem] = column;
        }

        private ContextMenu InitialiseContextMenuColumn(GridViewColumn column)
        {
            if (!(column.Header is GridViewColumnHeader))
            {
                column.Header = new GridViewColumnHeader() { Content = column.Header };
            }
            return (column.Header as GridViewColumnHeader).ContextMenu ??= new ContextMenu();
        }

        private ActivityViewModel _activityViewModel;
        private ICollection<ActivityRecord> _activityRecords;

        internal void AddRecords(SFDbContext sfDbContext)
        {
            _activityRecords = sfDbContext.ActivityRecords.Where(d => d.TimeStamp.Date == DateTime.Today.Date).ToList();
            DisplayActivitySummaries();
        }

        private void DisplayActivitySummaries()
        {
            foreach (var summary in _activityRecords.GroupBy(a => new { a.ProcessName, a.WindowTitle }))
            {
                _activityViewModel.AddNewActivitySummary(summary.Key.ProcessName, summary.Key.WindowTitle, summary.Count());                
            }
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _activityViewModel.SelectAllVisible();
        }

        private void UnselectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _activityViewModel.UnselectAllVisible();
        }

        private void FilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var filterDialog = new AddFilterDialog(_columnBindings[sender as MenuItem], typeof(string));
            if (filterDialog.ShowDialog() ?? false)
            {
                _activityViewModel.UpdateFilter(filterDialog.ColumnName, filterDialog.GetFilter());
            }
        }

        #region Sorting
        private void UpdateSorting(object sender, bool sortDescending)
        {
            if (TryGetColumnForMenuItem(sender, out var column))
            {
                _activityViewModel.UpdateSorting(column, sortDescending);            
            }
        }

        private void SortAscendingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(sender, false);
        }

        private void SortDescendingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateSorting(sender, true);
        } 

        #endregion

        private bool TryGetColumnForMenuItem(object sender, out GridViewColumn gridViewColumn)
        {
            gridViewColumn = null;
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Parent is ContextMenu contextMenu)
                {
                    if (contextMenu.PlacementTarget is GridViewColumnHeader gridViewColumnHeader)
                    {
                        gridViewColumn = gridViewColumnHeader.Column;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
