using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StayFocused
{
    public class CustomGridView : GridView
    {
        private ContextMenu processNameMenu;
        private ContextMenu timespanColumnContextMenu;

        public CustomGridView() : base()
        {
            SetupColumnContextMenus();
        }

        private void OrderByMenuItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FilterByMenuItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
            
 

            foreach (var column in Columns)
            {
                
            }

            //// Assign the context menus to their respective columns
            //NameColumnHeader.ContextMenu = nameColumnContextMenu;
            //TimespanColumnHeader.ContextMenu = timespanColumnContextMenu;
        }
    }

    public class CustomGridViewColumn : GridViewColumn
    {

    }
}
