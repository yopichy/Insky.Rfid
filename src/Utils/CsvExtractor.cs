using GenericParsing;
using System;
using System.Data;

namespace Insky.Rfid.Utils;

public static class CsvExtractor
{
    public static DataTable Read(string fileName, char separator)
    {
        if (string.IsNullOrEmpty(fileName) || separator == default(char)) return null;

        var dt = new DataTable();
        using (var parser = new GenericParser())
        {
            parser.SetDataSource(fileName);
            parser.ColumnDelimiter = separator;

            var i = 0;
            while (parser.Read())
            {
                switch (i)
                {
                    case 0:
                        dt.Columns.Add("TagId", typeof(string));
                        dt.Columns.Add("IsWritten", typeof(int));
                        break;
                    default:
                    {
                        var tagId = parser[0];
                        int isWritten = Convert.ToInt16(parser[1]);
                        dt.Rows.Add(tagId, isWritten);
                        break;
                    }
                }

                i++;
            }
        }

        return dt;
    }
}