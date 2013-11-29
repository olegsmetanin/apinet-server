using System;

namespace AGO.Reporting.Common
{
    /// <summary>
    /// Вертикальное выравнивание в ячейке.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// По верхнему краю ячейки
        /// </summary>
        Top,

        /// <summary>
        /// По середине ячейки
        /// </summary>
        Middle,

        /// <summary>
        /// По нижнему краю ячейки
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Горизонтальное выравнивание в ячейке
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// По левому краю ячейки
        /// </summary>
        Left,

        /// <summary>
        /// По центру ячейки
        /// </summary>
        Center,

        /// <summary>
        /// По правому краю ячейки
        /// </summary>
        Right
    }

    /// <summary>
    /// Направление текста в ячейке
    /// </summary>
    public enum TextDirection
    {
        /// <summary>
        /// По горизонтали
        /// </summary>
        Horizontal,

        /// <summary>
        /// По вертикали снизу вверх
        /// </summary>
        VerticalBottomToTop,

        /// <summary>
        /// По вертикали сверху вниз
        /// </summary>
        VerticalTopToBottom
    }

    /// <summary>
    /// Примитивное форматирование, которое передается из генератора данных генератору отчетов.
    /// </summary>
    public class ReportFieldFormat
    {
        public ReportFieldFormat(string source): this()
        {
            if (!string.IsNullOrEmpty(source))
            {
                var parts = source.Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("ha:"))
                    {
                        HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), part.Replace("ha:", string.Empty));
                    } else if (part.StartsWith("tb:"))
                    {
                        Bold = bool.Parse(part.Replace("tb:", string.Empty));
                    } else if (part.StartsWith("pbb:"))
                    {
                        PageBreakBefore = bool.Parse(part.Replace("pbb:", string.Empty));
                    } else if (part.StartsWith("pba:"))
                    {
                        PageBreakAfter = bool.Parse(part.Replace("pba:", string.Empty));
                    }
                }
            }
        }

        public ReportFieldFormat()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Горизонтальное выравнивание поля в параграфе
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Выделение текста (значения поля) жирным
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Вставить разрыв страницы перед параграфом поля
        /// </summary>
        public bool PageBreakBefore { get; set; }

        /// <summary>
        /// Вставить разрыв страницы после параграфа поля
        /// </summary>
        public bool PageBreakAfter { get; set; }

        public override string ToString()
        {
            return string.Format("ha:{0};tb:{1};pbb:{2};pba:{3}", HorizontalAlignment, Bold, PageBreakBefore, PageBreakAfter);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    /// <summary>
    /// Примитивное форматирование, которое передается из генератора данных генератору отчетов.
    /// Для ячеек таблиц.
    /// </summary>
    public class ReportCellFormat
    {
        public ReportCellFormat(string source): this()
        {
            if (!string.IsNullOrEmpty(source))
            {
                var parts = source.Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("ha"))
                        HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), part.Replace("ha:", string.Empty));
                    else if (part.StartsWith("va"))
                        VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), part.Replace("va:", string.Empty));
                    else if (part.StartsWith("td"))
                        TextDirection = (TextDirection)Enum.Parse(typeof(TextDirection), part.Replace("td:", string.Empty));
                    else if (part.StartsWith("tb:"))
                        Bold = bool.Parse(part.Replace("tb:", string.Empty));
                    else if (part.StartsWith("fs"))
                        FontSize = float.Parse(part.Replace("fs", string.Empty));
                }
            }
        }

        public ReportCellFormat()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            TextDirection = TextDirection.Horizontal;
        }

        /// <summary>
        /// Горизонтальное выравнивание
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Вертикальное выравнивание
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Направление текста
        /// </summary>
        public TextDirection TextDirection { get; set; }

        /// <summary>
        /// Выделение текста жирным
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public float FontSize { get; set;}

        public override string ToString()
        {
            return string.Format("ha:{0};va:{1};td:{2};tb:{3};fs{4}", HorizontalAlignment, VerticalAlignment, TextDirection, Bold, FontSize);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    /// <summary>
    /// Форматирование для группы ячеек (т.е. для объединенной ячейки).
    /// </summary>
    public class GroupCellFormat: ReportCellFormat
    {
        public GroupCellFormat()
        {
            Group = string.Empty;
        }

        public GroupCellFormat(string source): base(source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                var parts = source.Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("gn:"))
                    {
                        Group = part.Replace("gn:", string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Имя группы ячеек.
        /// </summary>
        public string Group { get; set; }

        public override string ToString()
        {
            return "gn:" + Group + ";" + base.ToString();
        }
    }

    /// <summary>
    /// Форматирование страницы отчета (секции в Word-документе).
    /// </summary>
    public class ReportPageSetup
    {
        public ReportPageSetup()
        {
            PageStartingNumber = 1;
            RestartPageNumbering = true;
        }

        public ReportPageSetup(string source): this()
        {
            if (!string.IsNullOrEmpty(source))
            {
                var parts = source.Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("psn"))
                    {
                        PageStartingNumber = int.Parse(part.Replace("psn:", string.Empty));
                    } 
                    else if (part.StartsWith("rpn"))
                    {
                        RestartPageNumbering = bool.Parse(part.Replace("rpn:", string.Empty));
                    }
                }
            }
        }

        /// <summary>
        /// № страницы, с которой начинать нумерацию
        /// </summary>
        public int PageStartingNumber { get; set; }

        /// <summary>
        /// Начинать ли нумерацию заново
        /// </summary>
        public bool RestartPageNumbering { get; set; }

        public override string ToString()
        {
            return "psn:" + PageStartingNumber + ";rpn:" + RestartPageNumbering;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}