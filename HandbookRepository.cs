using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace DoctorsHandbook
{
    /// <summary>
    /// Сутність: Медикамент на складі лікарні.
    /// </summary>
    [Serializable]
    public class Medicine
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public List<string> Substitutes { get; set; } // Список взаємозамінних ліків

        public Medicine(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
            Substitutes = new List<string>();
        }
    }

    /// <summary>
    /// Сутність: Картка хвороби в довіднику.
    /// </summary>
    [Serializable]
    public class Disease
    {
        public string Title { get; set; }
        public List<string> Symptoms { get; set; }
        public string Procedures { get; set; }
        // Список рекомендованих ліків: Назва ліків -> Необхідна кількість
        public Dictionary<string, int> RecommendedMedicines { get; set; }

        public Disease(string title, string procedures)
        {
            Title = title;
            Procedures = procedures;
            Symptoms = new List<string>();
            RecommendedMedicines = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Керуючий репозиторій для збереження стану та обробки бізнес-сценаріїв.
    /// </summary>
    public class HandbookRepository
    {
        private const string DatabaseFile = "handbook_data.bin";
        public List<Disease> Diseases { get; set; } = new List<Disease>();
        public List<Medicine> Storage { get; set; } = new List<Medicine>();

        public void SaveData()
        {
            try
            {
                using (Stream stream = File.Open(DatabaseFile, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, Diseases);
                    formatter.Serialize(stream, Storage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка автоматичного збереження даних: {ex.Message}", "Системний збій", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadData()
        {
            if (!File.Exists(DatabaseFile))
            {
                LoadDefaultDemoData();
                return;
            }

            try
            {
                using (Stream stream = File.Open(DatabaseFile, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Diseases = (List<Disease>)formatter.Deserialize(stream);
                    Storage = (List<Medicine>)formatter.Deserialize(stream);
                }
            }
            catch
            {
                LoadDefaultDemoData(); // Якщо файл пошкоджено — створюємо чисту демо-базу
            }
        }

        private void LoadDefaultDemoData()
        {
            // Створення стартових медикаментів на складі
            var med1 = new Medicine("Парацетамол", 150);
            med1.Substitutes.AddRange(new[] { "Ібупрофен", "Анальгін" });

            var med2 = new Medicine("Амоксицилін", 10); // Свідомо мала кількість для демонстрації браку
            med2.Substitutes.AddRange(new[] { "Азитроміцин", "Цефтріаксон" });

            var med3 = new Medicine("Лоратадин", 60);
            med3.Substitutes.Add("Цетрин");

            Storage.AddRange(new[] { med1, med2, med3 });

            // Створення базових карток хвороб
            var dis1 = new Disease("Грип", "Постільний режим протягом 5 днів, рясне тепле пиття.");
            dis1.Symptoms.AddRange(new[] { "Гарячка", "Кашель", "Слабкість", "Головний біль" });
            dis1.RecommendedMedicines.Add("Парацетамол", 10);
            dis1.RecommendedMedicines.Add("Амоксицилін", 20); // Затребує більше, ніж є на складі!

            var dis2 = new Disease("Гостра Алергія", "Повне усунення контакту з джерелом алергену.");
            dis2.Symptoms.AddRange(new[] { "Виспи", "Свербіж шкіри", "Набряк" });
            dis2.RecommendedMedicines.Add("Лоратадин", 10);

            Diseases.AddRange(new[] { dis1, dis2 });
            SaveData();
        }
    }
}