#:property TargetFramework=net10.0-windows
#:property UseWPF=true
#:property PublishAot=false
#:property Nullable=disable
#:property RootNamespace=UpgradeWf
#:project ..\WireFlow.Repository.Sqlite\WireFlow.Repository.Sqlite.csproj

// One-time upgrade of .wf files from format 1 (JSON with $type tags naming .NET classes) to format 2 (FlowJson: an
// element's Type names its kind). The programs open format 2 only. Each document is read with the format 1 settings,
// written with FlowJson, and checked: reading the new JSON must give the same document. A file is changed only when
// every document passes, in one transaction.
// Usage: dotnet run upgrade-wf-to-format-2.cs -- <file.wf> [<file.wf> ...]
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using WireFlow;

const long APPLICATION_ID = 0x57464C57; // "WFLW"
var format1 = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
int failedFiles = 0;

foreach (string file in args)
{
    Console.WriteLine($"== {file}");
    using var connection = new SqliteConnection($"Data Source={file};Mode=ReadWrite;Pooling=False");
    connection.Open();
    long Scalar(string sql) => (long)new SqliteCommand(sql, connection).ExecuteScalar();
    if (Scalar("PRAGMA application_id") != APPLICATION_ID)
    {
        Console.WriteLine("  not a WireFlow file; left as it is");
        failedFiles++;
        continue;
    }
    long format = Scalar("PRAGMA user_version");
    if (format != 1)
    {
        Console.WriteLine($"  format {format}; left as it is");
        continue;
    }

    // Every JSON value in the file, with the row it is in and the kind of document it is
    var rows = new List<(string Table, string Where, object[] Keys, string Column, string Kind, string Json)>();
    void Collect(string sql, Func<SqliteDataReader, (string, string, object[], string, string, string)> row)
    {
        using var reader = new SqliteCommand(sql, connection).ExecuteReader();
        while (reader.Read())
            if (!reader.IsDBNull(reader.FieldCount - 1))
                rows.Add(row(reader));
    }
    Collect("SELECT Json FROM Project", r => ("Project", "Id = 1", new object[0], "Json", "project", r.GetString(0)));
    foreach (string column in new[] { "Saved", "Draft" })
        Collect($"SELECT Kind, Name, {column} FROM Documents", r => ("Documents", "Kind = $k0 AND Name = $k1",
            new object[] { r.GetString(0), r.GetString(1) }, column, r.GetString(0), r.GetString(2)));
    Collect("SELECT Kind, Name, RevisionNo, Json FROM Archive", r => ("Archive", "Kind = $k0 AND Name = $k1 AND RevisionNo = $k2",
        new object[] { r.GetString(0), r.GetString(1), r.GetInt64(2) }, "Json", r.GetString(0), r.GetString(3)));

    Type TypeOf(string kind) => kind switch { "project" => typeof(FlowProject), "part" => typeof(FlowPart), _ => typeof(FlowPage) };
    int failures = 0;
    var updates = new List<(int Row, string Json)>();
    for (int i = 0; i < rows.Count; i++)
    {
        var row = rows[i];
        string what = $"{row.Table} {row.Kind} {string.Join("/", row.Keys)} {row.Column}";
        object old;
        try
        {
            old = JsonConvert.DeserializeObject(row.Json, TypeOf(row.Kind), format1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL {what}: format 1 JSON unreadable: {ex.Message}");
            failures++;
            continue;
        }
        string json = FlatFolderRepository.SerializeToJSON(old);
        object again = json == null ? null : JsonConvert.DeserializeObject(json, TypeOf(row.Kind), FlowJson.Settings);
        if (again == null || FlatFolderRepository.SerializeToJSON(again) != json)
        {
            Console.WriteLine($"  FAIL {what}: does not read back the same");
            failures++;
            continue;
        }
        updates.Add((i, json));
    }
    if (failures > 0)
    {
        Console.WriteLine($"  {failures} documents failed; the file is left as it is");
        failedFiles++;
        continue;
    }

    using (var transaction = connection.BeginTransaction())
    {
        foreach (var (i, json) in updates)
        {
            var row = rows[i];
            var command = new SqliteCommand($"UPDATE {row.Table} SET {row.Column} = $json WHERE {row.Where}", connection, transaction);
            command.Parameters.AddWithValue("$json", json);
            for (int k = 0; k < row.Keys.Length; k++)
                command.Parameters.AddWithValue("$k" + k, row.Keys[k]);
            if (command.ExecuteNonQuery() != 1)
                throw new InvalidOperationException($"{row.Table} row not updated");
        }
        new SqliteCommand($"PRAGMA user_version = {SqliteRepository.FORMAT_VERSION}", connection, transaction).ExecuteNonQuery();
        transaction.Commit();
    }
    Console.WriteLine($"  upgraded: {updates.Count} JSON values, now format {Scalar("PRAGMA user_version")}");
}
Console.WriteLine(failedFiles == 0 ? "ALL UPGRADED" : $"{failedFiles} FILES NOT UPGRADED");
return failedFiles == 0 ? 0 : 1;
