#:property TargetFramework=net10.0-windows
#:property UseWPF=true
#:property PublishAot=false
#:property Nullable=disable
#:property RootNamespace=UpgradeLinks
#:project ..\WireFlow.Repository.Sqlite\WireFlow.Repository.Sqlite.csproj

// One-time upgrade of .wf files for links that name IDs only. Each page without an ID gets one made of its name ("Page 2"
// gives page-2, FlowIds.ForName), the same in its saved version and its draft. Then every link (Href) in the pages, layouts
// and parts, saved and draft, that names a page by its name and is no ID already gets that page's ID. The archive is left
// as it is. A file is changed in one transaction, and only when something changes; running it again changes nothing.
// Usage: dotnet run upgrade-links-to-ids.cs -- <file.wf> [<file.wf> ...]
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using WireFlow;

const long APPLICATION_ID = 0x57464C57; // "WFLW"

foreach (string file in args)
{
    Console.WriteLine($"== {file}");
    using var connection = new SqliteConnection($"Data Source={file};Mode=ReadWrite;Pooling=False");
    connection.Open();
    long Scalar(string sql) => (long)new SqliteCommand(sql, connection).ExecuteScalar();
    if (Scalar("PRAGMA application_id") != APPLICATION_ID || Scalar("PRAGMA user_version") != 2)
    {
        Console.WriteLine("  not a WireFlow file of format 2; left as it is");
        continue;
    }

    // Every version of every document, read
    var docs = new List<(string Kind, string Name, string Column, FlowBase Node)>();
    foreach (var column in new[] { "Saved", "Draft" })
    {
        using var reader = new SqliteCommand($"SELECT Kind, Name, {column} FROM Documents WHERE {column} IS NOT NULL", connection).ExecuteReader();
        while (reader.Read())
        {
            string kind = reader.GetString(0), name = reader.GetString(1), json = reader.GetString(2);
            FlowBase node = kind == "part" ? FlatFolderRepository.DeserializeFromJSON<FlowPart>(json, name) : FlatFolderRepository.DeserializeFromJSON<FlowPage>(json, name);
            docs.Add((kind, name, column, node));
        }
    }
    var changed = new HashSet<(string, string, string)>();
    var ids = FlowIds.NewSet();
    foreach (var doc in docs)
        ids.UnionWith(FlowIds.IdsIn(doc.Node));

    // Each page's ID: its own, in either version, or a new one made of its name
    var pageIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var group in docs.Where(d => d.Kind == "page").GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
    {
        string id = group.Select(d => d.Node.Id).FirstOrDefault(i => !string.IsNullOrWhiteSpace(i));
        if (id == null)
        {
            id = FlowIds.ForName(group.Key, ids);
            ids.Add(id);
            Console.WriteLine($"  page \"{group.Key}\": ID {id}");
        }
        pageIds[group.Key] = id;
        foreach (var doc in group.Where(d => string.IsNullOrWhiteSpace(d.Node.Id)))
        {
            doc.Node.Id = id;
            changed.Add((doc.Kind, doc.Name, doc.Column));
        }
    }

    // Links that name a page by its name
    foreach (var doc in docs)
        foreach (var element in FlowIds.Nodes(doc.Node).OfType<FlowElementBase>())
        {
            string value = element.Href?.Trim();
            if (string.IsNullOrEmpty(value) || ids.Contains(value) || !pageIds.TryGetValue(value, out var target))
                continue;
            element.Href = target;
            changed.Add((doc.Kind, doc.Name, doc.Column));
            Console.WriteLine($"  {doc.Kind} \"{doc.Name}\" ({doc.Column.ToLowerInvariant()}): {element.Type ?? "element"} \"{element.Name}\" links to {target}, was \"{value}\"");
        }

    if (changed.Count == 0)
    {
        Console.WriteLine("  nothing to change");
        continue;
    }
    using var transaction = connection.BeginTransaction();
    foreach (var doc in docs.Where(d => changed.Contains((d.Kind, d.Name, d.Column))))
    {
        var command = new SqliteCommand($"UPDATE Documents SET {doc.Column} = $json WHERE Kind = $kind AND Name = $name", connection, transaction);
        command.Parameters.AddWithValue("$json", FlatFolderRepository.SerializeToJSON(doc.Node));
        command.Parameters.AddWithValue("$kind", doc.Kind);
        command.Parameters.AddWithValue("$name", doc.Name);
        command.ExecuteNonQuery();
    }
    transaction.Commit();
    Console.WriteLine($"  {changed.Count} document version(s) written");
}
