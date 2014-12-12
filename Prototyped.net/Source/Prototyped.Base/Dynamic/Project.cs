using System.Collections.Generic;

namespace Prototyped.Base.Dynamic
{
    public class Project
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Program { get; set; }
        public string FileName { get; set; }
        public List<SourceFile> Sources { get; set; }
    }
}
