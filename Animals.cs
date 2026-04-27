using System;
using System.Collections.Generic;
using System.Text;

namespace HW2_Variant25
{
    /// <summary>
    /// Модель справочной таблицы зоопарков
    /// </summary>
    public class Zoo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Zoo(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public Zoo() : this(0, "") { }
        public override string ToString() => $"[{Id}] {Name}";
    }
    /// <summary>
    /// Модель основной таблицы животных
    /// </summary>
    public class Animal
    {
        public int Id { get; set; }
        public int ZooId { get; set; }
        public string Name { get; set; }

        private double _weight;
        /// <summary>
        /// Масса животного в кг
        /// </summary>
        public double Weight
        {
            get => _weight;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Масса не может быть отрицательной");
                _weight = value;
            }
        }
        public Animal(int id, int zooId, string name, double weight)
        {
            Id = id;
            ZooId = zooId;
            Name = name;
            Weight = weight;
        }
        public Animal() : this(0, 0, "", 0) { }
        public override string ToString() =>
            $"[{Id}] {Name}, Зоопарк №{ZooId}, Вес: {Weight} кг";
    }
}
