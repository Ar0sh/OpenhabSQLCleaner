using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenhabSQLCleaner
{
    internal class OverrideTest
    {
        private int x;
        private int y;

        public OverrideTest(int xx, int yy)
        {
            x = xx;
            y = yy;
        }
        public static OverrideTest operator ++(OverrideTest a)
        {
            a.x += 3;
            a.y += 4;
            return a;
        }

    }
}
