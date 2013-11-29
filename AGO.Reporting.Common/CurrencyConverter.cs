/*
 * ��������� ��������� ����� ���� �� Pascal
 * ////////////////////////////////////////////////////
 * //                                                //
 * //   AcedCommon 1.12                              //
 * //                                                //
 * //   ��������� � ������� ���������� ����������.   //
 * //                                                //
 * //   mailto: acedutils@yandex.ru                  //
 * //                                                //
 * ////////////////////////////////////////////////////
 * ������ ���������� �� C#. ������ ��������� ����������� - ������� ������ ��� ����������.
 */

using System;
using System.Text;

namespace AGO.Reporting.Common
{
    /// <summary>
    /// ������ �������� �����
    /// </summary>
    [Flags]
    public enum GrammaticalTriadForm
    {
        /// <summary>
        /// ������ ������: �����, ��������� � �.�.
        /// </summary>
        Full = 0,
        /// <summary>
        /// ������� ������: ���., ���. � �.�.
        /// </summary>
        Short = 4
    }

    /// <summary>
    /// ���, � ������� ���������� �������� ����� ��������
    /// </summary>
    public enum GrammaticalGender
    {
        Male = 0,
        Female = 1, 
        Middle = 2
    }

    /// <summary>
    /// ������� G_NumToStr ���������� ����� �����, � ������� ������ ������ 
    /// ��������� �� ������ ������ �����, �.�. ���� �� ��������� ��������:
    /// </summary>
    public enum GrammaticalForm
    {
        /// <summary>
        /// ������ �����: "���� ����" ��� "�������� ���� �����"
        /// </summary>
        First = 1,
        /// <summary>
        /// ������ �����: "��� �����" ��� "������ �����"
        /// </summary>
        Second = 2,
        /// <summary>
        /// ������ �����: "����� ������" ��� "������ �����"
        /// </summary>
        Third = 3
    }

    /// <summary>
    ///  CurrencyToWords ���������� �������� ����� ��������. � ��������� V ����������
    ///  ��������� �������� �������� ����� � ������. ����� ���� �������� �������.
    ///  ��������� RubFormat � CopFormat ���������� ������ ������, ��������������,
    ///  ������ � ������. ��������� �������� ��� ��������:
    /// </summary>
    [Flags]
    public enum WordsFormat
    {
        /// <summary>
        /// ������ �������� ������: "342 �����" ��� "25 ������"
        /// </summary>
        NumFull = 0,
        /// <summary>
        /// ������ �������� ������: "���� �����" ��� "��� �������"
        /// </summary>
        Full = 2,
        /// <summary>
        /// ������� �������� ������: "475084 ���." ��� "15 ���."
        /// </summary>
        NumShort = 1,
        /// <summary>
        /// ������� �������� ������: "���� ���." ��� "������ ���."
        /// </summary>
        Short = 3,
        /// <summary>
        /// ������� ������ �������� �����: ���., ���., ...
        /// </summary>
        ShortTriad = 4,
        /// <summary>
        /// ��� ������, ��� ������ ��� ������� �������� ������
        /// </summary>
        None = 8
    }

    public class CurrencyConverter
    {
        private static readonly string[] M_Ed = {"���� ","��� ","��� ","������ ","���� ","����� ","���� ","������ ","������ "};
        private static readonly string[] W_Ed = {"���� ","��� ","��� ","������ ","���� ","����� ","���� ","������ ","������ "};
        private static readonly string[] G_Ed = {"���� ","��� ","��� ","������ ","���� ","����� ","���� ","������ ","������ "};
        private static readonly string[] E_Ds = {"������ ","����������� ","���������� ","���������� ","������������ ", "���������� ","����������� ","���������� ","������������ ","������������ "};
        private static readonly string[] D_Ds = {"�������� ","�������� ","����� ","��������� ","���������� ","��������� ", "����������� ","��������� "};
        private static readonly string[] U_Hd = {"��� ","������ ","������ ","��������� ","������� ","�������� ","������� ", "��������� ","��������� "};

        private static readonly string[,] M_Tr = {
                                                     {"���. ", "������ ", "������ ", "����� "},
                                                     {"���. ", "������� ", "�������� ", "��������� "},
                                                     {"����. ", "�������� ", "��������� ", "���������� "},
                                                     {"����. ", "�������� ", "��������� ", "���������� "},
                                                     {"�����. ", "����������� ", "������������ ", "������������� "},
                                                     {"�����. ", "����������� ", "������������ ", "������������� "}
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
                strValue = "���� ";
                return result;
            }

            if (value > 0)
            {
                sb = new StringBuilder(120);
            }
            else if (value != long.MinValue)
            {
                value = Math.Abs(value);
                sb = new StringBuilder("����� ");
            }
            else
            {
                
                switch(triadForm)
                {
                    case GrammaticalTriadForm.Full:
                        strValue =
                            "����� ������ ������������� ������ �������� ��� ������������ ������ ��������� ��� ��������� �������� ����� ���������� ��������� ��������� ������ �������� ������� ��������� ���� ����� ��������� ������ ";
                        break;
                    case GrammaticalTriadForm.Short:
                        strValue =
                            "����� ������ �����. ������ �������� ��� �����. ������ ��������� ��� ����. �������� ����� ����. ��������� ��������� ������ ���. ������� ��������� ���� ���. ��������� ������ ";
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

            //�������� ������ � �������������� �������. ������������� ������� � �������
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
                                        S2 = "����� ";
                                        break;
                                    case GrammaticalForm.Second:
                                        S2 = "����� ";
                                        break;
                                    case GrammaticalForm.Third:
                                        S2 = "������ ";
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
                                            S2 = " ����� ";
                                            break;
                                        case 2:
                                        case 3:
                                        case 4:
                                            S2 = " ����� ";
                                            break;
                                        default:
                                            S2 = " ������ ";
                                            break;
                                    }
                                }
                                else
                                {
                                    S2 = " ������ ";
                                }
                            }
                        }
                        else if ((rubFormat & WordsFormat.Full) != 0)
                        {
                            NumberToString(V1, out S1, 
                                           (GrammaticalTriadForm) (rubFormat & WordsFormat.ShortTriad),
                                           GrammaticalGender.Male);
                            S2 = "���. ";
                        }
                        else
                        {
                            S1 = V1.ToString();
                            S2 = " ���. ";
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
                                    S4 = "�������";
                                    break;
                                case GrammaticalForm.Second:
                                    S4 = "�������";
                                    break;
                                case GrammaticalForm.Third:
                                    S4 = "������";
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
                                        S4 = " �������";
                                        break;
                                    case 2:
                                    case 3:
                                    case 4:
                                        S4 = " �������";
                                        break;
                                    default:
                                        S4 = " ������";
                                        break;
                                }
                            }
                            else
                            {
                                S4 = " ������";
                            }
                        }
                    }
                    else if ((copFormat & WordsFormat.Full) != 0)
                    {
                        NumberToString(Cp, out S3,
                                       (GrammaticalTriadForm) (copFormat & WordsFormat.ShortTriad),
                                       GrammaticalGender.Female);
                        S4 = "���.";
                    }
                    else
                    {
                        S3 = string.Format("{0:D2}", Cp);
                        S4 = " ���.";
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
                                        S2 = "�����";
                                        break;
                                    case GrammaticalForm.Second:
                                        S2 = "�����";
                                        break;
                                    case GrammaticalForm.Third:
                                        S2 = "������";
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
                                            S2 = " �����";
                                            break;
                                        case 2:
                                        case 3:
                                        case 4:
                                            S2 = " �����";
                                            break;
                                        default:
                                            S2 = " ������";
                                            break;
                                    }
                                }
                                else
                                {
                                    S2 = " ������";
                                }
                            }
                        }
                        else if ((rubFormat & WordsFormat.Full) != 0)
                        {
                            NumberToString(V1, out S1, (GrammaticalTriadForm) (rubFormat & WordsFormat.ShortTriad),
                                           GrammaticalGender.Male);
                            S2 = "���.";
                        }
                        else
                        {
                            S1 = V1.ToString();
                            S2 = " ���.";
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
                                S2 = "�������";
                                break;
                            case GrammaticalForm.Second:
                                S2 = "�������";
                                break;
                            case GrammaticalForm.Third:
                                S2 = "������";
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
                                    S2 = " �������";
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    S2 = " �������";
                                    break;
                                default:
                                    S2 = " ������";
                                    break;
                            }
                        }
                        else
                        {
                            S2 = " ������";
                        }
                    }
                }
                else if ((copFormat & WordsFormat.Full) != 0)
                {
                    NumberToString(V1, out S1, 
                                   (GrammaticalTriadForm) (copFormat & WordsFormat.ShortTriad),
                                   GrammaticalGender.Female);
                    S2 = "���.";
                }
                else
                {
                    S1 = V1.ToString();
                    S2 = " ���.";
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