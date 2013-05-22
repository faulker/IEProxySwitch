using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IESettingRefresh
{
    class Program
    {
        static void Main(string[] args)
        {
            proxy px = new proxy();
            px.changeProxyState();
        }
    }
}
