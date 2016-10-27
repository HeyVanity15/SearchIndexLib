using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SearchEngineLib
{
    /// <summary>
    /// This class will represent an inverted index
    /// An inverted index is a hashtable data structure that maps each tokenized word to a list of files and locations
    /// </summary>
    public class Index
    {
        Dictionary<string, List<WordInfo>> _invertedIndex = new Dictionary<string, List<WordInfo>>();
        DataTable _table = null;

        public Index(DataTable table)
        {
            _table = table.Copy();

            Task.Run(() => BuildIndex(table));
        }

        /// <summary>
        /// Sanitize and tokenize all words before adding their location information to the index.
        /// </summary>
        /// <param name="table"></param>
        private void BuildIndex(DataTable table)
        {
            var regex = new Regex("^[0-9a-b]*$/g", RegexOptions.Compiled);

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn field in table.Columns)
                {
                    if (field.ColumnName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                        row[field.ColumnName].Equals(DBNull.Value))
                    {
                        continue;
                    }

                    var allWords = regex.Matches(row[field.ColumnName].ToString());

                    foreach (string word in allWords)
                    {
                        var wiList = new List<WordInfo>();
                        int position = 0;

                        if (_invertedIndex.TryGetValue(word, out wiList))
                        {
                            _invertedIndex.Add(word, new List<WordInfo>());
                        }

                        _invertedIndex[word].Add(new WordInfo
                        {
                            Id = row["id"] as int?,
                            Field = field.ColumnName,
                            Position = position++
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Get the information for the given search term.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public List<WordInfo> GetWordInfo(string word)
        {
            List<WordInfo> wiList = null;

            _invertedIndex.TryGetValue(word, out wiList);

            return wiList;
        }

        /// <summary>
        /// Get a datatable of the rows in which a term match is found.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public DataTable GetMatches(string word)
        {
            List<WordInfo> wiList = null;

            if (_table == null)
                return null;
            
            if (!_invertedIndex.TryGetValue(word, out wiList))
                return null;

            var ids = wiList.Where(wi => wi.Id.HasValue).Select(wi => wi.Id).ToList();

            var matches = _table.AsEnumerable()
                .Where(row => ids.Contains(row["id"] as int?))
                .Select(row => row);

            if (!matches.Any())
                return null;

            return matches.CopyToDataTable();
        }
    }
}
