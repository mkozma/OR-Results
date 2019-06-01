using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace OR_Results
{

    enum Status
    {
        [Description("Finished")]
        Finished,
        [Description("Started")]
        Started,
        [Description("Mispunch")]
        Mispunch,
        [Description("Did Not Finish")]
        DidNotFinish,
        [Description("Did Not Start")]
        DidNotStart
    }
}
