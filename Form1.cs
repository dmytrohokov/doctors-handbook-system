using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DoctorsHandbook
{
    /// <summary>
    /// Головна форма додатку "Електронний довідник лікаря".
    /// Реалізує графічний інтерфейс користувача (GUI) та зв'язує його з бізнес-логікою.
    /// </summary>
    public partial class Form1 : Form
    {
        // Об'єкт репозиторію для управління даними (завантаження, збереження, списки хвороб та складу)
        private readonly HandbookRepository _repository = new HandbookRepository();

        // Головні контейнери інтерфейсу (вкладки)
        private TabControl mainTabControl;
        private TabPage pageDiseases;
        private TabPage pageStorage;

        // Візуальні компоненти вкладки "Довідник захворювань"
        private ListBox listDiseases;
        private TextBox txtTitle, txtSymptoms, txtProcedures, txtSearch;
        private DataGridView gridMedicines;
        private Button btnSaveDisease, btnRemoveDisease, btnCreateRecipe, btnClearFields;

        // Візуальні компоненти вкладки "Склад медикаментів"
        private DataGridView gridStorage;
        private TextBox txtNewMedName, txtNewMedQty, txtSubstituteName;
        private Button btnAddStorage, btnRemoveStorage, btnLinkSubstitute;

        /// <summary>
        /// Конструктор форми. Задає первинні налаштування вікна та ініціалізує дані.
        /// </summary>
        public Form1()
        {
            // Налаштування головних параметрів вікна згідно з ергономічними вимогами
            this.Text = "Електронний довідник лікаря — Курсова робота ООП";
            this.Size = new Size(1020, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10f); // Сучасний читабельний шрифт

            // Завантаження бази даних із файлу через репозиторій
            _repository.LoadData();

            // Динамічне створення інтерфейсу та заповнення його даними
            CreateUserInterface();
            UpdateUserInterfaceData();
        }

        /// <summary>
        /// Ініціалізує головний контейнер вкладок додатку.
        /// </summary>
        private void CreateUserInterface()
        {
            mainTabControl = new TabControl { Dock = DockStyle.Fill };
            pageDiseases = new TabPage { Text = "Довідник захворювань та призначення" };
            pageStorage = new TabPage { Text = "Склад медикаментів лікарні" };

            mainTabControl.TabPages.Add(pageDiseases);
            mainTabControl.TabPages.Add(pageStorage);
            this.Controls.Add(mainTabControl);

            // Покрокове налаштування кожної окремої вкладки
            SetupDiseasesTab();
            SetupStorageTab();
        }

        #region ІНІЦІАЛІЗАЦІЯ ІНТЕРФЕЙСУ ВКЛАДОК

        /// <summary>
        /// Організація візуальних елементів на вкладці довідника захворювань.
        /// </summary>
        private void SetupDiseasesTab()
        {
            // 1. Основна робоча область лікаря (займає весь доступний простір праворуч)
            Panel pnlDoctorWorkspace = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            Label lblT = new Label { Text = "Назва захворювання:", Top = 15, Left = 15, Width = 150 };
            txtTitle = new TextBox { Top = 15, Left = 180, Width = 480 };

            Label lblS = new Label { Text = "Симптоми (через кому):", Top = 50, Left = 15, Width = 160 };
            txtSymptoms = new TextBox { Top = 50, Left = 180, Width = 480 };

            Label lblP = new Label { Text = "Лікувальні процедури:", Top = 85, Left = 15, Width = 150 };
            txtProcedures = new TextBox { Top = 85, Left = 180, Width = 480, Multiline = true, Height = 60 };

            Label lblM = new Label { Text = "Рекомендовані лікарські засоби:", Top = 160, Left = 15, Width = 250 };

            // Таблиця для введення рецептурних компонентів хвороби
            gridMedicines = new DataGridView
            {
                Top = 185,
                Left = 15,
                Width = 645,
                Height = 145,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToDeleteRows = true,
                AllowUserToAddRows = true
            };
            gridMedicines.Columns.Add("MedName", "Назва препарату");
            gridMedicines.Columns.Add("MedQty", "Кількість на курс (од.)");

            // Кнопки керування карткою захворювання
            btnSaveDisease = new Button { Text = "Зберегти хворобу", Top = 345, Left = 15, Width = 180, Height = 38, BackColor = Color.LightGreen };
            btnSaveDisease.Click += BtnSaveDisease_Click;

            btnRemoveDisease = new Button { Text = "Видалити картку", Top = 345, Left = 205, Width = 150, Height = 38, BackColor = Color.LightCoral };
            btnRemoveDisease.Click += BtnRemoveDisease_Click;

            btnClearFields = new Button { Text = "Очистити поля", Top = 345, Left = 365, Width = 140, Height = 38 };
            btnClearFields.Click += (s, e) => ResetDoctorWorkspace();

            // Головна кнопка призначення лікування пацієнту
            btnCreateRecipe = new Button { Text = "ВИПИСАТИ РЕЦЕПТ ТА ОНОВИТИ СКЛАД", Top = 400, Left = 15, Width = 645, Height = 55, Font = new Font(this.Font, FontStyle.Bold), BackColor = Color.LightBlue };
            btnCreateRecipe.Click += BtnCreateRecipe_Click;

            // Додавання елементів управління на робочу панель
            pnlDoctorWorkspace.Controls.AddRange(new Control[] { lblT, txtTitle, lblS, txtSymptoms, lblP, txtProcedures, lblM, gridMedicines, btnSaveDisease, btnRemoveDisease, btnClearFields, btnCreateRecipe });

            // 2. Ліва бокова навігаційна панель (Пошук та вибір хвороб)
            Panel pnlLeftNav = new Panel { Width = 280, Dock = DockStyle.Left, Padding = new Padding(8) };

            // Текстове поле пошуку з ефектом Placeholder (водяного знаку)
            txtSearch = new TextBox { Dock = DockStyle.Top, Height = 30, Text = "Введіть симптом для пошуку...", ForeColor = Color.Gray };
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Введіть симптом для пошуку...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Введіть симптом для пошуку..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Список для відображення знайдених/наявних діагнозів
            listDiseases = new ListBox { Dock = DockStyle.Fill };
            listDiseases.SelectedIndexChanged += ListDiseases_SelectedIndexChanged;

            pnlLeftNav.Controls.Add(listDiseases);
            pnlLeftNav.Controls.Add(txtSearch);

            // Порядок додавання елементів управління на вкладку хвороб
            pageDiseases.Controls.Add(pnlDoctorWorkspace);
            pageDiseases.Controls.Add(pnlLeftNav);
        }

        /// <summary>
        /// Організація візуальних елементів на вкладці складського обліку лікарні.
        /// </summary>
        private void SetupStorageTab()
        {
            // НАДІЙНЕ РІШЕННЯ: Використовуємо абсолютні координати замість DockStyle, 
            // щоб гарантовано уникнути багів перекриття шарів у WinForms (як на скриншоті 1).

            // 1. Конфігурація таблиці складу (Верхня частина сторінки)
            gridStorage = new DataGridView
            {
                Top = 10,
                Left = 10,
                Width = 980, // Оптимальна ширина під розмір форми 1020
                Height = 250, // Чітко зафіксована висота таблиці
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // 2. Конфігурація панелі з формами введення (Розміщується строго ПІД таблицею на Top = 270)
            Panel pnlStorageForms = new Panel
            {
                Top = 270,
                Left = 10,
                Width = 980,
                Height = 310,
                Padding = new Padding(10)
            };

            // Елементи оприбуткування/керування партіями ліків
            Label lblStoredName = new Label { Text = "Назва ліків:", Top = 15, Left = 15, Width = 150 };
            txtNewMedName = new TextBox { Top = 15, Left = 180, Width = 220 };

            Label lblStoredQty = new Label { Text = "Кількість надходження:", Top = 55, Left = 15, Width = 170 };
            txtNewMedQty = new TextBox { Top = 55, Left = 180, Width = 220 };

            btnAddStorage = new Button { Text = "Оприбуткувати партію ліків", Top = 13, Left = 420, Width = 230, Height = 32, BackColor = Color.LightGreen };
            btnAddStorage.Click += BtnPutStorage_Click;

            btnRemoveStorage = new Button { Text = "Вилучити препарат з бази", Top = 51, Left = 420, Width = 230, Height = 32, BackColor = Color.LightCoral };
            btnRemoveStorage.Click += BtnRemoveStorage_Click;

            // Елементи управління взаємозамінністю (Аналоги ліків)
            Label lblSubTitle = new Label { Text = "Керування взаємозамінністю лікарських засобів:", Top = 120, Left = 15, Width = 400, Font = new Font(this.Font, FontStyle.Bold) };

            txtSubstituteName = new TextBox { Top = 155, Left = 15, Width = 385, Text = "Введіть назву аналога для обраного препарату...", ForeColor = Color.Gray };
            txtSubstituteName.Enter += (s, e) => { if (txtSubstituteName.Text.StartsWith("Введіть назву")) { txtSubstituteName.Text = ""; txtSubstituteName.ForeColor = Color.Black; } };
            txtSubstituteName.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSubstituteName.Text)) { txtSubstituteName.Text = "Введіть назву аналога для обраного препарату..."; txtSubstituteName.ForeColor = Color.Gray; } };

            btnLinkSubstitute = new Button { Text = "Додати аналог до бази", Top = 153, Left = 420, Width = 230, Height = 32 };
            btnLinkSubstitute.Click += BtnLinkSubstitute_Click;

            // Збір елементів управління на панель форм
            pnlStorageForms.Controls.AddRange(new Control[] {
                lblStoredName, txtNewMedName, lblStoredQty, txtNewMedQty,
                btnAddStorage, btnRemoveStorage, lblSubTitle, txtSubstituteName, btnLinkSubstitute
            });

            // Додавання всіх сконструйованих блоків на сторінку вкладки Складу
            pageStorage.Controls.Add(gridStorage);
            pageStorage.Controls.Add(pnlStorageForms);
        }

        #endregion

        #region КЛЮЧОВА БИЗНЕС-ЛОГІКА ТА ОБРОБКА ПОДІЙ

        /// <summary>
        /// Синхронізує та оновлює списки та таблиці на формі актуальними даними з репозиторію.
        /// </summary>
        private void UpdateUserInterfaceData()
        {
            // Оновлення списку назв хвороб ліворуч із збереженням поточного вибору
            string currentSelection = listDiseases.SelectedItem?.ToString();
            listDiseases.Items.Clear();
            foreach (var d in _repository.Diseases)
            {
                listDiseases.Items.Add(d.Title);
            }
            if (!string.IsNullOrEmpty(currentSelection))
            {
                listDiseases.SelectedItem = currentSelection;
            }

            // Проекція списку об'єктів Medicine в анонімні типи для красивого відображення в DataGridView
            gridStorage.DataSource = null;
            gridStorage.DataSource = _repository.Storage.Select(m => new {
                Препарат = m.Name,
                Наявна_Кількість = m.Quantity,
                Допустимі_Аналоги = string.Join(", ", m.Substitutes)
            }).ToList();
        }

        /// <summary>
        /// Спрацьовує при виборі захворювання зі списку. Заповнює текстові поля картки хвороби.
        /// </summary>
        private void ListDiseases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listDiseases.SelectedItem == null) return;
            string title = listDiseases.SelectedItem.ToString();
            var disease = _repository.Diseases.FirstOrDefault(d => d.Title == title);

            if (disease != null)
            {
                txtTitle.Text = disease.Title;
                txtProcedures.Text = disease.Procedures;
                txtSymptoms.Text = string.Join(", ", disease.Symptoms);

                // Очищення та заповнення таблиці рекомендованих препаратів
                gridMedicines.Rows.Clear();
                foreach (var item in disease.RecommendedMedicines)
                {
                    gridMedicines.Rows.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Фільтрує список захворювань у реальному часі на основі введеного симптому.
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query) || query == "Введіть симптом для пошуку...")
            {
                UpdateUserInterfaceData();
                return;
            }

            var results = _repository.Diseases
                .Where(d => d.Symptoms.Any(s => s.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(d => d.Title)
                .ToArray();

            listDiseases.Items.Clear();
            listDiseases.Items.AddRange(results);
        }

        /// <summary>
        /// Зберігає нову або оновлює існуючу картку хвороби, зчитуючи дані з форми.
        /// </summary>
        private void BtnSaveDisease_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string procedures = txtProcedures.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Помилка реєстрації: назва хвороби не може бути порожньою.", "Контроль даних", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var disease = _repository.Diseases.FirstOrDefault(d => d.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            bool isInserted = false;

            if (disease == null)
            {
                disease = new Disease(title, procedures);
                isInserted = true;
            }
            else
            {
                disease.Procedures = procedures;
            }

            disease.Symptoms = txtSymptoms.Text.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            disease.RecommendedMedicines.Clear();
            foreach (DataGridViewRow row in gridMedicines.Rows)
            {
                if (row.Cells[0].Value == null || row.Cells[1].Value == null) continue;

                string medName = row.Cells[0].Value.ToString().Trim();
                if (int.TryParse(row.Cells[1].Value.ToString(), out int qty) && qty > 0)
                {
                    disease.RecommendedMedicines[medName] = qty;
                }
            }

            if (isInserted) _repository.Diseases.Add(disease);

            _repository.SaveData();
            UpdateUserInterfaceData();
            MessageBox.Show("Картку захворювання успішно збережено у медичному реєстрі.", "Сповіщення системи", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Видаляє обрану картку захворювання з медичного реєстру з попереднім підтвердженням.
        /// </summary>
        private void BtnRemoveDisease_Click(object sender, EventArgs e)
        {
            if (listDiseases.SelectedItem == null) return;
            string title = listDiseases.SelectedItem.ToString();

            var question = MessageBox.Show($"Ви впевнені, що бажаєте безповоротно видалити картку '{title}'?", "Підтвердження видалення запису", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (question == DialogResult.Yes)
            {
                var target = _repository.Diseases.FirstOrDefault(d => d.Title == title);
                if (target != null)
                {
                    _repository.Diseases.Remove(target);
                    _repository.SaveData();
                    UpdateUserInterfaceData();
                    ResetDoctorWorkspace();
                }
            }
        }

        /// <summary>
        /// ГОЛОВНА ФУНКЦІОНАЛЬНА ВИМОГА: Формування рецепту пацієнту із автоматичним списанням ліків або підбором аналогів.
        /// </summary>
        private void BtnCreateRecipe_Click(object sender, EventArgs e)
        {
            if (listDiseases.SelectedItem == null)
            {
                MessageBox.Show("Оберіть діагноз пацієнта зі списку ліворуч.", "Помилка формування бланка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            string title = listDiseases.SelectedItem.ToString();
            var disease = _repository.Diseases.First(d => d.Title == title);

            string officialBlank = $"========================================\n" +
                                   $"          ОФІЦІЙНИЙ РЕЦЕПТУРНИЙ БЛАНК    \n" +
                                   $"========================================\n" +
                                   $"Встановлений діагноз: {disease.Title}\n" +
                                   $"Призначені процедури: {disease.Procedures}\n" +
                                   $"----------------------------------------\n" +
                                   $"Схема медикаментозного лікування:\n";

            List<string> deficitAlerts = new List<string>();

            foreach (var element in disease.RecommendedMedicines)
            {
                // ВИПРАВЛЕННЯ БАГУ: Очищаємо назву ліків від випадкових пробілів на початку/в кінці рядка
                string targetMed = element.Key.Trim();
                int targetQty = element.Value;

                // Надійний пошук з ігноруванням регістру та зайвих пробілів у базі складу
                var stockItem = _repository.Storage.FirstOrDefault(m => m.Name.Trim().Equals(targetMed, StringComparison.OrdinalIgnoreCase));

                if (stockItem != null && stockItem.Quantity >= targetQty)
                {
                    stockItem.Quantity -= targetQty;
                    officialBlank += $"- {stockItem.Name} : {targetQty} од. [Списано зі складу лікарні]\n";
                }
                else
                {
                    deficitAlerts.Add($"Препарат '{targetMed}' відсутній на складі лікарні (необхідно: {targetQty} од.)!");

                    string substituteText = (stockItem != null && stockItem.Substitutes.Count > 0)
                        ? string.Join(", ", stockItem.Substitutes)
                        : "допустимі лікарями аналоги не занесені до бази";

                    officialBlank += $"- {targetMed} : {targetQty} од. [БРАК НА СКЛАДІ] -> Рекомендовані замінники: {substituteText}\n";
                }
            }

            officialBlank += "========================================\n";

            _repository.SaveData();
            UpdateUserInterfaceData();

            if (deficitAlerts.Count > 0)
            {
                string alertText = string.Join("\n", deficitAlerts);
                MessageBox.Show($"[УВАГА: ДЕФІЦИТ LІКІВ НА СКЛАДІ]\n\n{alertText}", "Повідомлення для лікуючого лікаря", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            MessageBox.Show(officialBlank, "Рецептурний blank", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Обробляє оприбуткування нової партії ліків на склад (або збільшення існуючих залишків).
        /// </summary>
        private void BtnPutStorage_Click(object sender, EventArgs e)
        {
            string name = txtNewMedName.Text.Trim();

            if (string.IsNullOrEmpty(name) || !int.TryParse(txtNewMedQty.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Помилка введення. Вкажіть текстову назву ліків та цілу додатну кількість.", "Контроль помилок", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var item = _repository.Storage.FirstOrDefault(m => m.Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Quantity += qty;
            }
            else
            {
                _repository.Storage.Add(new Medicine(name, qty));
            }

            _repository.SaveData();
            UpdateUserInterfaceData();
            txtNewMedName.Clear();
            txtNewMedQty.Clear();
            MessageBox.Show("Складські залишки успішно збільшено.", "Облік складу", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Повністю видаляє вибрану позицію медикаменту із бази даних складу лікарні.
        /// </summary>
        private void BtnRemoveStorage_Click(object sender, EventArgs e)
        {
            if (gridStorage.CurrentRow == null)
            {
                MessageBox.Show("Оберіть препарат у таблиці для його видалення.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string targetMedName = gridStorage.CurrentRow.Cells["Препарат"].Value.ToString();

            var confirm = MessageBox.Show($"Ви впевнені, що хочете видалити препарат '{targetMedName}' та всі його аналоги зі складу бази даних?",
                "Підтвердження вилучення ліків", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                var target = _repository.Storage.FirstOrDefault(m => m.Name == targetMedName);
                if (target != null)
                {
                    _repository.Storage.Remove(target);
                    _repository.SaveData();
                    UpdateUserInterfaceData();
                    MessageBox.Show($"Препарат '{targetMedName}' успішно видалено.", "Облік складу", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Зв'язує новий аналог (замінник) із вибраним у таблиці медикаментом.
        /// </summary>
        private void BtnLinkSubstitute_Click(object sender, EventArgs e)
        {
            if (gridStorage.CurrentRow == null)
            {
                MessageBox.Show("Оберіть ліки у верхній таблиці складу для зв'язування.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string targetMedName = gridStorage.CurrentRow.Cells["Препарат"].Value.ToString();
            string subName = txtSubstituteName.Text.Trim();

            if (string.IsNullOrEmpty(subName) || subName.StartsWith("Введіть назву"))
            {
                MessageBox.Show("Будь ласка, вкажіть назву препарату-аналога.", "Валідація", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var medicine = _repository.Storage.First(m => m.Name == targetMedName);

            if (!medicine.Substitutes.Contains(subName, StringComparer.OrdinalIgnoreCase))
            {
                medicine.Substitutes.Add(subName);
                _repository.SaveData();
                UpdateUserInterfaceData();
                txtSubstituteName.Clear();
                MessageBox.Show($"Препарат '{subName}' тепер зареєстрований як аналог для '{targetMedName}'.", "Зв'язок встановлено", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Допоміжний метод очищення робочих полів форми лікаря для створення нової картки.
        /// </summary>
        private void ResetDoctorWorkspace()
        {
            txtTitle.Clear();
            txtProcedures.Clear();
            txtSymptoms.Clear();
            gridMedicines.Rows.Clear();
        }

        #endregion
    }
}