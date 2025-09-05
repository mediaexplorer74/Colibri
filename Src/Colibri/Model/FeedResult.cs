using System.Collections.Generic;

namespace Colibri.Model
{
    public class FeedResult
    {
        public List<AudioPost> Posts { get; set; }
        public string NextFrom { get; set; }
    }
}
