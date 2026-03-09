using System.Data;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EcoTrack.Application.DTOs;
using EcoTrack.Application.Interfaces;
using EcoTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Npgsql;

namespace EcoTrack.Infrastructure.Services;

public class AiExtractorService : IAiExtractorService
{
    private const int EmbeddingDimensions = 1536;

    private readonly EcoTrackDbContext _dbContext;
    private readonly ILogger<AiExtractorService> _logger;
    private readonly HttpClient _httpClient = new();
    private readonly Kernel? _kernel;
    private readonly string _provider;
    private readonly string _ollamaBaseUrl;
    private readonly string _ollamaModel;
    private readonly string _connectionString;

    public AiExtractorService(
        EcoTrackDbContext dbContext,
        IConfiguration configuration,
        ILogger<AiExtractorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _provider = configuration["Ai:Provider"]?.Trim().ToLowerInvariant() ?? "none";
        _ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _ollamaModel = configuration["Ollama:Model"] ?? "llama3.2";
        _connectionString = configuration.GetConnectionString("EcoTrackDatabase")
            ?? throw new InvalidOperationException("Connection string 'EcoTrackDatabase' not found.");
        _kernel = BuildKernel(configuration, logger, _provider);
    }

    public async Task<EmissionDto> ExtractEmissionAsync(string rawText)
    {
        var extracted = await ExtractAndMapAsync(rawText, CancellationToken.None);

        return new EmissionDto
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.Empty,
            Category = extracted.MatchedCategoryName ?? extracted.Category,
            Amount = extracted.Amount,
            Co2Equivalent = 0,
            ReportDate = DateTimeOffset.UtcNow
        };
    }

    public async Task<UnstructuredEmissionExtractionDto> ExtractAndMapAsync(string rawText, CancellationToken cancellationToken = default)
    {
        var parsed = await ExtractStructuredAsync(rawText, cancellationToken);

        await EnsureCategoryEmbeddingsAsync(cancellationToken);
        var match = await FindClosestCategoryAsync(parsed.Category, cancellationToken);

        return new UnstructuredEmissionExtractionDto
        {
            Activity = parsed.Activity,
            Amount = parsed.Amount,
            Unit = parsed.Unit,
            Category = parsed.Category,
            MatchedCategoryId = match?.CategoryId,
            MatchedCategoryName = match?.CategoryName,
            SimilarityScore = match?.SimilarityScore ?? 0
        };
    }

    private async Task<ParsedExtraction> ExtractStructuredAsync(string rawText, CancellationToken cancellationToken)
    {
        if (_provider == "ollama")
            return await ExtractWithOllamaAsync(rawText, cancellationToken);

        if (_kernel is null)
        {
            _logger.LogWarning("AI provider is not configured. Using deterministic extraction fallback.");
            return DeterministicParse(rawText);
        }

        const string prompt = """
You are an extraction engine for carbon accounting records.
Extract data from the user note and return ONLY valid JSON (no markdown, no extra text).
Output schema:
{
  "activity": "string",
  "amount": number,
  "unit": "string",
  "category": "string"
}
Rules:
- activity: concise human-readable activity description.
- amount: numeric value only.
- unit: standardized unit (e.g. kWh, km, m3, ton, flight).
- category: normalized emission category candidate (e.g. Electricity, Business travel, Natural gas, Transport).
- If missing value, infer best-effort but keep JSON valid.

Input:
{{$input}}
""";

        try
        {
            var arguments = new KernelArguments
            {
                ["input"] = rawText
            };

            var completion = await _kernel.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);
            var raw = completion.ToString();
            var json = UnwrapJson(raw);

            var parsed = JsonSerializer.Deserialize<ParsedExtraction>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return NormalizeParsed(parsed, rawText);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic extraction failed. Using deterministic fallback for input: {RawText}", rawText);
            return DeterministicParse(rawText);
        }
    }

    private async Task<ParsedExtraction> ExtractWithOllamaAsync(string rawText, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new OllamaGenerateRequest
            {
                Model = _ollamaModel,
                Prompt = BuildPrompt().Replace("{{$input}}", rawText, StringComparison.Ordinal),
                Stream = false,
                Format = "json"
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_ollamaBaseUrl.TrimEnd('/')}/api/generate",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);
            var json = UnwrapJson(result?.Response);
            var parsed = JsonSerializer.Deserialize<ParsedExtraction>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return NormalizeParsed(parsed, rawText);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama extraction failed. Using deterministic fallback for input: {RawText}", rawText);
            return DeterministicParse(rawText);
        }
    }

    private static string BuildPrompt() =>
        """
You are an extraction engine for carbon accounting records.
Extract data from the user note and return ONLY valid JSON (no markdown, no extra text).
Output schema:
{
  "activity": "string",
  "amount": number,
  "unit": "string",
  "category": "string"
}
Rules:
- activity: concise human-readable activity description.
- amount: numeric value only.
- unit: standardized unit (e.g. kWh, km, m3, ton, flight).
- category: normalized emission category candidate (e.g. Electricity, Business travel, Natural gas, Transport).
- If missing value, infer best-effort but keep JSON valid.

Input:
{{$input}}
""";

    private static ParsedExtraction NormalizeParsed(ParsedExtraction? parsed, string rawText)
    {
        if (parsed is null)
            return DeterministicParse(rawText);

        return new ParsedExtraction
        {
            Activity = string.IsNullOrWhiteSpace(parsed.Activity) ? rawText : parsed.Activity.Trim(),
            Amount = parsed.Amount <= 0 ? DeterministicParse(rawText).Amount : parsed.Amount,
            Unit = string.IsNullOrWhiteSpace(parsed.Unit) ? "unit" : parsed.Unit.Trim(),
            Category = string.IsNullOrWhiteSpace(parsed.Category) ? GuessCategory(rawText) : parsed.Category.Trim()
        };
    }

    private async Task EnsureCategoryEmbeddingsAsync(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.EmissionCategories.AsNoTracking().ToListAsync(cancellationToken);
        if (categories.Count == 0)
            return;

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            foreach (var category in categories)
            {
                var embeddingText = ToPgVectorLiteral(CreateDeterministicEmbedding($"{category.Name} {category.Description}"));

                await using var command = new NpgsqlCommand(
                    """
                    UPDATE "EmissionCategories"
                    SET "Embedding" = CAST(@embedding AS vector)
                    WHERE "Id" = @id AND "Embedding" IS NULL
                    """,
                    connection);

                command.Parameters.Add(new NpgsqlParameter("id", category.Id));
                command.Parameters.Add(new NpgsqlParameter("embedding", embeddingText));
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Embedding initialization failed. Continuing without vector search.");
        }
    }

    private async Task<CategoryMatch?> FindClosestCategoryAsync(string categoryText, CancellationToken cancellationToken)
    {
        try
        {
            var queryEmbedding = ToPgVectorLiteral(CreateDeterministicEmbedding(categoryText));

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(
                """
                SELECT "Id", "Name", ("Embedding" <-> CAST(@embedding AS vector)) AS distance
                FROM "EmissionCategories"
                WHERE "Embedding" IS NOT NULL
                ORDER BY "Embedding" <-> CAST(@embedding AS vector)
                LIMIT 1
                """,
                connection);

            command.Parameters.Add(new NpgsqlParameter("embedding", queryEmbedding));

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var distance = reader.GetDouble(2);
                return new CategoryMatch(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    Math.Max(0, 1 - distance));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vector category search failed. Falling back to name-based search.");
        }

        var fallback = await _dbContext.EmissionCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Name, $"%{categoryText}%"), cancellationToken);

        return fallback is null ? null : new CategoryMatch(fallback.Id, fallback.Name, 0.5);
    }

    private static string UnwrapJson(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return "{}";

        var trimmed = response.Trim();

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            if (start >= 0 && end > start)
                return trimmed[start..(end + 1)];
        }

        return trimmed;
    }

    private static ParsedExtraction DeterministicParse(string rawText)
    {
        var amountMatch = Regex.Match(rawText, @"(\d+(?:[\.,]\d+)?)");
        var amount = amountMatch.Success
            ? decimal.Parse(amountMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)
            : 1m;

        return new ParsedExtraction
        {
            Activity = rawText,
            Amount = amount,
            Unit = GuessUnit(rawText),
            Category = GuessCategory(rawText)
        };
    }

    private static string GuessUnit(string text)
    {
        var normalized = text.ToLowerInvariant();
        if (normalized.Contains("kwh")) return "kWh";
        if (normalized.Contains("km")) return "km";
        if (normalized.Contains("m3")) return "m3";
        if (normalized.Contains("flight") || normalized.Contains("let")) return "flight";
        return "unit";
    }

    private static string GuessCategory(string text)
    {
        var normalized = text.ToLowerInvariant();
        if (normalized.Contains("flight") || normalized.Contains("let") || normalized.Contains("trip")) return "Business travel";
        if (normalized.Contains("kwh") || normalized.Contains("electric")) return "Electricity";
        if (normalized.Contains("gas") || normalized.Contains("m3")) return "Natural gas";
        if (normalized.Contains("km") || normalized.Contains("transport")) return "Transport";
        return "General";
    }

    private static float[] CreateDeterministicEmbedding(string input)
    {
        var vector = new float[EmbeddingDimensions];
        if (string.IsNullOrWhiteSpace(input))
            return vector;

        var bytes = Encoding.UTF8.GetBytes(input.ToLowerInvariant());
        for (var i = 0; i < bytes.Length; i++)
        {
            var index = bytes[i] % EmbeddingDimensions;
            vector[index] += 1f;
        }

        var norm = Math.Sqrt(vector.Sum(x => x * x));
        if (norm <= 0) return vector;

        for (var i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / norm);

        return vector;
    }

    private static string ToPgVectorLiteral(float[] vector)
    {
        var values = vector.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return $"[{string.Join(',', values)}]";
    }

    private static Kernel? BuildKernel(IConfiguration configuration, ILogger logger, string provider)
    {
        var model = configuration["Ai:Model"] ?? "gpt-4o-mini";

        try
        {
            var builder = Kernel.CreateBuilder();

            if (provider == "azureopenai")
            {
                var endpoint = configuration["AzureOpenAI:Endpoint"];
                var apiKey = configuration["AzureOpenAI:ApiKey"];
                var deploymentName = configuration["AzureOpenAI:DeploymentName"];

                if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(deploymentName))
                {
                    builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
                    return builder.Build();
                }
            }
            else if (provider == "openai")
            {
                var apiKey = configuration["OpenAI:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    builder.AddOpenAIChatCompletion(model, apiKey);
                    return builder.Build();
                }
            }

            logger.LogWarning("Semantic Kernel provider is not fully configured. Falling back to deterministic extraction.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to initialize Semantic Kernel. Falling back to deterministic extraction.");
            return null;
        }
    }

    private sealed class ParsedExtraction
    {
        public string Activity { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    private sealed class OllamaGenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; set; }
        public string Format { get; set; } = "json";
    }

    private sealed class OllamaGenerateResponse
    {
        public string Response { get; set; } = string.Empty;
    }

    private sealed record CategoryMatch(Guid CategoryId, string CategoryName, double SimilarityScore);
}
