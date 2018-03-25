using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gsec.model;

namespace gsec.ui.events
{
    public class RangerSelectionEventArgs : EventArgs
    {
        public Ranger Ranger { get; set; }

        public RangerSelectionEventArgs(Ranger ranger)
        {
            Ranger = ranger;
        }
    }

    public delegate void RangerSelectionEventHandler(object sender, RangerSelectionEventArgs e);
}
