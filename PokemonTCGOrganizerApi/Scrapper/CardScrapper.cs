using HtmlAgilityPack;
using System.Net.Http;

public class CardScraper
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<PokemonCard>> ScrapeScarletVioletSeriesAsync()
    {
        var allCards = new List<PokemonCard>();
        var baseUrl = "https://www.tcgcollector.com";

        // Requisição com User-Agent
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/sets");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        var response = await _httpClient.SendAsync(request);
        var html = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Encontra a div com os sets da série Scarlet & Violet
        var svDiv = doc.GetElementbyId("scarlet-violet-series");
        if (svDiv == null)
            return allCards; // Div não encontrada

        // Seleciona todos os links para os sets dentro da div
        var setLinks = svDiv.SelectNodes(".//a[contains(@href, '/sets/')]")
            ?.Select(a => baseUrl + a.GetAttributeValue("href", ""))
            .Distinct()
            .ToList() ?? new();
        
        
        var ppDiv = doc.GetElementbyId("play-pokemon-series");
        if (ppDiv == null)
            return allCards;
        setLinks.AddRange(ppDiv.SelectNodes(".//a[contains(@href, '/sets/')]")
            ?.Select(a => baseUrl + a.GetAttributeValue("href", ""))
            .Distinct()
            .ToList() ?? new());

        foreach (var setUrl in setLinks)
        {
            try
            {
                var setHtml = await _httpClient.GetStringAsync(setUrl);
                var setDoc = new HtmlDocument();
                setDoc.LoadHtml(setHtml);

                var setNameNode = setDoc.GetElementbyId("card-search-result-title-set-like-name");//setDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'page-title')]");
                var setName = setNameNode?.InnerText.Trim() ?? "Unknown";
                var setCodeNode = setDoc.GetElementbyId("card-search-result-title-set-code");
                var setCode = setCodeNode?.InnerText.Trim() ?? "PPS";               
                var cardNodes = setDoc.DocumentNode.SelectNodes("//a[contains(@class, 'card-image-grid-item-link')]");
                if (cardNodes == null) continue;

                foreach (var node in cardNodes)
                {
                    var id = node.SelectSingleNode(".//span[contains(@class,'card-image-grid-item-info-overlay-text-part')]")?.InnerText.Trim();
                    var name = node.SelectSingleNode(".//div[contains(@class,'card-image-grid-item-card-title')]")?.InnerText.Trim();
                    var img = node.SelectSingleNode(".//img")?.GetAttributeValue("src", null);

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                    {
                        allCards.Add(new PokemonCard
                        {
                            CardName = name.Replace("&#039;", "'"),
                            SetCode = setCode,
                            SetName = setName,
                            CardNumber = id.Split('/')[0],
                            CardId = id,
                            ImageUrl = img
                        });
                    }
                }
            }
            catch
            {
                // Ignora erros de scraping por set
                continue;
            }
        }

        return allCards;
    }
}
