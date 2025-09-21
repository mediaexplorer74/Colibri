using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Model;
using VkLib.Core.Attachments;
using VkLib.Core.Audio;
using VkLib.Core.Groups;
using GalaSoft.MvvmLight.Messaging;
using Colibri.Services;

namespace Colibri.Services
{
    public class FeedService
    {
        private readonly VkLib.Vk _vk;

        public FeedService(VkLib.Vk vk)
        {
            _vk = vk;
        }

        public async Task<FeedResult> GetNewsAsync(int count = 50, string nextFrom = null, bool onlyWithAudio = true)
        {
            var result = new FeedResult();

            try
            {
                var response = await _vk.News.Get(filters: "post", count: count, startFrom: nextFrom);

                if (response?.Items != null && response.Items.Count > 0)
                {
                    var audioIds = new List<string>();
                    var posts = new List<AudioPost>();
                    var postAudioMatches = new Dictionary<string, IList<string>>();

                    foreach (var newsEntry in response.Items)
                    {
                        // Merge attachments from the post and entire copy history chain
                        var attachments = new List<VkAttachment>();
                        if (newsEntry.Attachments != null)
                            attachments.AddRange(newsEntry.Attachments);
                        if (newsEntry.CopyHistory != null && newsEntry.CopyHistory.Count > 0)
                        {
                            foreach (var ch in newsEntry.CopyHistory)
                            {
                                if (ch.Attachments != null)
                                    attachments.AddRange(ch.Attachments);
                            }
                        }

                        if (attachments == null || attachments.Count == 0)
                            continue;

                        var ids = attachments.Where(a => a is VkAudioAttachment).Select(a => $"{a.OwnerId}_{a.Id}").ToList();
                        audioIds.AddRange(ids);

                        var post = new AudioPost
                        {
                            Id = newsEntry.Id.ToString(),
                            Text = newsEntry.Text,
                            PostUri = new Uri($"http://vk.ru/wall{newsEntry.SourceId}_{newsEntry.Id}"),
                            AuthorUri = new Uri(string.Format("https://vk.ru/{0}{1}", newsEntry.Author is VkGroup ? "club" : "id", newsEntry.Author.Id)),
                            Author = newsEntry.Author,
                            Date = newsEntry.Date
                        };

                        var imageUrl = attachments.OfType<VkPhotoAttachment>().FirstOrDefault()?.SourceMax;
                        if (!string.IsNullOrEmpty(imageUrl))
                            post.ImageUri = new Uri(imageUrl);

                        // Collect video attachments for UI
                        post.Videos = attachments.OfType<VkVideoAttachment>().ToList();

                        posts.Add(post);
                        postAudioMatches.Add(post.Id, ids);
                    }

                    if (audioIds.Count == 0)
                    {
                        result.Posts = onlyWithAudio ? new List<AudioPost>() : posts;
                        result.NextFrom = response.NextFrom;
                        return result;
                    }

                    var tracks = new List<VkAudio>();
                    foreach (var chunk in Split(audioIds, 100))
                    {
                        var resultAudios = await _vk.Audio.GetById(chunk.ToList());
                        if (resultAudios != null && resultAudios.Count > 0)
                            tracks.AddRange(resultAudios);
                    }

                    foreach (var post in posts)
                    {
                        post.Tracks = tracks.Where(t => postAudioMatches[post.Id].Contains($"{t.OwnerId}_{t.Id}")).ToList();
                    }

                    if (onlyWithAudio)
                        result.Posts = posts.Where(p => p.Tracks != null && p.Tracks.Count > 0).ToList();
                    else
                        result.Posts = posts;
                    result.NextFrom = response.NextFrom;
                }
            }
            catch (VkLib.Error.VkInvalidTokenException)
            {
                Messenger.Default.Send(new ViewModel.Messaging.LoginStateChangedMessage { IsLoggedIn = false });
            }

            return result;
        }

        private static IEnumerable<IEnumerable<T>> Split<T>(IList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(size);
        }
    }
}
