using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using ShoeStoreApp.Models;
using ShoeStoreApp.Pages;
using Authorization = ShoeStoreApp.Pages.Authorization;


namespace ShoeStoreApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Authorization());
        }
        public void SetName(string FullName)
        {
            if (string.IsNullOrEmpty(FullName))
            {
                txtblName.Text = "Гость";
            }
            else
            {
                txtblName.Text = FullName;
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Authorization)
            {
                txtblHeader.Text = "Авторизация";
                btnBack.Visibility = Visibility.Collapsed;
                btnExit.Visibility = Visibility.Collapsed;
                txtblName.Visibility = Visibility.Collapsed;
            }
            if (e.Content is ProductList)
            {
                txtblHeader.Text = "Список товаров";
                btnBack.Visibility = Visibility.Collapsed;
                btnExit.Visibility = Visibility.Visible;
                txtblName.Visibility = Visibility.Visible;
            }
            if (e.Content is ProductEdit)
            {
                txtblHeader.Text = "Редактирование товара";
                btnBack.Visibility = Visibility.Visible;
                btnExit.Visibility = Visibility.Visible;
                txtblName.Visibility = Visibility.Visible;
            }
            if (e.Content is OrderList)
            {
                txtblHeader.Text = "Список заказов";
                btnBack.Visibility = Visibility.Visible;
                btnExit.Visibility = Visibility.Visible;
                txtblName.Visibility = Visibility.Visible;
            }
            if (e.Content is OrderEdit)
            {
                txtblHeader.Text = "Редактирование заказа";
                btnBack.Visibility = Visibility.Visible;
                btnExit.Visibility = Visibility.Visible;
                txtblName.Visibility = Visibility.Visible;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            txtblName.Text = "";
            MainFrame.Navigate(new Authorization());
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is ProductList)
            {
                MainFrame.Navigate(new Authorization());
            }
            else
            {
                MainFrame.GoBack();
            }
        }
    }
}
