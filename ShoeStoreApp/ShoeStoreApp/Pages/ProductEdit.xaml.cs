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
    /// Логика взаимодействия для ProductEdit.xaml
    /// </summary>
    public partial class ProductEdit : Page
    {
        private Products _product;
        private string _sourceFilePath = null;
        private bool IsNewProduct;
        private List<Units> _allUnits = new List<Units>();
        private List<Suppliers> _allSuppliers = new List<Suppliers>();
        private List<Producers> _allProducers = new List<Producers>();
        private List<Categories> _allCategory = new List<Categories>();
        private ShoeStoreDBEntities db = ShoeStoreDBEntities.GetContext();

        public ProductEdit(Products products)
        {
            InitializeComponent();
            _product = products;

            this.IsVisibleChanged += ProductEdit_IsVisibleChanged;

            LoadCMB();
            IsNewProduct = _product == null;
            if (IsNewProduct)
            {
                _product = new Products();
            }
            else
            {
                LoadData();
            }
        }

        private void ProductEdit_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible && !IsNewProduct)
            {
                LoadData();
            }
        }

        private void LoadCMB()
        {
            _allUnits = db.Units.ToList();
            cmbUnits.SelectedValuePath = "UnitId";
            cmbUnits.DisplayMemberPath = "UnitName";
            cmbUnits.ItemsSource = _allUnits;

            _allProducers = db.Producers.ToList();
            cmbProducer.SelectedValuePath = "ProducerId";
            cmbProducer.DisplayMemberPath = "ProducerName";
            cmbProducer.ItemsSource = _allProducers;

            _allSuppliers = db.Suppliers.ToList();
            cmbSupplier.SelectedValuePath = "SupplierId";
            cmbSupplier.DisplayMemberPath = "SupplierName";
            cmbSupplier.ItemsSource = _allSuppliers;

            _allCategory = db.Categories.ToList();
            cmbCategory.SelectedValuePath = "CategoryId";
            cmbCategory.DisplayMemberPath = "CategoryName";
            cmbCategory.ItemsSource = _allCategory;
        }

        private void LoadData()
        {
            var freshProduct = db.Products.Find(_product.ProductId);
            if (freshProduct != null)
            {
                _product = freshProduct;
            }

            txbArticul.Text = _product.ProductArticle;
            txbProductName.Text = _product.ProductName;
            cmbUnits.SelectedValue = _product.UnitId;
            txbPrice.Text = _product.Price.ToString();
            cmbSupplier.SelectedValue = _product.SupplierId;
            cmbProducer.SelectedValue = _product.ProducerId;
            cmbCategory.SelectedValue = _product.CategoryId;
            txbDiscount.Text = _product.Discount.ToString();
            txbCount.Text = _product.Count.ToString();
            txbDescription.Text = _product.Description;

            _sourceFilePath = null;

            if (!string.IsNullOrEmpty(_product.Photo))
            {
                try
                {
                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _product.Photo);
                    if (System.IO.File.Exists(fullPath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        imgProduct.Source = bitmap;
                    }
                    else
                    {
                        imgProduct.Source = null;
                    }
                }
                catch
                {
                    imgProduct.Source = null;
                }
            }
            else
            {
                imgProduct.Source = null;
            }
        }

        private bool ValidationFields(out decimal price, out int count, out int discount)
        {
            string error = "";
            price = 0; count = 0; discount = 0;

            if (string.IsNullOrWhiteSpace(txbProductName.Text)) error += "Введите название товара!\n";
            if (cmbUnits.SelectedValue == null) error += "Выберите единицу измерения.\n";
            if (cmbCategory.SelectedValue == null) error += "Выберите категорию.\n";
            if (cmbProducer.SelectedValue == null) error += "Выберите производителя.\n";
            if (cmbSupplier.SelectedValue == null) error += "Выберите поставщика.\n";
            if (!decimal.TryParse(txbPrice.Text, out price) || price <= 0) error += "Введите корректную цену > 0.\n";
            if (!int.TryParse(txbCount.Text, out count) || count < 0) error += "Количество должно быть целым числом >= 0.\n";
            if (!int.TryParse(txbDiscount.Text, out discount) || discount < 0 || discount > 100) error += "Скидка должна быть числом от 0 до 100.\n";

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidationFields(out decimal validPrice, out int validcount, out int validDiscount))
                {
                    _product.ProductArticle = txbArticul.Text;
                    _product.ProductName = txbProductName.Text;
                    _product.UnitId = (int)cmbUnits.SelectedValue;
                    _product.Price = validPrice;
                    _product.SupplierId = (int)cmbSupplier.SelectedValue;
                    _product.ProducerId = (int)cmbProducer.SelectedValue;
                    _product.CategoryId = (int)cmbCategory.SelectedValue;
                    _product.Discount = validDiscount;
                    _product.Count = validcount;
                    _product.Description = txbDescription.Text;

                    if (!string.IsNullOrEmpty(_sourceFilePath))
                    {
                        string fileName = System.IO.Path.GetFileName(_sourceFilePath);
                        string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                        System.IO.Directory.CreateDirectory(targetFolder);
                        string targetPath = System.IO.Path.Combine(targetFolder, fileName);

                        System.IO.File.Copy(_sourceFilePath, targetPath, true);
                        _product.Photo = fileName;
                        _sourceFilePath = null;
                    }

                    if (IsNewProduct)
                    {
                        db.Products.Add(_product);
                        MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var localProduct = db.Products.Find(_product.ProductId);
                        if (localProduct != null)
                        {
                            db.Entry(localProduct).CurrentValues.SetValues(_product);
                            MessageBox.Show("Данные товара успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    db.SaveChanges();
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IsNewProduct)
            {
                NavigationService.GoBack();
                return;
            }

            var result = MessageBox.Show($"Вы уверены что хотите удалить: {_product.ProductName}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var productDB = db.Products.FirstOrDefault(p => p.ProductId == _product.ProductId);

                    if (productDB != null)
                    {
                        db.Products.Remove(productDB);
                        db.SaveChanges();
                        MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Товар уже удален или не найден", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _sourceFilePath = openFileDialog.FileName;
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_sourceFilePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgProduct.Source = bitmap;
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}