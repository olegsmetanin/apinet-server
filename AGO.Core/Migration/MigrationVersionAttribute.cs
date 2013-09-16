using FluentMigrator;

namespace AGO.Core.Migration
{
    /// <summary>
    /// Атрибут версии миграции с нумерацией согласно нашей конвенции
    /// YYYYMMDDXX, где
    /// YYYY - год
    /// MM - месяц
    /// DD - день
    /// XX - номер версии в пределах одного дня
    /// </summary>
    public class MigrationVersionAttribute: MigrationAttribute
    {
        public MigrationVersionAttribute(int year, int month, int day, int version)
            : base(CalculateVersion(year, month, day, version))
        {
        }

        public MigrationVersionAttribute(int year, int month, int day, int version, TransactionBehavior transactionBehavior)
            : base(CalculateVersion(year, month, day, version), transactionBehavior)
        {
        }

        internal static long CalculateVersion(int year, int month, int day, int version)
        {
            return year*1000000L + month*10000L + day*100L + version;
        }
    }
}