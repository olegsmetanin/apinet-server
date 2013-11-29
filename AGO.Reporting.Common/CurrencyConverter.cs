/*
 * Копирайты создателя этого кода на Pascal
 * ////////////////////////////////////////////////////
 * //                                                //
 * //   AcedCommon 1.12                              //
 * //                                                //
 * //   Константы и функции различного назначения.   //
 * //                                                //
 * //   mailto: acedutils@yandex.ru                  //
 * //                                                //
 * ////////////////////////////////////////////////////
 * Просто переведено на C#. Крайне желателен рефакторинг - слишком сложно для восприятия.
 */

using System;
using System.Text;

namespace AGO.Reporting.Common
{
    /// <summary>
    /// Формат названия триад
    /// </summary>
    [Flags]
    public enum GrammaticalTriadForm
    {
        /// <summary>
        /// Полный формат: тысяч, миллионов и т.д.
        /// </summary>
        Full = 0,
        /// <summary>
        /// Краткий формат: тыс., млн. и т.д.
        /// </summary>
        Short = 4
    }

    /// <summary>
    /// Род, в котором необходимо выдавать число прописью
    /// </summary>
    public enum GrammaticalGender
    {
        Male = 0,
        Female = 1, 
        Middle = 2
    }

    /// <summary>
    /// Функция G_NumToStr возвращает номер формы, в которой должно стоять 
    /// следующее за данным числом слово, т.е. одно из следующих значений:
    /// </summary>
    public enum GrammaticalForm
    {
        /// <summary>
        /// Первая форма: "один слон" или "двадцать одна кошка"
        /// </summary>
        First = 1,
        /// <summary>
        /// Вторая форма: "три слона" или "четыре кошки"
        /// </summary>
        Second = 2,
        /// <summary>
        /// Третья форма: "шесть слонов" или "восемь кошек"
        /// </summary>
        Third = 3
    }

    /// <summary>
    ///  CurrencyToWords возвращает денежную сумму прописью. В параметре V передается
    ///  численное значение денежной суммы в рублях. Сотые доли выражают копейки.
    ///  Параметры RubFormat и CopFormat определяют формат записи, соответственно,
    ///  рублей и копеек. Возможные значения для форматов:
    /// </summary>
    [Flags]
    public enum WordsFormat
    {
        /// <summary>
        /// Полный числовой формат: "342 рубля" или "25 копеек"
        /// </summary>
        NumFull = 0,
        /// <summary>
        /// Полный строчный формат: "Один рубль" или "две копейки"
        /// </summary>
        Full = 2,
        /// <summary>
        /// Краткий числовой формат: "475084 руб." или "15 коп."
        /// </summary>
        NumShort = 1,
        /// <summary>
        /// Краткий строчный формат: "Пять руб." или "десять коп."
        /// </summary>
        Short = 3,
        /// <summary>
        /// Краткая запись названий триад: тыс., млн., ...
        /// </summary>
        ShortTriad = 4,
        /// <summary>
        /// Нет рублей, нет копеек или простая числовая запись
        /// </summary>
        None = 8
    }

    public class CurrencyConverter
    {
        private static readonly string[] M_Ed = {"один ","два ","три ","четыре ","пять ","шесть ","семь ","восемь ","девять "};
        private static readonly string[] W_Ed = {"одна ","две ","три ","четыре ","пять ","шесть ","семь ","восемь ","девять "};
        private static readonly string[] G_Ed = {"одно ","два ","три ","четыре ","пять ","шесть ","семь ","восемь ","девять "};
        private static readonly string[] E_Ds = {"десять ","одиннадцать ","двенадцать ","тринадцать ","четырнадцать ", "пятнадцать ","шестнадцать ","семнадцать ","восемнадцать ","девятнадцать "};
        private static readonly string[] D_Ds = {"двадцать ","тридцать ","сорок ","пятьдесят ","шестьдесят ","семьдесят ", "восемьдесят ","девяносто "};
        private static readonly string[] U_Hd = {"сто ","двести ","триста ","четыреста ","пятьсот ","шестьсот ","семьсот ", "восемьсот ","девятьсот "};

        private static readonly string[,] M_Tr = {
                                                     {"тыс. ", "тысяча ", "тысячи ", "тысяч "},
                                                     {"млн. ", "миллион ", "миллиона ", "миллионов "},
                                                     {"млрд. ", "миллиард ", "миллиарда ", "миллиардов "},
                                                     {"трлн. ", "триллион ", "триллиона ", "триллионов "},
                                                     {"квадр. ", "квадриллион ", "квадриллиона ", "квадриллионов "},
                                                     {"квинт. ", "квинтиллион ", "квинтиллиона ", "квинтиллионов "}
                                                 };

        private static int ModDiv10(ref int value)
        {
            int result = value%10;
            value = value/10;
            return result;
        }

        public static GrammaticalForm NumberToString(long value, out string strValue, GrammaticalTriadForm triadForm, GrammaticalGender gender)
        {
            long V1;
            int[] VArr = new int[7];
            int Count;
            StringBuilder sb;

            GrammaticalForm result = GrammaticalForm.Third;
            strValue = string.Empty;

            if (value == 0)
            {
                strValue = "ноль ";
                return result;
            }

            if (value > 0)
            {
                sb = new StringBuilder(120);
            }
            else if (value != long.MinValue)
            {
                value = Math.Abs(value);
                sb = new StringBuilder("минус ");
            }
            else
            {
                
                switch(triadForm)
                {
                    case GrammaticalTriadForm.Full:
                        strValue =
                            "минус девять квинтиллионов двести двадцать три квадриллиона триста семьдесят два триллиона тридцать шесть миллиардов восемьсот пятьдесят четыре миллиона семьсот семьдесят пять тысяч восемьсот восемь ";
                        break;
                    case GrammaticalTriadForm.Short:
                        strValue =
                            "минус девять квинт. двести двадцать три квадр. триста семьдесят два трлн. тридцать шесть млрд. восемьсот пятьдесят четыре млн. семьсот семьдесят пять тыс. восемьсот восемь ";
                        break;
                };
                return result;
            }

            Count = 0;
            do
            {
                V1 = value / 1000;
                VArr[Count] = Convert.ToInt32(value - (V1*1000));
                value = V1;
                Count++;
            } while (V1 != 0);
            for(int i = Count - 1; i >= 0; i--)
            {
                int H = VArr[i];
                result = GrammaticalForm.Third;
                if (H != 0)
                {
                    int E = ModDiv10(ref H);
                    int D = ModDiv10(ref H);
                    if (D != 1)
                    {
                        if (E == 1)
                        {
                            result = GrammaticalForm.First;
                        }
                        else if (2 <= E && E <= 4)
                        {
                            result = GrammaticalForm.Second;
                        }

                        if (H != 0 && D != 0)
                        {
                            sb.Append(U_Hd[H - 1]).Append(D_Ds[D - 2]);
                        }
                        else if (H != 0)
                        {
                            sb.Append(U_Hd[H - 1]);
                        }
                        else if (D != 0)
                        {
                            sb.Append(D_Ds[D - 2]);
                        }

                        if (E != 0)
                        {
                            if (i == 0)
                            {
                                switch(gender)
                                {
                                    case GrammaticalGender.Male:
                                        sb.Append(M_Ed[E - 1]);
                                        break;
                                    case GrammaticalGender.Female:
                                        sb.Append(W_Ed[E - 1]);
                                        break;
                                    case GrammaticalGender.Middle:
                                        sb.Append(G_Ed[E - 1]);
                                        break;
                                    default:
                                        sb.Append("#### ");
                                        break;
                                }
                            }
                            else if (i == 1)
                            {
                                sb.Append(W_Ed[E - 1]);
                            }
                            else
                            {
                                sb.Append(M_Ed[E - 1]);
                            }
                        }
                    }
                    else
                    {
                        if (H == 0)
                        {
                            sb.Append(E_Ds[E]);
                        }
                        else
                        {
                            sb.Append(U_Hd[H - 1]).Append(E_Ds[E]);
                        }
                    }
                    if (i != 0)
                    {
                        switch(triadForm)
                        {
                            case GrammaticalTriadForm.Full:
                                sb.Append(M_Tr[i - 1, (int) result]);
                                break;
                            default:
                                sb.Append(M_Tr[i - 1, 0]);
                                break;
                        }
                    }
                }
            }
            strValue = sb.ToString();
            return result;
        }


        public static string CurrencyToWords(double value)
        {
            return CurrencyToWords(value, WordsFormat.Full, WordsFormat.NumShort);
        }

        public static string CurrencyToWords(decimal value)
        {
            return CurrencyToWords(Convert.ToDouble(value), WordsFormat.Full, WordsFormat.NumShort);
        }

        public static string CurrencyToWords(decimal value, WordsFormat rubFormat, WordsFormat copFormat)
        {
            return CurrencyToWords(Convert.ToDouble(value), rubFormat, copFormat);
        }

        public static string CurrencyToWords(double value, WordsFormat rubFormat, WordsFormat copFormat)
        {
            long V1;
            string S1, S2;
            S1 = S2 = string.Empty;
            int I;
            bool Negative;

            string result;

            //Работаем только с положительными числами. Отрицательные выводим в скобках
            if (value >= 0)
            {
                Negative = false;
            }
            else
            {
                Negative = true;
                value = Math.Abs(value);
            }

            if (rubFormat != WordsFormat.None)
            {
                if (copFormat != WordsFormat.None)
                {
                    V1 = Convert.ToInt64(Math.Truncate(value));
                    int Cp = Convert.ToInt32(Math.Round((value - Math.Truncate(value))*100));
                    if (V1 != 0)
                    {
                        if ((rubFormat & WordsFormat.NumShort) == 0)
                        {
                            if ((rubFormat & WordsFormat.Full) != 0)
                            {
                                switch(NumberToString(V1, out S1, (GrammaticalTriadForm)(rubFormat & WordsFormat.ShortTriad), GrammaticalGender.Male))
                                {
                                    case GrammaticalForm.First:
                                        S2 = "рубль ";
                                        break;
                                    case GrammaticalForm.Second:
                                        S2 = "рубля ";
                                        break;
                                    case GrammaticalForm.Third:
                                        S2 = "рублей ";
                                        break;
                                }
                            }
                            else
                            {
                                S1 = V1.ToString();
                                I = Convert.ToInt32(V1/100);
                                if (10 < I && I < 20)
                                {
                                    switch(I/10)
                                    {
                                        case 1:
                                            S2 = " рубль ";
                                            break;
                                        case 2:
                                        case 3:
                                        case 4:
                                            S2 = " рубля ";
                                            break;
                                        default:
                                            S2 = " рублей ";
                                            break;
                                    }
                                }
                                else
                                {
                                    S2 = " рублей ";
                                }
                            }
                        }
                        else if ((rubFormat & WordsFormat.Full) != 0)
                        {
                            NumberToString(V1, out S1, 
                                           (GrammaticalTriadForm) (rubFormat & WordsFormat.ShortTriad),
                                           GrammaticalGender.Male);
                            S2 = "руб. ";
                        }
                        else
                        {
                            S1 = V1.ToString();
                            S2 = " руб. ";
                        }
                    }
                    else
                    {
                        S1 = string.Empty;
                        S2 = string.Empty;
                    }

                    string S3, S4;
                    S3 = S4 = string.Empty;
                    if ((copFormat & WordsFormat.NumShort) == 0)
                    {
                        if ((copFormat & WordsFormat.Full) != 0)
                        {
                            switch(NumberToString(Cp, out S3, (GrammaticalTriadForm)(copFormat & WordsFormat.ShortTriad), GrammaticalGender.Female))
                            {
                                case GrammaticalForm.First:
                                    S4 = "копейка";
                                    break;
                                case GrammaticalForm.Second:
                                    S4 = "копейки";
                                    break;
                                case GrammaticalForm.Third:
                                    S4 = "копеек";
                                    break;
                            }
                        }
                        else
                        {
                            S3 = string.Format("{0:D2}", Cp);
                            I = Cp/100;
                            if (10 < I && I < 20)
                            {
                                switch(I/10)
                                {
                                    case 1:
                                        S4 = " копейка";
                                        break;
                                    case 2:
                                    case 3:
                                    case 4:
                                        S4 = " копейки";
                                        break;
                                    default:
                                        S4 = " копеек";
                                        break;
                                }
                            }
                            else
                            {
                                S4 = " копеек";
                            }
                        }
                    }
                    else if ((copFormat & WordsFormat.Full) != 0)
                    {
                        NumberToString(Cp, out S3,
                                       (GrammaticalTriadForm) (copFormat & WordsFormat.ShortTriad),
                                       GrammaticalGender.Female);
                        S4 = "коп.";
                    }
                    else
                    {
                        S3 = string.Format("{0:D2}", Cp);
                        S4 = " коп.";
                    }
                    S1 = FirstLetterUpper(S1);
                    if (!Negative)
                    {
                        result = S1 + S2 + S3 + S4;
                    }
                    else
                    {
                        result = "(" + S1 + S2 + S3 + S4 + ")";
                    }
                }
                else
                {
                    V1 = Convert.ToInt64(Math.Round(value));
                    if (V1 != 0)
                    {
                        if ((rubFormat & WordsFormat.NumShort) == 0)
                        {
                            if ((rubFormat & WordsFormat.Full) != 0)
                            {
                                switch(NumberToString(V1, out S1, (GrammaticalTriadForm)(rubFormat & WordsFormat.ShortTriad), GrammaticalGender.Male))
                                {
                                    case GrammaticalForm.First:
                                        S2 = "рубль";
                                        break;
                                    case GrammaticalForm.Second:
                                        S2 = "рубля";
                                        break;
                                    case GrammaticalForm.Third:
                                        S2 = "рублей";
                                        break;
                                }
                            }
                            else
                            {
                                S1 = V1.ToString();
                                I = Convert.ToInt32(V1/100);
                                if (10 < I && I < 20)
                                {
                                    switch(I/10)
                                    {
                                        case 1:
                                            S2 = " рубль";
                                            break;
                                        case 2:
                                        case 3:
                                        case 4:
                                            S2 = " рубля";
                                            break;
                                        default:
                                            S2 = " рублей";
                                            break;
                                    }
                                }
                                else
                                {
                                    S2 = " рублей";
                                }
                            }
                        }
                        else if ((rubFormat & WordsFormat.Full) != 0)
                        {
                            NumberToString(V1, out S1, (GrammaticalTriadForm) (rubFormat & WordsFormat.ShortTriad),
                                           GrammaticalGender.Male);
                            S2 = "руб.";
                        }
                        else
                        {
                            S1 = V1.ToString();
                            S2 = " руб.";
                        }
                        S1 = FirstLetterUpper(S1);
                        if (!Negative)
                        {
                            result = S1 + S2;
                        }
                        else
                        {
                            result = "(" + S1 + S2 + ")";
                        }
                    }
                    else
                    {
                        result = string.Empty;
                    }
                }
            }
            else if ((copFormat & WordsFormat.None) != 0)
            {
                V1 = Convert.ToInt64(Math.Round(value*100));
                if ((copFormat & WordsFormat.NumShort) == 0)
                {
                    if ((copFormat & WordsFormat.Full) != 0)
                    {
                        switch(NumberToString(V1, out S1, (GrammaticalTriadForm)(copFormat & WordsFormat.ShortTriad), GrammaticalGender.Female))
                        {
                            case GrammaticalForm.First:
                                S2 = "копейка";
                                break;
                            case GrammaticalForm.Second:
                                S2 = "копейки";
                                break;
                            case GrammaticalForm.Third:
                                S2 = "копеек";
                                break;
                        }
                    }
                    else
                    {
                        S1 = V1.ToString();
                        I = Convert.ToInt32(V1/100);
                        if (10 < I && I < 20)
                        {
                            switch(I/10)
                            {
                                case 1:
                                    S2 = " копейка";
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    S2 = " копейки";
                                    break;
                                default:
                                    S2 = " копеек";
                                    break;
                            }
                        }
                        else
                        {
                            S2 = " копеек";
                        }
                    }
                }
                else if ((copFormat & WordsFormat.Full) != 0)
                {
                    NumberToString(V1, out S1, 
                                   (GrammaticalTriadForm) (copFormat & WordsFormat.ShortTriad),
                                   GrammaticalGender.Female);
                    S2 = "коп.";
                }
                else
                {
                    S1 = V1.ToString();
                    S2 = " коп.";
                }
                S1 = FirstLetterUpper(S1);
                if (!Negative)
                {
                    result = S1 + S2;
                }
                else
                {
                    result = "(" + S1 + S2 + ")";
                }
            }
            else
            {
                if (!Negative)
                {
                    result = value.ToString("C");
                }
                else
                {
                    result = value.ToString("({0:C})");
                }
            }

            return result;
        }

        private static string FirstLetterUpper(string s)
        {
            switch(s.Length)
            {
                case 0: return s;
                case 1: return s.ToUpper();
                default:
                    return new string(new char[] { s[0] }).ToUpper() + s.Substring(1, s.Length - 1);
            }
        }
    }
}