using System;
using System.Collections.Generic;
using System.Text;

namespace AutoComplete
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fullNames = new List<FullName>
            {
              new FullName {Name = "Иван", Surname = "Двитриев1", Patronymic = "Сергеевич"},
              new FullName {Name = "Иван", Surname = null, Patronymic = "Сергеевич"},
              //new FullName {Name = "Семен", Surname = "Семенов", Patronymic = "Сергеевич"},
              new FullName {Name = "Иван", Surname = "Двитриев", Patronymic = "Сергеевич"},
              new FullName {Name = "Иван", Surname = "Семенов", Patronymic = "Сергеевич"},
              new FullName {Name = null, Surname = "Семенов", Patronymic = null}
            };

            var autoCompleter = new AutoCompleter();
            autoCompleter.AddToSearch(fullNames);

            fullNames = new List<FullName>
            {
              new FullName {Name = "Иван", Surname = "Двитриев1", Patronymic = "Сергеевич"},
              new FullName {Name = "Георгий", Surname = "Семенов", Patronymic = "Сергеевич"},
              new FullName {Name = "Георгий", Surname = "Семенов", Patronymic = null},
              new FullName {Name = "Геннадий", Surname = "Семенов", Patronymic = null},
              new FullName {Name = null, Surname = "Сенов", Patronymic = null},
              new FullName {Name = null, Surname = "Сумка", Patronymic = null}

            };
            autoCompleter.AddToSearch(fullNames);
            //var fullNames = new List<FullName>()
            //{
            //    new FullName()
            //    {
            //        Name = null,
            //        Surname = "Сюткин",
            //        Patronymic = null
            //    },
            //    new FullName()
            //    {
            //        Name = null,
            //        Surname = "Сютов",
            //        Patronymic = null
            //    },
            //    new FullName()
            //    {
            //        Name = null,
            //        Surname = "Салкин",
            //        Patronymic = null
            //    },
            //    new FullName()
            //    {
            //        Name = null,
            //        Surname = "Светлов",
            //        Patronymic = null
            //    }
            //};
            //autoCompleter.AddToSearch(fullNames);

            //foreach (var val in autoCompleter.Search("Салк"))
            //{
            //    Console.WriteLine(val);
            //}
            //Console.WriteLine("-------------------------------------------");

            foreach (var val in autoCompleter.Search("С"))
            {
                Console.WriteLine(val);
            }

        }
    }
}
