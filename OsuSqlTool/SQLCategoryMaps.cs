using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSqlTool
{
    public class SQLCategoryMaps
    {
        public SQLCategoryMaps(IEnumerable<SQLMap> maps, SQLCategory category)
        {
            Category = category;
            Maps = maps.Where(o => o.Category == Category);
        }

        public SQLCategory Category { get; private set; }
        public IEnumerable<SQLMap> Maps { get; private set; }
    }
}
