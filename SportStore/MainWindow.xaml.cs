﻿using SportStore.Models;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SportStore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 11	Пахомова	Аиша	Анатольевна	m4ic8j5qgstw@gmail.com	2L6KZG	1
    public partial class MainWindow : Window
    {
        bool admin = false;
        public MainWindow(User user=null)
        {
            InitializeComponent();
            
           
            addUser.Visibility = Visibility.Hidden;
            deleteUser.Visibility = Visibility.Hidden;

            using (SportStoreContext db = new SportStoreContext())
            {
                if (user != null)
                {
                    statusUser.Text = user.RoleNavigation.Name;
                    if (user.Role == 1)
                    {
                        addUser.Visibility = Visibility.Visible;
                        deleteUser.Visibility = Visibility.Visible;
                        admin = true;
                    }
                    statusUser.Text = user.RoleNavigation.Name;
                    MessageBox.Show($"{user.RoleNavigation.Name}: {user.Surname} {user.Name} {user.Patronymic}. \r\t");
                }
                else
                {
                    statusUser.Text = "Гость";
                    MessageBox.Show("Гость");
                }

                productlistView.ItemsSource = db.Products.ToList();
                 

                List<string> sortList = new List<string>() { "По возрастанию цены", "По убыванию цены" };
                sortUserComboBox.ItemsSource = sortList;

                List<string> filtertList = db.Products.Select(u => u.Manufacturer).Distinct().ToList();
                filtertList.Insert(0, "Все производители");
                filterUserComboBox.ItemsSource = filtertList.ToList();

                countProducts.Text = $"Количество: {db.Products.Count()}";
            }
             

        }


        private void UpdateProducts()
        {
            using (SportStoreContext db = new SportStoreContext())
            {

                var currentProducts = db.Products.ToList();
                productlistView.ItemsSource = currentProducts;

                //Сортировка
                if (sortUserComboBox.SelectedIndex != -1)
                {
                    if (sortUserComboBox.SelectedValue == "По убыванию цены")
                    {
                        currentProducts = currentProducts.OrderByDescending(u => u.Cost).ToList();

                    }

                    if (sortUserComboBox.SelectedValue == "По возрастанию цены")
                    {
                        currentProducts = currentProducts.OrderBy(u => u.Cost).ToList();

                    }
                }


                // Фильтрация
                if (filterUserComboBox.SelectedIndex != -1)
                {
                    if (db.Products.Select(u => u.Manufacturer).Distinct().ToList().Contains(filterUserComboBox.SelectedValue))
                    {
                        currentProducts = currentProducts.Where(u => u.Manufacturer == filterUserComboBox.SelectedValue.ToString()).ToList();
                    }
                    else
                    {
                        currentProducts = currentProducts.ToList();
                    }
                }

                // Поиск

                if (searchBox.Text.Length > 0)
                {

                    currentProducts = currentProducts.Where(u => u.Name.Contains(searchBox.Text) || u.Description.Contains(searchBox.Text)).ToList();

                }


                countProducts.Text = $"Количество: {currentProducts.Count} из {db.Products.ToList().Count}";

                productlistView.ItemsSource = currentProducts;
            }
        }

        private void сlearButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
            sortUserComboBox.SelectedIndex = -1;
            filterUserComboBox.SelectedIndex = -1;
        }

        private void EditProduct_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (admin)
            {  
                new AddProductWindow(ShowPanel.DataContext as Product).ShowDialog();
            } 

        }


        private void filterUserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (SportStoreContext db = new SportStoreContext())
            {
                if (db.Products.Select(u => u.Manufacturer).Distinct().ToList().Contains(filterUserComboBox.SelectedValue))
                {
                    productlistView.ItemsSource = db.Products.Where(u => u.Manufacturer == filterUserComboBox.SelectedValue).ToList();
                    
                }
                else
                {
                    productlistView.ItemsSource = db.Products.ToList();
                    
                }
            }
            UpdateProducts();
        }


        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (SportStoreContext db = new SportStoreContext())
            {
                if (searchBox.Text.Length > 0)
                {
                    productlistView.ItemsSource = db.Products.Where(u => u.Name.Contains(searchBox.Text) || u.Description.Contains(searchBox.Text)).ToList();
                    
                }

            }
            UpdateProducts();
        }


        private void sortUserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (SportStoreContext db = new SportStoreContext())
            {
                if (sortUserComboBox.SelectedValue == "По убыванию цены")
                {
                    productlistView.ItemsSource = db.Products.OrderByDescending(u => u.Cost).ToList();
               
                }

                if (sortUserComboBox.SelectedValue == "По возрастанию цены")
                {
                    productlistView.ItemsSource = db.Products.OrderBy(u => u.Cost).ToList();
                }
            }
            UpdateProducts();
        }



        private void exitButtonClick(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void addUserButtonClick(object sender, RoutedEventArgs e)
        {

            new AddProductWindow(null).ShowDialog();

        }
        private bool CanDeleteProduct(Product product)
        {
            using (SportStoreContext db = new SportStoreContext())
            {

                // провека наличи оригинального товара в заказе
                OrderProduct position = db.OrderProducts.Where(o => o.ProductId == product.Id).FirstOrDefault() as OrderProduct;

                if (position is not null)
                {
                    MessageBox.Show($"Товар: {product.Name} присутствует в товарной позиции заказа {position.OrderId}. \n Товар нельзя удалить!");
                    return false;
                }

                // найдем все связанные товары с данным товаром
                List<RelatedProduct> rp = db.RelatedProducts.Where(p => p.ProductId == product.Id).ToList();

                // найдем, существуют ли товарные позиции из нашего списка связанных товаров и самого товара в заказах
                foreach (RelatedProduct r in rp)
                {
                    OrderProduct order = db.OrderProducts.Where(o => o.ProductId == r.RelatedProdutId).FirstOrDefault() as OrderProduct;
                    if (order is not null)
                    {
                        Product p = db.Products.Where(p => p.Id == r.RelatedProdutId).FirstOrDefault() as Product;
                        MessageBox.Show($"Товар {p.Name} связан с товаром {product.Name} присутствует в товарной позиции заказа {order.OrderId}. \n Товары нельзя удалить!");
                        return false;
                    }
                }
                 
                return true;
            }
        }

        private void delUserButton(object sender, RoutedEventArgs e)
        {

            using (SportStoreContext db = new SportStoreContext())
            {

                var product = (productlistView.SelectedItem) as Product;

                if (CanDeleteProduct(product))
                {
                    if (product != null)
                    {

                        if (MessageBox.Show($"Вы точно хотите удалить {product.Name}", "Внимание!",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            db.Products.Remove(product);
                            db.SaveChanges();
                            MessageBox.Show($"Товар {product.Name} удален!");
                            productlistView.ItemsSource = db.Products.ToList();
                            countProducts.Text = $"Количество: {db.Products.Count()}";
                        }
                    }
                }
            }

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Product p = (productlistView.SelectedItem) as Product;
            ShowPanel.DataContext = p;
        }
    }
}