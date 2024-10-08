using ClientWPF.Core;
using ClientWPF.MVVM.View;
using ClientWPF.Repositories.Implementation;
using HashGenerators;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ClientWPF.MVVM.ViewModel
{
    internal class AccountViewModel : ObservableObject
    {
        private const string _secret = ""; 
        private int attempts;
        private readonly UsersRepository _usersRepository;
        public ObservableCollection<User> Users { get; set; }
        public AccountViewModel()
        {
            _usersRepository = new UsersRepository();
            IsAdmin = false;
            attempts = 2;
        }

        #region Accessors
        private string _name;
        public string Name 
        {
            get { return _name; }
            set 
            { 
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        private string _secretPhrase;
        public string SecretPhrase
        {
            get { return _secretPhrase; }
            set
            {
                _secretPhrase = value;
                OnPropertyChanged(nameof(SecretPhrase));
            }
        }
        private bool _isAdmin;
        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                SecretPhrase = String.Empty;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }
        #endregion

        #region Commands
        private readonly RelayCommand _signIn;
        public RelayCommand SignIn
        {
            get
            {
                return _signIn ?? (new RelayCommand(async obj =>
                {
                    User user = _usersRepository.FindUserByName(Name);
                    if (user != null && attempts > 0)
                    {


                        // Администратор или пользователь
                        if ((IsAdmin && SecretPhrase.ToLower().Trim() == "Админ!" && user.Password == Password && user.Name == Name)
                        || (!IsAdmin && user.Name == Name && user.Password == Password))
                        {
                            // Успех
                            MainWindow window = new MainWindow(user);
                            window.Show();
                            App.Current.MainWindow = window;
                            App.Current.Windows[0].Close(); // Закрыть окно авторизации
                        }
                        else
                            MessageBox.Show($"Неверные данные! Попытки осталось: {attempts--}");
                    }
                    else
                    {
                        if (attempts > 0)
                            MessageBox.Show($"Пользователи не найдены! Попыток остлось: {attempts--}");
                        else
                        {
                            MessageBox.Show("Попробуй позже.");
                            App.Current.MainWindow.Close();
                        }
                    }
                }));
            }
        }
        private readonly RelayCommand _signUp;
        public RelayCommand SignUp
        {
            get
            {
                return _signUp ?? (new RelayCommand(async obj =>
                {
                    RegistrationView registrationView = new RegistrationView();
                    registrationView.ShowDialog();
                    registrationView.Focus();
                }));
            }
        }
        #endregion
    }
}
