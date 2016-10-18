using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace VK.WindowsPhone.SDK.API.Model
{
    public class VKList<T>
    {
        public int count { get; set; }

        public List<T> items { get; set; }
    }

	public class VKMarketCommentsList
		: VKList<VKComment>
	{
		[JsonProperty("profiles")]
		public List<VKUser> Profiles { get; set; }

		[JsonProperty("groups")]
		public List<VKGroup> Groups { get; set; }
	}
}
