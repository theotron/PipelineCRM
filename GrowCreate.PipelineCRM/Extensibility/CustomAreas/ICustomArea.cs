using GrowCreate.PipelineCRM.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowCreate.PipelineCRM.CustomAreas
{
    public interface ICustomArea
    {
        string Name { get; }
        string Icon { get; }
        string Alias { get; }
        string Url { get; }
        IEnumerable<CustomerAreaFolder> Folders { get; }
        int SortOrder { get; }
    }

    public class CustomerAreaFolder
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
