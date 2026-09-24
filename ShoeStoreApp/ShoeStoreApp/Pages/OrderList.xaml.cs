using ShoeStoreApp.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace ShoeStoreApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderList.xaml
    /// </summary>
    public partial class OrderList : Page
    {
        private Users _currentUser;
        private List<Orders> _allOrders = new List<Orders>();

        public OrderList(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            this.IsVisibleChanged += OrderList_IsVisibleChanged;

            LoadButtons();
            LoadData();
        }

        private void OrderList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            using (var db = ShoeStoreDBEntities.GetContext())
            {
                _allOrders = db.Orders
                    .Include(o => o.Users)
                    .Include(o => o.OrderStatuses)
                    .Include(o => o.PickupPoints.Cities)
                    .ToList();
                lbOrders.ItemsSource = _allOrders;
            }
        }

        private void LoadButtons()
        {
            if (_currentUser == null) return;

            switch (_currentUser.RoleId)
            {
                case 1:
                    btnAdd.Visibility = Visibility.Visible;
                    btnEdit.Visibility = Visibility.Visible;
                    break;
                case 2:
                    btnAdd.Visibility = Visibility.Collapsed;
                    btnEdit.Visibility = Visibility.Collapsed;
                    break;
                default:
                    btnAdd.Visibility = Visibility.Collapsed;
                    btnEdit.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lbOrders.SelectedItem is Orders selectedOrder)
            {
                NavigationService.Navigate(new OrderEdit(selectedOrder));
            }
            else
            {
                MessageBox.Show("Выберите заказ для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrderEdit(null));
        }
    }
}