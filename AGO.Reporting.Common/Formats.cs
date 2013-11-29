using System;

namespace AGO.Reporting.Common
{
    /// <summary>
    /// ������������ ������������ � ������.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// �� �������� ���� ������
        /// </summary>
        Top,

        /// <summary>
        /// �� �������� ������
        /// </summary>
        Middle,

        /// <summary>
        /// �� ������� ���� ������
        /// </summary>
        Bottom
    }

    /// <summary>
    /// �������������� ������������ � ������
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// �� ������ ���� ������
        /// </summary>
        Left,

        /// <summary>
        /// �� ������ ������
        /// </summary>
        Center,

        /// <summary>
        /// �� ������� ���� ������
        /// </summary>
        Right
    }

    /// <summary>
    /// ����������� ������ � ������
    /// </summary>
    public enum TextDirection
    {
        /// <summary>
        /// �� �����������
        /// </summary>
        Horizontal,

        /// <summary>
        /// �� ��������� ����� �����
        /// </summary>
        VerticalBottomToTop,

        /// <summary>
        /// �� ��������� ������ ����
        /// </summary>
        VerticalTopToBottom
    }

    /// <summary>
    /// ����������� ��������������, ������� ���������� �� ���������� ������ ���������� �������.
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
        /// �������������� ������������ ���� � ���������
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// ��������� ������ (�������� ����) ������
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// �������� ������ �������� ����� ���������� ����
        /// </summary>
        public bool PageBreakBefore { get; set; }

        /// <summary>
        /// �������� ������ �������� ����� ��������� ����
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
    /// ����������� ��������������, ������� ���������� �� ���������� ������ ���������� �������.
    /// ��� ����� ������.
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
        /// �������������� ������������
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// ������������ ������������
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// ����������� ������
        /// </summary>
        public TextDirection TextDirection { get; set; }

        /// <summary>
        /// ��������� ������ ������
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// ������ ������
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
    /// �������������� ��� ������ ����� (�.�. ��� ������������ ������).
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
        /// ��� ������ �����.
        /// </summary>
        public string Group { get; set; }

        public override string ToString()
        {
            return "gn:" + Group + ";" + base.ToString();
        }
    }

    /// <summary>
    /// �������������� �������� ������ (������ � Word-���������).
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
        /// � ��������, � ������� �������� ���������
        /// </summary>
        public int PageStartingNumber { get; set; }

        /// <summary>
        /// �������� �� ��������� ������
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