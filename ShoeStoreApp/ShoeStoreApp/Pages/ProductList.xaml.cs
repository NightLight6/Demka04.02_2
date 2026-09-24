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
    /// Логика взаимодействия для ProductList.xaml
    /// </summary>
    public partial class ProductList : Page
    {
        private List<Products> _allProducts = new List<Products>();
        private List<Suppliers> _allSuppliers = new List<Suppliers>();
        private Users _currentUser;

        public ProductList(Users user)
        {
            InitializeComponent();
            _currentUser = user;

            this.IsVisibleChanged += ProductList_IsVisibleChanged;

            if (_currentUser != null)
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.SetName(_currentUser.Fullname);
                }
            }
            else
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.SetName(string.Empty);
                }
            }
            LoadData();
            LoadFilters();
            GetRole();
        }

        private void ProductList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                LoadData();
            }
        }

        private void GetRole()
        {
            if (_currentUser != null)
            {
                switch (_currentUser.RoleId)
                {
                    case 1:
                        HeadPanel.Visibility = Visibility.Visible;
                        btnAdd.Visibility = Visibility.Visible;
                        btnOrders.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        HeadPanel.Visibility = Visibility.Visible;
                        btnAdd.Visibility = Visibility.Collapsed;
                        btnOrders.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        HeadPanel.Visibility = Visibility.Collapsed;
                        btnAdd.Visibility = Visibility.Collapsed;
                        btnOrders.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        HeadPanel.Visibility = Visibility.Collapsed;
                        btnAdd.Visibility = Visibility.Collapsed;
                        btnOrders.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void LoadData()
        {
            using (var db = ShoeStoreDBEntities.GetContext())
            {
                _allProducts = db.Products.Include(p => p.Units).Include(p => p.Suppliers)
                    .Include(p => p.Producers).Include(p => p.Categories).ToList();
                _allSuppliers = db.Suppliers.ToList();

                ApplyFilters();
            }
        }

        private void LoadFilters()
        {
            var allSuppliers = new Suppliers { SupplierId = 0, SupplierName = "Все производители" };
            _allSuppliers.Insert(0, allSuppliers);
            cmbFilter.ItemsSource = _allSuppliers;
            cmbFilter.DisplayMemberPath = "SupplierName";
            cmbFilter.SelectedValuePath = "SupplierId";
            cmbFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (_allProducts == null || _allProducts.Count == 0)
            {
                lbProducts.ItemsSource = null;
                return;
            }

            var filtered = _allProducts.AsEnumerable();

            string searchText = tbFind.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(p => p.ProductName.ToLower().Contains(searchText)
                    || p.Categories.CategoryName.ToLower().Contains(searchText)
                    || p.Description.ToLower().Contains(searchText));
            }

            if (cmbFilter.SelectedValue is int selectedProducerId && selectedProducerId > 0)
            {
                filtered = filtered.Where(p => p.ProducerId == selectedProducerId);
            }

            if (cmbSort.SelectedItem is ComboBoxItem selectedSort)
            {
                switch (selectedSort.Content.ToString())
                {
                    case "Количество по возр.":
                        filtered = filtered.OrderBy(p => p.Count);
                        break;
                    case "Количество по уб.":
                        filtered = filtered.OrderByDescending(p => p.Count);
                        break;
                }
            }

            lbProducts.ItemsSource = filtered.ToList();
        }

        private void tbFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductEdit(null));
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                NavigationService.Navigate(new OrderList(_currentUser));
            }
        }

        private void lbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentUser != null)
            {
                if (_currentUser.RoleId != 1)
                {
                    return;
                }
                if (lbProducts.SelectedItem is Products selectedProduct)
                {
                    NavigationService.Navigate(new ProductEdit(selectedProduct));
                    lbProducts.SelectedItem = null;
                }
            }
        }
    }
}