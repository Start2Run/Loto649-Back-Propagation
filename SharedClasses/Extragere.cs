using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedClasses
{
    public class DgvDataFormat
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Numbers { get; set; }
        public string Year { get; set; }
    }

    public class Extragere
    {
        public DgvDataFormat DgvData
        {
            get { return new DgvDataFormat { Id = _id, Date = _data.ToShortDateString(), Numbers = string.Join(" ", _numere), Year=_data.Year.ToString() }; }
        }

        public DateTime Date
        {
            get { return _data; }
        }

        public int[] Numbers
        {
            get { return _numere; }
        }

        public string Key
        {
            get { return _key; }
        }

        public List<string> Keys4
        {
            get
            {
                return HelpFunctions.ListKeysK(Numbers.ToList(), 4);
            }
        }

        public List<string> Keys5
        {
            get
            {
                return HelpFunctions.ListKeysK(Numbers.ToList(), 5);
            }
        }

        public bool Invalid;

        DateTime _data;
        int[] _numere;
        string _key;
        int _id;

        public Extragere(string[] extragere, int numbers, int id)
        {
            if ((extragere.Count() < numbers+1) || (!DateTime.TryParse(extragere[0], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _data)))
            {
                Invalid = true;
                return;
            }
            else
                _numere = new int[numbers];
            for (int i = 1; i < extragere.Count(); i++)
                if (!int.TryParse(extragere[i], out _numere[i - 1]))
                {
                    Invalid = true;
                    return;
                }
            var sortedValues = _numere.OrderBy(v => v).ToList();
            _key = string.Join("", sortedValues);
            _id = id;
        }
    }
}
