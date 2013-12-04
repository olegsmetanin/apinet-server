using System;
using System.IO;
using System.Threading;
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
        string FileName { get; }

		/// <summary>
		/// ���������� MIME-��� ����� � �������
		/// </summary>
    	string ContentType { get; }
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
        /// <param name="token">����� ��� ���������� ����������</param>
        void MakeReport(string pathToTemplate, XmlDocument data, CancellationToken token);
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
        /// <param name="token">����� ��� ���������� ����������</param>
        void MakeReport(string parameters, Func<Guid, string> templateResolver, Guid mainTemplateId, CancellationToken token);
    }
}