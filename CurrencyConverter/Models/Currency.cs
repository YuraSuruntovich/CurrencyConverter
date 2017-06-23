using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class Currency
    {
        public int Cur_Id { get; set; }  
        public string Cur_Name { get; set; }
        public string Cur_Abbreviation { get; set; }
        public string ImagePath { get; set; } // путь к изображению
    }
}
