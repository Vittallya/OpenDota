using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class example
    {
        //Авто-Свойство
        public int Number { get; }
        //"под капотом" среда создает у себя поле для этого авто-свойства

        //Обычное свойство
        public int Number2 
        {
            get
            {
                return number;
            }
        }

        public int this[string str]
        {
            get
            {
                return 3424;
            }
        }

        public int get(string str)
        {
            return 3234;
        }

        public example()
        {
            //с доступом get и только с ним, можно редактировать только авто-свойства и только в конструкторе
            Number = 3234;
        }

        //Поле
        private int number;


        void Choto()
        {
            Number = 32434;
        }
    }

    public class example2: example
    {
        public example2()
        {
            Number = 432;
        }
    }
}