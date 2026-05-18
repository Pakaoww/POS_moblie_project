using System.Net.Http.Json;
using System.Text.Json;

namespace POS_moblie_project.Services;

public class ProductAiService
{
    private readonly HttpClient _http = new();
    private const string ApiKey = "sk-ItPqctZUrqlbVGqEJkJXTLMnEuM9YG7Pw5nsE2pkUtBbeLCd";
    private const string ApiUrl = "https://api.opentyphoon.ai/v1/chat/completions";
    private const string Model = "typhoon-v2.5-30b-a3b-instruct";

    public async Task<ProductSuggestion> AnalyzeTextAsync(string rawText)
    {
        var requestBody = new
        {
            model = Model,
            max_tokens = 300,
            messages = new[]
            {
                new
                {
                    role    = "system",
                    content = "You are a product data analysis assistant. Extract product information from the provided text. Always respond with valid JSON only, no additional text or formatting."
                },
                new
                {
                    role    = "user",
                    content = $$"""
                        Extract product information from the following text:
                        "{{rawText}}"

                        Analyze and respond ONLY with valid JSON in this exact format:
                        {
                          "productName": "The name or title of the product",
                          "productCode": "The product code, SKU, or barcode if found",
                          "price": 0.0
                        }

                        Rules:
                        - If a field is not found, use empty string for productName/productCode and 0 for price
                        - Ensure price is a valid decimal number
                        - Return ONLY the JSON object, no other text
                        """
                }
            }
        };

        return await SendRequestAsync(requestBody);
    }

    public async Task<ProductSuggestion> AnalyzeImageAsync(Stream imageStream)
    {
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());

        var requestBody = new
        {
            model = "typhoon-v2-vision-instruct",
            max_tokens = 300,
            messages = new object[]
            {
                new
                {
                    role    = "system",
                    content = "You are a product image analysis assistant. Extract product information from images. Always respond with valid JSON only, no additional text or formatting."
                },
                new
                {
                    role    = "user",
                    content = new object[]
                    {
                        new
                        {
                            type      = "image_url",
                            image_url = new
                            {
                                url = $"data:image/jpeg;base64,{base64}"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = """
                                Analyze this product image and extract the following information.
                                Respond ONLY with valid JSON in this exact format:
                                {
                                  "productName": "The product name or title visible in the image",
                                  "productCode": "The product code, SKU, or barcode if visible",
                                  "price": 0.0
                                }

                                Rules:
                                - If a field is not visible, use empty string for productName/productCode and 0 for price
                                - Extract price as a decimal number without currency symbols
                                - Return ONLY the JSON object, no other text
                                """
                        }
                    }
                }
            }
        };

        return await SendRequestAsync(requestBody);
    }

    private async Task<ProductSuggestion> SendRequestAsync(object requestBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("Authorization", $"Bearer {ApiKey}");
        request.Content = JsonContent.Create(requestBody);

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        content = content
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        return JsonSerializer.Deserialize<ProductSuggestion>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new ProductSuggestion();
    }
}

public class ProductSuggestion
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
}