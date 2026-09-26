using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TapeKhashDesktop.Data
{
    public class TapeRow
    {
        public int Id;
        public string Box;
        public string Tape;
        public string ImageUrl;
        public List<string> ImageUrls = new List<string>();
        public string Status;
        public string HolderUsername;
        public string HolderName;
        public bool IsMine;
        public string Note;

        public static TapeRow FromJson(JObject o)
        {
            var row = new TapeRow
            {
                Id = o.Value<int?>("row") ?? 0,
                Box = o.Value<string>("box") ?? "",
                Tape = o.Value<string>("tape") ?? "",
                ImageUrl = o.Value<string>("imageUrl") ?? "",
                Status = o.Value<string>("status") ?? "",
                HolderUsername = o.Value<string>("holderUsername") ?? "",
                HolderName = o.Value<string>("holderName") ?? "",
                IsMine = o.Value<bool?>("isMine") ?? false,
                Note = o.Value<string>("note") ?? "",
            };
            var arr = o["imageUrls"] as JArray;
            if (arr != null) foreach (var u in arr) row.ImageUrls.Add(u.ToString());
            return row;
        }
    }
}
