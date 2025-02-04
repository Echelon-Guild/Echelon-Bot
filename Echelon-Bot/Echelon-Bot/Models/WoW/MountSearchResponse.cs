using System.Text.Json.Serialization;

namespace EchelonBot.Models.WoW
{
    public class MountSearchResult
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
        [JsonPropertyName("maxPageSize")]
        public int MaxPageSize { get; set; }
        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }
        [JsonPropertyName("results")]
        public Result[] Results { get; set; }
    }

    public class Result
    {
        [JsonPropertyName("key")]
        public Key Key { get; set; }
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Key
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("creature_displays")]
        public CreatureDisplay[] CreatureDisplays { get; set; }
        [JsonPropertyName("name")]
        public LocalizedName Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("source")]
        public Source Source { get; set; }
        [JsonPropertyName("faction")]
        public Faction Faction { get; set; }
    }

    public class LocalizedName
    {
        [JsonPropertyName("it_IT")]
        public string Italian { get; set; }
        [JsonPropertyName("ru_RU")]
        public string Russian { get; set; }
        [JsonPropertyName("en_GB")]
        public string EnglishGB { get; set; }
        [JsonPropertyName("zh_TW")]
        public string ChineseTW { get; set; }
        [JsonPropertyName("ko_KR")]
        public string Korean { get; set; }
        [JsonPropertyName("en_US")]
        public string EnglishUS { get; set; }
        [JsonPropertyName("es_MX")]
        public string SpanishMX { get; set; }
        [JsonPropertyName("pt_BR")]
        public string PortugueseBR { get; set; }
        [JsonPropertyName("es_ES")]
        public string SpanishES { get; set; }
        [JsonPropertyName("zh_CN")]
        public string ChineseCN { get; set; }
        [JsonPropertyName("fr_FR")]
        public string French { get; set; }
        [JsonPropertyName("de_DE")]
        public string German { get; set; }
    }

    public class Source
    {
        [JsonPropertyName("name")]
        public LocalizedName Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Faction
    {
        [JsonPropertyName("name")]
        public LocalizedName Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class CreatureDisplay
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
