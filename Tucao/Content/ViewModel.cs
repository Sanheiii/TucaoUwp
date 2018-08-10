using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tucao
{
    class ViewModel
    {
        /// <summary>
        /// 历史记录
        /// </summary>
        [Serializable]
        public class History
        {
            public string Hid { get; set; }
            public string Title { get; set; }
            public int Part { get; set; }
            public string Position { get; set; }
            public long Time { get; set; }
        }
    }
}
