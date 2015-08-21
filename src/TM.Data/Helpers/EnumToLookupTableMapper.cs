using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;


namespace TM.Data
{
   public class EnumToLookupTableMapper
   {
      private readonly DbContext _context;
      private readonly EdmMetadataHandler _metadataHandler;

      public EnumToLookupTableMapper(DbContext context)
         : this(context, new EdmMetadataHandler(context.GetMetadataWorkspace()))
      { }

      internal EnumToLookupTableMapper(DbContext context, EdmMetadataHandler metadataHandler)
      {
         _context = context;
         _metadataHandler = metadataHandler;

         Schema = "Enum";
         NameFieldLength = 255;
      }

      public string Schema { get; private set; }

      public int NameFieldLength { get; set; }


      public void MapEnums()
      {
         var lookupTables = _metadataHandler.GetEnumLookupTables();
         var scriptGenerator = new SqlServerScriptGenerator(Schema, NameFieldLength);
         var script = scriptGenerator.GenerateScript(lookupTables);
         ExecuteSqlCommand(script);
      }

      private int ExecuteSqlCommand(string sql)
      {
         var result = _context.Database.ExecuteSqlCommand(sql);
         return result;
      }


      #region Nested Classes

      [SuppressMessage("ReSharper", "StringLiteralTypo")]
      internal class SqlServerScriptGenerator
      {
         private readonly string _lookupTableSchema;
         private readonly int _nameFieldLength;

         public SqlServerScriptGenerator(string lookupTableSchema, int nameFieldLength)
         {
            _lookupTableSchema = lookupTableSchema;
            _nameFieldLength = nameFieldLength;
         }
         
         public string GenerateScript(ICollection<EnumLookupTable> lookupTables)
         {
            var sql = new StringBuilder();
            sql.AppendLine("set nocount on;");
            sql.AppendLine("set xact_abort on; -- rollback on error");
            sql.AppendLine("begin tran;");
            sql.AppendLine(CreateSchema());
            sql.AppendLine(CreateTables(lookupTables));
            sql.AppendLine(PopulateTables(lookupTables));
            sql.AppendLine(AddForeignKeys(lookupTables));
            sql.AppendLine("commit;");
            return sql.ToString();
         }

         private string CreateSchema()
         {
            var sql = string.Format(
               @"IF NOT EXISTS(
SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME = N'{0}')
begin
  EXEC sp_executesql N'CREATE SCHEMA {0} AUTHORIZATION dbo'
end",
               _lookupTableSchema);

            return sql;
         }

         private string CreateTables(IEnumerable<EnumLookupTable> lookupTables)
         {
            var sql = new StringBuilder();
            foreach (var lookupTable in lookupTables)
            {
               sql.AppendFormat(
                  @"IF OBJECT_ID(N'[{0}].[{1}]', N'U') IS NULL
begin
	CREATE TABLE [{0}].[{1}] (Id {2} CONSTRAINT [PK_{0}.{1}] PRIMARY KEY, Name nvarchar({3}));
	exec sys.sp_addextendedproperty @name=N'Warning', @level0type=N'SCHEMA', @level0name=N'{0}', @level1type=N'TABLE',
		@level1name=N'{1}', @value=N'Automatically generated. Contents will be overwritten on app startup.';
end",
                  _lookupTableSchema, lookupTable.Name, lookupTable.PrimaryKeyType, _nameFieldLength);
               sql.AppendLine();
            }
            return sql.ToString();
         }

         private string PopulateTables(IEnumerable<EnumLookupTable> lookupTables)
         {
            var sql = new StringBuilder();
            sql.AppendLine(string.Format("CREATE TABLE #lookups (Id int, Name nvarchar({0}) COLLATE database_default);", _nameFieldLength));
            foreach (var lookupTable in lookupTables)
            {
               sql.AppendLine(PopulateTable(lookupTable));
            }
            sql.AppendLine("DROP TABLE #lookups;");
            return sql.ToString();
         }

         private string PopulateTable(EnumLookupTable lookupTable)
         {
            var sql = new StringBuilder();
            foreach (var row in lookupTable.Rows.OrderBy(x => x.Id))
            {
               sql.AppendFormat("INSERT INTO #lookups (Id, Name) VALUES ({0}, N'{1}');\r\n", row.Id, SanitizeSqlString(row.Name));
            }

            sql.AppendLine(string.Format(@"
MERGE INTO [{0}].[{1}] dst
	USING #lookups src ON src.Id = dst.Id
	WHEN MATCHED AND src.Name <> dst.Name THEN
		UPDATE SET Name = src.Name
	WHEN NOT MATCHED THEN
		INSERT (Id, Name)
		VALUES (src.Id, src.Name)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
;"
               , _lookupTableSchema, lookupTable.Name));

            sql.AppendLine("TRUNCATE TABLE #lookups;");
            return sql.ToString();
         }

         private string AddForeignKeys(IEnumerable<EnumLookupTable> lookupTables)
         {
            var sql = new StringBuilder();
            foreach (var lookupTable in lookupTables)
            {
               foreach (var reference in lookupTable.References)
               {
                  var foreignKeyName = string.Format("[FK_{0}.{1}_{2}.{3}_{4}]",
                  reference.TableSchema, reference.TableName, _lookupTableSchema, lookupTable.Name, reference.ColumnName);

                  sql.AppendFormat(
                     " IF OBJECT_ID(N'[{1}].{0}', N'F') IS NULL ALTER TABLE [{1}].[{2}] ADD CONSTRAINT {0} FOREIGN KEY ({3}) REFERENCES [{4}].[{5}] (Id);\r\n",
                     foreignKeyName, reference.TableSchema, reference.TableName, reference.ColumnName, _lookupTableSchema, lookupTable.Name);
               }
            }
            return sql.ToString();
         }

         private static string SanitizeSqlString(string value)
         {
            return value.Replace("'", "''");
         }

      }


      internal class EdmMetadataHandler
      {
         private readonly MetadataWorkspace _workspace;

         public EdmMetadataHandler(MetadataWorkspace workspace)
         {
            _workspace = workspace;
         }

         public ICollection<EnumLookupTable> GetEnumLookupTables()
         {
            var conceptualEntityContainer = _workspace
               .GetItems<EntityContainer>(DataSpace.CSpace)
               .Single();

            var mappings = _workspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
               .Single()
               .EntitySetMappings
               .ToList();

            var enumSet = new Dictionary<string, EnumLookupTable>();

            foreach (var conceptualEntitySet in conceptualEntityContainer.EntitySets)
            {
               var mappingFragment = mappings.Single(x => x.EntitySet == conceptualEntitySet)
                  .EntityTypeMappings.Single()
                  .Fragments.Single();

               var tableName = mappingFragment.StoreEntitySet.Table;
               var tableSchema = mappingFragment.StoreEntitySet.Schema;

               foreach (var property in conceptualEntitySet.ElementType.Properties)
               {
                  if (property.IsEnumType)
                  {
                     var propertyMapping = (ScalarPropertyMapping)mappingFragment.PropertyMappings
                        .Single(x => x.Property.Name == property.Name);

                     var reference = new TableReference
                     {
                        TableName = tableName,
                        TableSchema = tableSchema,
                        ColumnName = propertyMapping.Column.Name
                     };

                     AddReference(enumSet, property.EnumType, reference);

                  } else if (property.IsComplexType)
                  {
                     var complexPropertyMapping = ((ComplexPropertyMapping)mappingFragment.PropertyMappings
                        .Single(x => x.Property.Name == property.Name));

                     foreach (var complexTypeProperty in property.ComplexType.Properties)
                     {
                        if (complexTypeProperty.IsEnumType)
                        {
                           var propertyMapping = (ScalarPropertyMapping)complexPropertyMapping
                              .TypeMappings
                              .SelectMany(x => x.PropertyMappings)
                              .Single(x => x.Property.Name == complexTypeProperty.Name);

                           var reference = new TableReference
                           {
                              TableName = tableName,
                              TableSchema = tableSchema,
                              ColumnName = propertyMapping.Column.Name
                           };

                           AddReference(enumSet, complexTypeProperty.EnumType, reference);
                        }
                     }
                  }
               }
            }

            return enumSet.Values;
         }

         private void AddReference(Dictionary<string, EnumLookupTable> enumSet, EnumType enumType, TableReference reference)
         {
            EnumLookupTable entry;
            if (enumSet.TryGetValue(enumType.FullName, out entry))
            {
               entry.References.Add(reference);
            } else
            {
               var enumInfo = new EnumLookupTable(enumType, reference);
               enumSet.Add(enumType.FullName, enumInfo);
            }
         }
      }


      internal class TableReference
      {
         public string TableName { get; set; }
         public string TableSchema { get; set; }
         public string ColumnName { get; set; }
      }

      internal class EnumLookupTable
      {
         public EnumLookupTable(EnumType enumType, TableReference reference)
         {
            EnumType = enumType;
            Rows = new List<EnumLookupTableRow>();
            if (enumType.IsFlags)
            {
               foreach (var enumField in enumType.Members)
               {

                  var enumValue = enumField.Value;
                  switch (enumType.UnderlyingType.PrimitiveTypeKind)
                  {
                     case PrimitiveTypeKind.Byte:
                     case PrimitiveTypeKind.SByte:
                        if (((byte)enumValue & ((byte)enumValue - 1)) != 0)
                        {
                           // composed enum value
                           continue;
                        }
                        break;
                     case PrimitiveTypeKind.Int16:
                        if (((short)enumValue & ((short)enumValue - 1)) != 0)
                        {
                           // composed enum value
                           continue;
                        }
                        break;
                     case PrimitiveTypeKind.Int32:
                        if (((int)enumValue & ((int)enumValue - 1)) != 0)
                        {
                           // composed enum value
                           continue;
                        }
                        break;
                     case PrimitiveTypeKind.Int64:
                        if (((long)enumValue & ((long)enumValue - 1)) != 0)
                        {
                           // composed enum value
                           continue;
                        }
                        break;
                     default:
                        throw new ArgumentOutOfRangeException();
                  }

                  var name = enumField.Name.SeparateWords();

                  Rows.Add(new EnumLookupTableRow
                  {
                     Id = enumValue.ToString(),
                     Name = name
                  });
               }
            } else
            {
               foreach (var enumField in enumType.Members)
               {
                  var enumValue = enumField.Value.ToString();
                  var name = enumField.Name.SeparateWords();

                  Rows.Add(new EnumLookupTableRow
                  {
                     Id = enumValue,
                     Name = name
                  });
               }
            }

            References = new List<TableReference>
         {
            reference
         };
         }

         public EnumType EnumType { get; set; }
         public List<TableReference> References { get; set; }
         public List<EnumLookupTableRow> Rows { get; set; }

         public string Name
         {
            get { return EnumType.Name; }
         }

         public string PrimaryKeyType
         {
            get
            {
               switch (EnumType.UnderlyingType.PrimitiveTypeKind)
               {
                  case PrimitiveTypeKind.Byte:
                  case PrimitiveTypeKind.SByte:
                     return "tinyint";
                  case PrimitiveTypeKind.Int16:
                     return "smallint";
                  case PrimitiveTypeKind.Int32:
                     return "int";
                  case PrimitiveTypeKind.Int64:
                     return "bigint";
                  default:
                     throw new ArgumentOutOfRangeException();
               }
            }
         }
      }

      internal class EnumLookupTableRow
      {
         public string Id { get; set; }

         public string Name { get; set; }
      }


      #endregion
   }
}
