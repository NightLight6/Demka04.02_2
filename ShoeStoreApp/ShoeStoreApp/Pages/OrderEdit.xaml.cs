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

namespace ShoeStoreApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderEdit.xaml
    /// </summary>
    public partial class OrderEdit : Page
    {
        private Orders _order;
        private bool IsNew;
        private ShoeStoreDBEntities db = ShoeStoreDBEntities.GetContext();
        private List<PickupPoints> _pickupPoints;
        private List<Users> _allUsers;
        private List<OrderStatuses> _orderStatuses;

        public OrderEdit(Orders order)
        {
            InitializeComponent();
            _order = order;

            this.IsVisibleChanged += OrderEdit_IsVisibleChanged;

            LoadCMB();
            IsNew = _order == null;
            if (IsNew)
            {
                _order = new Orders();
            }
            else
            {
                LoadOrder();
            }
        }

        private void OrderEdit_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible && !IsNew)
            {
                LoadOrder();
            }
        }

        private void LoadOrder()
        {
            var freshOrder = db.Orders.Find(_order.OrderId);
            if (freshOrder != null)
            {
                _order = freshOrder;
            }

            tbOrderDate.Text = _order.OrderDate?.ToString("dd.MM.yyyy") ?? "";
            tbDeliveryDate.Text = _order.DeliveryDate?.ToString("dd.MM.yyyy") ?? "";
            tbCode.Text = _order.Code?.ToString() ?? "";
            cmbOrderStatus.SelectedValue = _order.OrderStatusId;
            cmbPickupPoint.SelectedValue = _order.PointId;
            cmbUser.SelectedValue = _order.UserId;
        }

        private void LoadCMB()
        {
            _orderStatuses = db.OrderStatuses.ToList();
            cmbOrderStatus.SelectedValuePath = "StatusId";
            cmbOrderStatus.DisplayMemberPath = "StatusName";
            cmbOrderStatus.ItemsSource = _orderStatuses;

            _pickupPoints = db.PickupPoints.ToList();
            cmbPickupPoint.SelectedValuePath = "PointId";
            cmbPickupPoint.DisplayMemberPath = "FullAdress";
            cmbPickupPoint.ItemsSource = _pickupPoints;

            _allUsers = db.Users.ToList();
            cmbUser.SelectedValuePath = "UserId";
            cmbUser.DisplayMemberPath = "Fullname";
            cmbUser.ItemsSource = _allUsers;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbOrderStatus.SelectedValue == null)
                    throw new Exception("Выберите статус заказа!");
                if (cmbPickupPoint.SelectedValue == null)
                    throw new Exception("Выберите пункт выдачи!");
                if (cmbUser.SelectedValue == null)
                    throw new Exception("Выберите пользователя!");

                if (DateTime.TryParse(tbOrderDate.Text, out DateTime parsedDate))
                {
                    _order.OrderDate = parsedDate;
                }
                else
                {
                    throw new Exception("Введите корректную дату заказа (дд.мм.гггг)!");
                }

                if (DateTime.TryParse(tbDeliveryDate.Text, out DateTime parsedDDate))
                {
                    _order.DeliveryDate = parsedDDate;
                }
                else
                {
                    throw new Exception("Введите корректную дату доставки (дд.мм.гггг)!");
                }

                _order.PointId = Convert.ToInt32(cmbPickupPoint.SelectedValue);
                _order.UserId = Convert.ToInt32(cmbUser.SelectedValue);
                _order.OrderStatusId = Convert.ToInt32(cmbOrderStatus.SelectedValue);

                if (!string.IsNullOrWhiteSpace(tbCode.Text))
                {
                    _order.Code = Convert.ToInt32(tbCode.Text);
                }
                else
                {
                    _order.Code = null;
                }

                if (IsNew)
                {
                    db.Orders.Add(_order);
                    MessageBox.Show("Заказ успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var localOrder = db.Orders.Find(_order.OrderId);
                    if (localOrder != null)
                    {
                        db.Entry(localOrder).CurrentValues.SetValues(_order);
                        MessageBox.Show("Данные заказа успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        throw new Exception("Заказ не найден в базе данных. Возможно, он был удален другим пользователем.");
                    }
                }
                db.SaveChanges();
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IsNew)
            {
                NavigationService.GoBack();
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ №{_order.OrderId}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var orderDB = db.Orders.FirstOrDefault(p => p.OrderId == _order.OrderId);

                    if (orderDB != null)
                    {
                        var orderInfos = db.OrderInfo.Where(oi => oi.OrderId == _order.OrderId).ToList();
                        if (orderInfos.Any())
                        {
                            db.OrderInfo.RemoveRange(orderInfos);
                        }

                        db.Orders.Remove(orderDB);
                        db.SaveChanges();
                        MessageBox.Show("Заказ успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Заказ уже удален или не найден в базе данных.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}