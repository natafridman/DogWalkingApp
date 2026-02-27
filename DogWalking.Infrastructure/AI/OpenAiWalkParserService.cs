using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;

namespace DogWalking.Infrastructure.AI;

/// <summary>
/// Calls the OpenAI Chat Completions API to parse natural language walk requests.
/// Uses gpt-4o-mini (cheapest model) with structured JSON output.
/// API key is injected via constructor from appsettings.json.
/// </summary>
public sealed class OpenAiWalkParserService : IAiWalkParserService
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string Model = "gpt-4o-mini";

    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenAiWalkParserService(HttpClient http, string apiKey)
    {
        _http = http;
        _apiKey = apiKey;
    }

    public async Task<AiParsedWalkRequest> ParseAsync(string input, IEnumerable<string> existingDogNames,
        string defaultZone, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set it in appsettings.json under OpenAI:ApiKey.");

        var systemPrompt = BuildSystemPrompt(existingDogNames, defaultZone);
        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = input }
            },
            temperature = 0.1,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _http.SendAsync(request, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = TryExtractErrorMessage(responseJson);
            throw new InvalidOperationException($"OpenAI API error ({response.StatusCode}): {errorDetail}");
        }

        return ParseResponse(responseJson);
    }

    private static string BuildSystemPrompt(IEnumerable<string> existingDogNames, string defaultZone)
    {
        var dogNames = string.Join(", ", existingDogNames);
        var zones = string.Join(", ", WalkZoneExtensions.All().Select(z => z.ToDisplayName()));
        var breeds = string.Join(", ", DogBreedExtensions.All().Select(b => b.ToDisplayName()));

        return $$"""
            You are a dog walking schedule assistant. Parse the user's natural language request into structured JSON.

            CONTEXT:
            - Client's existing dogs: [{{dogNames}}]
            - Client's default zone: {{defaultZone}}
            - Available zones: {{zones}}
            - Available breeds: {{breeds}}
            - Available recurrence: OneTime, AllWorkingDays, EveryTwoWorkingDays, WeeklySameDay

            RULES:
            1. If a dog name is NOT in the existing dogs list, add it to "newDogs" with breed "Other" unless the user specifies a breed.
            2. If no zone/location is mentioned, use "{{defaultZone}}".
            3. If no duration is mentioned, default to 60 minutes.
            4. When specific days of week are mentioned (e.g. "Mondays and Tuesdays"), use recurrence "WeeklySameDay".
            5. When "every day" or "all working days" is mentioned, use "AllWorkingDays" with dayOfWeek null.
            6. When "every other day" is mentioned, use "EveryTwoWorkingDays" with dayOfWeek null.
            7. When a single specific event is described (e.g. "tomorrow", "next Friday"), use "OneTime" with dayOfWeek null.
            8. EXPAND all combinations: 2 days x 2 times = 4 walk entries.
            9. Use 24-hour time format "HH:mm".
            10. dayOfWeek must be one of: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday, or null.
            11. Match zone names as closely as possible to the available list. If no match, use the default zone.

            Return ONLY valid JSON with this schema:
            {
              "newDogs": [{ "name": "string", "breed": "string" }],
              "walks": [{ "dogName": "string", "dayOfWeek": "string|null", "time": "HH:mm", "durationMinutes": number, "location": "string", "recurrence": "OneTime|AllWorkingDays|EveryTwoWorkingDays|WeeklySameDay" }]
            }
            """;
    }

    private static AiParsedWalkRequest ParseResponse(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()
            ?? throw new InvalidOperationException("Empty AI response.");

        var parsed = JsonSerializer.Deserialize<AiRawResponse>(content, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse AI response JSON.");

        var newDogs = (parsed.NewDogs ?? [])
            .Select(d => new AiParsedDog(d.Name ?? "Unknown", d.Breed ?? "Other"))
            .ToList();

        var walks = (parsed.Walks ?? [])
            .Select(w => new AiParsedWalk(
                w.DogName ?? "Unknown",
                ParseDayOfWeek(w.DayOfWeek),
                TimeOnly.TryParse(w.Time, out var t) ? t : new TimeOnly(9, 0),
                w.DurationMinutes > 0 ? w.DurationMinutes : 60,
                w.Location ?? "Palermo",
                ParseRecurrence(w.Recurrence)))
            .ToList();

        return new AiParsedWalkRequest(newDogs, walks);
    }

    private static DayOfWeek? ParseDayOfWeek(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Enum.TryParse<DayOfWeek>(value, ignoreCase: true, out var dow) ? dow : null;
    }

    private static RecurrenceType ParseRecurrence(string? value) => value switch
    {
        "AllWorkingDays" => RecurrenceType.AllWorkingDays,
        "EveryTwoWorkingDays" => RecurrenceType.EveryTwoWorkingDays,
        "WeeklySameDay" => RecurrenceType.WeeklySameDay,
        _ => RecurrenceType.OneTime
    };

    private static string TryExtractErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("error").GetProperty("message").GetString() ?? json;
        }
        catch { return json; }
    }

    // ── JSON deserialization models ─────────────────────────────

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class AiRawResponse
    {
        [JsonPropertyName("newDogs")]
        public List<AiRawDog>? NewDogs { get; set; }

        [JsonPropertyName("walks")]
        public List<AiRawWalk>? Walks { get; set; }
    }

    private sealed class AiRawDog
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("breed")]
        public string? Breed { get; set; }
    }

    private sealed class AiRawWalk
    {
        [JsonPropertyName("dogName")]
        public string? DogName { get; set; }

        [JsonPropertyName("dayOfWeek")]
        public string? DayOfWeek { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("durationMinutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("recurrence")]
        public string? Recurrence { get; set; }
    }
}
