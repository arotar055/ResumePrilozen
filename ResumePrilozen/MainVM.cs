using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ResumePrilozen
{
    public class MainVM : INotifyPropertyChanged
    {
        private const string ResumesFileName = "DataResumes.txt";

        // Вводимые поля
        private string _userNameInput = "";
        private string _userAgeInput = "";
        private string _userAddressInput = "";
        private string _userEmailInput = "";
        private string _chosenMaritalStatus = "";

        // Флажки (навыки)
        private bool _skillCSharp;
        private bool _skillJava;
        private bool _skillPython;
        private bool _skillSql;
        private bool _skillHtml;
        private bool _skillCss;
        private bool _skillGit;
        private bool _skillDocker;
        private bool _skillScrum;
        private bool _skillEnglish;

        // Выбранное резюме
        private ResumeModel _selectedResume;

        public ObservableCollection<ResumeModel> AllResumes { get; set; }
            = new ObservableCollection<ResumeModel>();

        public ObservableCollection<string> MaritalStatuses { get; }
            = new ObservableCollection<string>
        {
            "Холост/Холоста",
            "Женат/Замужем",
            "Разведён/Разведена"
        };

        public ResumeModel SelectedResume
        {
            get => _selectedResume;
            set { _selectedResume = value; OnPropertyChanged(); }
        }

        // Поля ввода
        public string UserNameInput
        {
            get => _userNameInput;
            set
            {
                // Простая фильтрация
                if (value != null)
                {
                    // Разрешаем буквы, пробел, дефис
                    var filtered = Regex.Replace(value, @"[^a-zA-Zа-яА-ЯёЁ\s\-]+", "");
                    if (filtered != value) return;
                }
                _userNameInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddResume));
            }
        }
        public string UserAgeInput
        {
            get => _userAgeInput;
            set
            {
                if (!Regex.IsMatch(value ?? "", @"^\d{0,2}$"))
                    return;
                _userAgeInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddResume));
            }
        }
        public string UserAddressInput
        {
            get => _userAddressInput;
            set { _userAddressInput = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddResume)); }
        }
        public string UserEmailInput
        {
            get => _userEmailInput;
            set
            {
                _userEmailInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddResume));
            }
        }
        public string ChosenMaritalStatus
        {
            get => _chosenMaritalStatus;
            set { _chosenMaritalStatus = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddResume)); }
        }

        // Навыки
        public bool SkillCSharp
        {
            get => _skillCSharp;
            set { _skillCSharp = value; OnPropertyChanged(); }
        }
        public bool SkillJava
        {
            get => _skillJava;
            set { _skillJava = value; OnPropertyChanged(); }
        }
        public bool SkillPython
        {
            get => _skillPython;
            set { _skillPython = value; OnPropertyChanged(); }
        }
        public bool SkillSql
        {
            get => _skillSql;
            set { _skillSql = value; OnPropertyChanged(); }
        }
        public bool SkillHtml
        {
            get => _skillHtml;
            set { _skillHtml = value; OnPropertyChanged(); }
        }
        public bool SkillCss
        {
            get => _skillCss;
            set { _skillCss = value; OnPropertyChanged(); }
        }
        public bool SkillGit
        {
            get => _skillGit;
            set { _skillGit = value; OnPropertyChanged(); }
        }
        public bool SkillDocker
        {
            get => _skillDocker;
            set { _skillDocker = value; OnPropertyChanged(); }
        }
        public bool SkillScrum
        {
            get => _skillScrum;
            set { _skillScrum = value; OnPropertyChanged(); }
        }
        public bool SkillEnglish
        {
            get => _skillEnglish;
            set { _skillEnglish = value; OnPropertyChanged(); }
        }

        // Условие, можно ли добавить
        public bool CanAddResume
        {
            get
            {
                // ФИО заполнено?
                if (string.IsNullOrWhiteSpace(UserNameInput)) return false;
                // Возраст в диапазоне?
                if (!int.TryParse(UserAgeInput, out int ageNum)) return false;
                if (ageNum < 1 || ageNum > 99) return false;
                // Семейное положение выбрано?
                if (string.IsNullOrEmpty(ChosenMaritalStatus)) return false;
                // Адрес заполнен?
                if (string.IsNullOrWhiteSpace(UserAddressInput)) return false;
                // Email не пуст
                if (string.IsNullOrWhiteSpace(UserEmailInput)) return false;
                // Можно добавить
                return true;
            }
        }

        // Команды
        public ICommand AddResumeCmd { get; }
        public ICommand ClearInputsCmd { get; }
        public ICommand ShowResumeCmd { get; }
        public ICommand DeleteSelectedCmd { get; }

        // Конструктор
        public MainVM()
        {
            AddResumeCmd = new RelayCmd(_ => AddResume(), _ => CanAddResume);
            ClearInputsCmd = new RelayCmd(_ => ClearInputs());
            ShowResumeCmd = new RelayCmd(_ => ShowSelectedResume(), _ => SelectedResume != null);
            DeleteSelectedCmd = new RelayCmd(_ => DeleteSelectedResume(), _ => SelectedResume != null);

            LoadFromFile();
        }

        private void AddResume()
        {
            try
            {
                int age = int.TryParse(UserAgeInput, out int parsedAge) ? parsedAge : 0;
                var skillList = CollectSkills();

                // Создаем новую модель
                var newR = new ResumeModel
                {
                    UserFullName = UserNameInput.Trim(),
                    UserAge = age,
                    MaritalStat = ChosenMaritalStatus,
                    UserAddress = UserAddressInput.Trim(),
                    UserEmail = UserEmailInput.Trim(),
                    UserSkills = string.Join(",", skillList)
                };

                // Добавляем в коллекцию
                AllResumes.Add(newR);
                // Добавляем в файл (дозапись)
                AppendToFile(newR);

                // Сброс
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении резюме: " + ex.Message);
            }
        }

        private void DeleteSelectedResume()
        {
            if (SelectedResume == null) return;
            var confirm = MessageBox.Show($"Удалить '{SelectedResume.UserFullName}' из базы?",
                                          "Подтверждение",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                AllResumes.Remove(SelectedResume);
                RewriteAllFile();
                SelectedResume = null;
            }
        }

        private void ShowSelectedResume()
        {
            if (SelectedResume == null) return;
            var dlg = new DetailWindow(SelectedResume);
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void ClearInputs()
        {
            UserNameInput = "";
            UserAgeInput = "";
            UserAddressInput = "";
            UserEmailInput = "";
            ChosenMaritalStatus = "";

            SkillCSharp = false;
            SkillJava = false;
            SkillPython = false;
            SkillSql = false;
            SkillHtml = false;
            SkillCss = false;
            SkillGit = false;
            SkillDocker = false;
            SkillScrum = false;
            SkillEnglish = false;
        }

        private void LoadFromFile()
        {
            try
            {
                if (!File.Exists(ResumesFileName))
                {
                    return;
                }
                var lines = File.ReadAllLines(ResumesFileName);
                AllResumes.Clear();
                foreach (var ln in lines)
                {
                    var obj = ResumeModel.FromLine(ln);
                    if (obj != null)
                        AllResumes.Add(obj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки из файла: " + ex.Message);
            }
        }

        private void AppendToFile(ResumeModel r)
        {
            try
            {
                using (var writer = new StreamWriter(ResumesFileName, true))
                {
                    writer.WriteLine(r.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка записи в файл: " + ex.Message);
            }
        }

        private void RewriteAllFile()
        {
            try
            {
                using (var writer = new StreamWriter(ResumesFileName, false))
                {
                    foreach (var r in AllResumes)
                    {
                        writer.WriteLine(r.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка перезаписи файла: " + ex.Message);
            }
        }

        private System.Collections.Generic.List<string> CollectSkills()
        {
            var skillList = new System.Collections.Generic.List<string>();
            if (SkillCSharp) skillList.Add("CSharp");
            if (SkillJava) skillList.Add("Java");
            if (SkillPython) skillList.Add("Python");
            if (SkillSql) skillList.Add("SQL");
            if (SkillHtml) skillList.Add("HTML");
            if (SkillCss) skillList.Add("CSS");
            if (SkillGit) skillList.Add("Git");
            if (SkillDocker) skillList.Add("Docker");
            if (SkillScrum) skillList.Add("Scrum");
            if (SkillEnglish) skillList.Add("English");
            return skillList;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
