using System;
using System.IO;
using System.Xml;


namespace AGO.Reporting.Common
{
    /// <summary>
    /// ��������� ������ ���������� �������
    /// </summary>
    public interface IReportGeneratorResult
    {
        /// <summary>
        /// ���������� ��������� ������ ���������� � ���� ������
        /// <exception cref="NotSupportedException">���������, ���� ��������� ������ �� ����� ���� �������� � �����</exception>
        /// </summary>
        Stream Result { get; }

        /// <summary>
        /// ���������� ������������ �� ��������� ��� ��� ����� ������
        /// </summary>
        string FileName { get; set; }
    }

    /// <summary>
    /// ���� ��������� ������ ������������ ���������� ������� � ������ ��������.
    /// </summary>
    public interface IReportGenerator: IReportGeneratorResult
    {
        /// <summary>
        /// ��������� ������� �������� ������ (���� ���� ����� ���������)
        /// </summary>
        /// <param name="pathToTemplate">���� � ������� ������. ���� ��� ��������� ��������� ��
        /// �� �����, �� ����� ���� null ��� ������ �������, ������� �� ������������ ������ ����������</param>
        /// <param name="data">������, �� ������� ���������� ��������� �����</param>
        void MakeReport(string pathToTemplate, XmlDocument data);
    }

    /// <summary>
    /// ��������� ����������� �������, ����������� "������" ������ �������� ������, �� 
    /// ��������������� �������� �������� ���������� ������ � ������� xml � �������� ��
    /// � ������ ������.
    /// </summary>
    public interface ICustomReportGenerator: IReportGeneratorResult
    {
        /// <summary>
        /// ���������� �����
        /// </summary>
        /// <param name="parameters">��������� ��� ��������� ������ (json)</param>
        /// <param name="templateResolver">������� ��� ���������� �������������� ������� ������ � ���� � ����� �������</param>
        /// <param name="mainTemplateId">������������� ��������� ������� ������</param>
        void MakeReport(string parameters, Func<Guid, string> templateResolver, Guid mainTemplateId);
    }
}