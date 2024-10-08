using ClientWPF.Core;
using ClientWPF.Repositories.Implementation;
using HashGenerators;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ClientWPF.MVVM.ViewModel
{
    internal class RegistrationViewModel : ObservableObject
    {
        private readonly UsersRepository _usersRepository;
        private readonly UserImagesRepository _userImagesRepository;
        private readonly BankAccountsRepository _bankAccountsRepository;
        private readonly Regex _regexForUsername;
        private readonly Regex _regexForPasswordWeak;
        private readonly Regex _regexForPasswordAverage;
        private readonly Regex _regexForPasswordStrong;
        public RegistrationViewModel()
        {
            _usersRepository = new UsersRepository();
            _bankAccountsRepository = new BankAccountsRepository();
            _userImagesRepository = new UserImagesRepository();

            _regexForUsername = new Regex("^[a-zA-Z0-9]+$");
            _regexForPasswordWeak = new Regex("^[a-zA-Z0-9]+$");
            _regexForPasswordAverage = new Regex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
            _regexForPasswordStrong = new Regex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$");

            _foregroundColor = System.Windows.Media.Brushes.Black;
            // Установить фотографию пользователя по умолчанию
            ImagePath = @"../../Images/defUser.png";
        }

        #region Accessors (helpers for ui design)
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                if (!String.IsNullOrEmpty(value))
                {
                    if (_regexForPasswordStrong.IsMatch(value))
                    {
                        PasswordLineWidth = 300;
                        ForegroundColor = System.Windows.Media.Brushes.Green;
                        PasswordCheckerLabel = "Strong";
                    }
                    else if (_regexForPasswordAverage.IsMatch(value))
                    {
                        PasswordLineWidth = 150;
                        ForegroundColor = System.Windows.Media.Brushes.Orange;
                        PasswordCheckerLabel = "Average";
                    }
                    else if (_regexForPasswordWeak.IsMatch(value))
                    {
                        PasswordLineWidth = 80;
                        ForegroundColor = System.Windows.Media.Brushes.Red;
                        PasswordCheckerLabel = "Weak";
                    }
                }
                else
                    PasswordCheckerLabel = String.Empty;
                OnPropertyChanged(nameof(Password));
            }
        }
        private string _password2;
        public string Password2
        {
            get { return _password2; }
            set
            {
                _password2 = value;
                OnPropertyChanged(nameof(Password2));
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
        private string _passwordCheckerLabel;
        public string PasswordCheckerLabel
        {
            get { return _passwordCheckerLabel; }
            set
            {
                _passwordCheckerLabel = value;
                OnPropertyChanged(nameof(PasswordCheckerLabel));
            }
        }
        private System.Windows.Media.Brush _foregroundColor;
        public System.Windows.Media.Brush ForegroundColor
        {
            get { return _foregroundColor; }
            set
            {
                _foregroundColor = value;
                OnPropertyChanged("ForegroundColor");
            }
        }
        private int _passwordLineWidth;
        public int PasswordLineWidth
        {
            get { return _passwordLineWidth; }
            set
            {
                _passwordLineWidth = value;
                OnPropertyChanged(nameof(PasswordLineWidth));
            }
        }
        private bool _isAdmin;
        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }
        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }
        #endregion

        #region Commands
        private readonly RelayCommand _signUp;
        public RelayCommand SignUp
        {
            get
            {
                return _signUp ?? (new RelayCommand(async obj =>
                {
                    if (!_regexForUsername.IsMatch(Name) || String.IsNullOrWhiteSpace(Name))
                        MessageBox.Show("Введите правильное имя!");
                    else if (String.IsNullOrWhiteSpace(Password))
                        MessageBox.Show("Пожалуйста, введите правильный пароль!");
                    else if (Password != Password2)
                        MessageBox.Show("Пароли не совпадают!");
                    else if (!_regexForPasswordAverage.IsMatch(Password))
                        MessageBox.Show("Пароль слишком слабый!");
                    else
                    {
                        User userWithEqualName = _usersRepository.FindUserByName(Name);
                        if (userWithEqualName != null)
                            MessageBox.Show($"Пользователь с именем -> {Name} уже был зарегистрирован! Попробуйте изменить свое имя пользователя.");
                        else
                        {
                            User newUser = new User()
                            {
                                Name = Name,
                                Password = MD5Generator.ProduceMD5Hash(Password),
                                RegistrationDate = DateTime.Now
                            };
                            if (IsAdmin && SecretPhrase.ToUpper() == "Админ!")
                                newUser.RoleId = 1; // Admin
                            else if (IsAdmin && SecretPhrase.ToUpper() != "Админ!")
                            {
                                MessageBox.Show("Нет, вы не администратор!");
                                newUser.RoleId = 2; // User
                            }
                            else
                                newUser.RoleId = 2; // User
                            int okay = await _usersRepository.AddUser(newUser);
                            if (okay == 1)
                            {
                                User addedUser = _usersRepository.FindUserByName(Name);
                                if (ImagePath != @"../../Images/defUser.png")
                                {
                                    _userImagesRepository.AddImageByUserId(ImagePath, addedUser.Id);
                                }
                                if (newUser.RoleId == 2)
                                {
                                    BankAccount ba = new BankAccount();
                                    ba.UserId = addedUser.Id;
                                    ba.MoneyAmount = 10000;
                                    _bankAccountsRepository.AddBankAccount(ba);
                                    MessageBox.Show($"{newUser.Name} был успешно добавлен! Мы выдали вам 10000$ на ваш первый депозит.");
                                }
                                else
                                    MessageBox.Show($"{newUser.Name} как администратор был успешно добавлен!");
                            }
                            else
                                MessageBox.Show($"Что-то пошло не так... Попробуй позже.");
                            App.Current.Windows[1].Close();
                        }

                    }
                }));
            }
        }
        private readonly RelayCommand _backToSignIn;
        public RelayCommand BackToSignIn
        {
            get
            {
                return _backToSignIn ?? (new RelayCommand(obj =>
                {
                    App.Current.Windows[1].Close();
                }));
            }
        }
        private readonly RelayCommand _openFileDialog;
        public RelayCommand OpenFileDialog
        {
            get
            {
                return _openFileDialog ?? (new RelayCommand(obj =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Multiselect = false;
                    ofd.Title = "Выберите свою фотографию";
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.apng;*.avif;*.gif;*.jfif;*.pjpeg";
                    ofd.ShowDialog();
                    ImagePath = ofd.FileName;
                }));
            }
        }
        #endregion
    }
}
