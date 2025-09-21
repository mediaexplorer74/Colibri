using System;
using System.Collections.Generic;
using VkLib.Core.Audio;
using VkLib.Core.Users;
using VkLib.Core.Attachments;

namespace Colibri.Model
{
    /// <summary>
    /// Post with attached tracks (from News/Wall)
    /// </summary>
    public class AudioPost
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public VkProfileBase Author { get; set; }
        public Uri ImageUri { get; set; }
        public List<VkLib.Core.Audio.VkAudio> Tracks { get; set; }
        public List<VkVideoAttachment> Videos { get; set; }
        public Uri PostUri { get; set; }
        public Uri AuthorUri { get; set; }
    }
}
