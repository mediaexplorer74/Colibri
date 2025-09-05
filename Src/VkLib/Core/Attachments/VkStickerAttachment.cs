using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Attachments
{
    public class VkStickerAttachment : VkAttachment
    {
        /// <summary>
        /// Id of stickers set
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 64x64 image
        /// </summary>
        public string Photo64 { get; set; }

        /// <summary>
        /// 128x128 image
        /// </summary>
        public string Photo128 { get; set; }

        /// <summary>
        /// 256x256 image
        /// </summary>
        public string Photo256 { get; set; }

        /// <summary>
        /// 352x252 image
        /// </summary>
        public string Photo352 { get; set; }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public override string Type { get { return "sticker"; } }

        public static new VkStickerAttachment FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkStickerAttachment();

            // Support both old (id) and new (sticker_id) schemas
            var idToken = json["id"] ?? json["sticker_id"];
            if (idToken != null)
                result.Id = idToken.Value<long>();

            var productIdToken = json["product_id"];
            if (productIdToken != null)
                result.ProductId = productIdToken.Value<long>();

            // Old fields mapping if available
            if (json["photo_64"] != null)
                result.Photo64 = (string)json["photo_64"];
            if (json["photo_128"] != null)
                result.Photo128 = (string)json["photo_128"];
            if (json["photo_256"] != null)
                result.Photo256 = (string)json["photo_256"];
            if (json["photo_352"] != null)
                result.Photo352 = (string)json["photo_352"];

            // New schema: images / images_with_background arrays
            // We prefer plain images if present, otherwise fallback to images_with_background
            var imagesToken = json["images"] ?? json["images_with_background"];
            if (imagesToken != null && imagesToken.Type == JTokenType.Array)
            {
                // Choose URLs for approximate sizes 64, 128, 256 by the closest greater-or-equal width
                string GetBestUrlForSize(int size)
                {
                    var imgs = imagesToken
                        .Where(t => t["url"] != null && t["width"] != null)
                        .Select(t => new { Url = (string)t["url"], Width = (int)t["width"], Height = (int?)(t["height"] ?? 0) })
                        .OrderBy(t => t.Width)
                        .ToList();

                    if (imgs.Count == 0)
                        return null;

                    var best = imgs.FirstOrDefault(t => t.Width >= size) ?? imgs.Last();
                    return best.Url;
                }

                result.Photo64 = result.Photo64 ?? GetBestUrlForSize(64);
                result.Photo128 = result.Photo128 ?? GetBestUrlForSize(128);
                result.Photo256 = result.Photo256 ?? GetBestUrlForSize(256);

                // Width/Height from the largest image
                var largest = imagesToken
                    .Where(t => t["url"] != null && t["width"] != null)
                    .Select(t => new { Width = (int)t["width"], Height = (int?)(t["height"] ?? 0) })
                    .OrderByDescending(t => t.Width)
                    .FirstOrDefault();
                if (largest != null)
                {
                    result.Width = largest.Width;
                    result.Height = largest.Height ?? result.Height;
                }
            }

            if (json["width"] != null)
                result.Width = (int)json["width"];

            if (json["height"] != null)
                result.Height = (int)json["height"];

            return result;
        }
    }
}
