using System;
using Newtonsoft.Json;
using VK.WindowsPhone.SDK.Json;

namespace VK.WindowsPhone.SDK.API.Model
{
	public class VKMarketItemPrice
	{
		/// <summary>
		/// Цена товара в сотых долях единицы валюты
		/// </summary>
		[JsonProperty("amount")]
		public double Amount { get; set; }

		/// <summary>
		/// Валюта
		/// </summary>
		[JsonProperty("currency")]
		public VKCurrency Currency { get; set; }

		/// <summary>
		/// Строковое представление цены
		/// </summary>
		[JsonProperty("text")]
		public string Text { get; set; }
	}

	public class VKMarketSection
	{
		/// <summary>
		/// идентификатор секции
		/// </summary>
		[JsonProperty("id")]
		public long SectionID { get; set; }

		/// <summary>
		/// название секции
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public class VKMarketCategory
	{
		/// <summary>
		/// идентификатор категории
		/// </summary>
		[JsonProperty("id")]
		public long CategoryID { get; set; }

		/// <summary>
		/// название категории
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// секция
		/// </summary>
		[JsonProperty("section")]
		public VKMarketSection Section { get; set; }
	}

	public enum VKMarketAvailability
	{
		/// <summary>
		/// товар доступен
		/// </summary>
		Available = 0,

		/// <summary>
		/// товар удален
		/// </summary>
		Deleted = 1,

		/// <summary>
		/// товар недоступен
		/// </summary>
		Unavailable = 2
	}

	public class VKMarketLike
	{
		/// <summary>
		/// есть ли отметка «Мне нравится» от текущего пользователя
		/// </summary>
		[JsonProperty("user_likes")]
		[JsonConverter(typeof(JsonBoolConverter))]
		public bool CurrentUserLikesIt { get; set; }

		/// <summary>
		/// число отметок «Мне нравится»
		/// </summary>
		[JsonProperty("count")]
		public long LikesCount { get; set; }
	}

	public class VKMarketItem
	{
		/// <summary>
		/// идентификатор товара
		/// </summary>
		[JsonProperty("id")]
		public long ItemID { get; set; }

		/// <summary>
		/// идентификатор владельца товара
		/// </summary>
		[JsonProperty("owner_id")]
		public long OwnerID { get; set; }

		/// <summary>
		/// название товара
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }

		/// <summary>
		/// Описание товара
		/// </summary>
		[JsonProperty("description")]
		public string Description { get; set; }

		/// <summary>
		/// Цена товата
		/// </summary>
		[JsonProperty("price")]
		public VKMarketItemPrice Price { get; set; }

		/// <summary>
		/// Категория товара
		/// </summary>
		[JsonProperty("category")]
		public VKMarketCategory Category { get; set; }

		/// <summary>
		/// URL изображения-обложки товара
		/// </summary>
		[JsonProperty("thumb_photo")]
		public string CoverPhoto { get; set; }

		/// <summary>
		/// дата создания товара
		/// </summary>
		[JsonProperty("date")]
		[JsonConverter(typeof(JsonUnixTimeConverter))]
		public DateTime Date { get; set; }

		/// <summary>
		/// Статус товара
		/// </summary>
		[JsonProperty("availability")]
		public VKMarketAvailability Availability { get; set; }

		/// <summary>
		/// изображения товара
		/// </summary>
		[JsonProperty("photos")]
		public VKPhoto[] Photos { get; set; }

		/// <summary>
		/// возможность комментировать товар для текущего пользователя
		/// </summary>
		[JsonProperty("can_comment")]
		public bool CanComment { get; set; }

		/// <summary>
		/// возможность сделать репост товара для текущего пользователя
		/// </summary>
		[JsonProperty("can_repost")]
		public bool CanRepost { get; set; }

		/// <summary>
		/// информация об отметках «Мне нравится»
		/// </summary>
		[JsonProperty("likes")]
		public VKMarketLike Likes { get; set; }
	}
}
