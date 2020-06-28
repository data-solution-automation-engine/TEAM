using System;
using System.Collections.Generic;
using System.Text;

namespace TEAM
{
    /// <summary>
    /// These settings are driven by the TEAM application.
    /// They have to be updated through TEAM, i.e. via the Team Configuration / Settings file in the designated directory.
    /// </summary>
    internal class TeamConfigurationSettings
    {
        //Prefixes
        internal static string StgTablePrefixValue { get; set; }
        internal static string PsaTablePrefixValue { get; set; }
        internal static string HubTablePrefixValue { get; set; }
        internal static string SatTablePrefixValue { get; set; }
        internal static string LinkTablePrefixValue { get; set; }
        internal static string LsatPrefixValue { get; set; }

        //Connection strings
        internal static TeamConnectionProfile MetadataConnection { get; set; } = new TeamConnectionProfile();

        internal static string DwhKeyIdentifier { get; set; }
        internal static string PsaKeyLocation { get; set; }
        internal static string SchemaName { get; set; }
        internal static string EventDateTimeAttribute { get; set; }
        internal static string LoadDateTimeAttribute { get; set; }
        internal static string ExpiryDateTimeAttribute { get; set; }
        internal static string ChangeDataCaptureAttribute { get; set; }
        internal static string RecordSourceAttribute { get; set; }
        internal static string EtlProcessAttribute { get; set; }
        internal static string EtlProcessUpdateAttribute { get; set; }
        internal static string RowIdAttribute { get; set; }
        internal static string RecordChecksumAttribute { get; set; }
        internal static string CurrentRowAttribute { get; set; }
        internal static string AlternativeRecordSourceAttribute { get; set; }
        internal static string AlternativeLoadDateTimeAttribute { get; set; }
        internal static string AlternativeSatelliteLoadDateTimeAttribute { get; set; }
        internal static string LogicalDeleteAttribute { get; set; }
        internal static string TableNamingLocation { get; set; }
        internal static string KeyNamingLocation { get; set; }
        internal static string EnableAlternativeSatelliteLoadDateTimeAttribute { get; set; }
        internal static string EnableAlternativeRecordSourceAttribute { get; set; }
        internal static string EnableAlternativeLoadDateTimeAttribute { get; set; }
    }
}
