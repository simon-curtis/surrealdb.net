using Microsoft.Spatial;
using SurrealDb.Net.Internals.Constants;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SurrealDb.Net.Internals.Json.Converters.Spatial;

internal class GeographyMultiPolygonValueConverter : JsonConverter<GeographyMultiPolygon>
{
    public override GeographyMultiPolygon? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.None || reader.TokenType == JsonTokenType.Null)
            return default;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty(SpatialConverterConstants.TypePropertyName, out var typeProperty))
        {
            var type = typeProperty.GetString();

            if (type == MultiPolygonConverter.TypeValue)
            {
                var coordinatesProperty = root.GetProperty(
                    SpatialConverterConstants.CoordinatesPropertyName
                );

                if (coordinatesProperty.ValueKind != JsonValueKind.Array)
                    throw new JsonException(
                        $"Cannot deserialize {nameof(GeographyMultiPolygon)} because coordinates must be an array"
                    );

                var geographyBuilder = GeographyFactory.MultiPolygon();

                MultiPolygonConverter.ConstructGeographyMultiPolygon(
                    ref coordinatesProperty,
                    geographyBuilder
                );

                return geographyBuilder.Build();
            }

            throw new JsonException(
                $"Cannot deserialize {nameof(GeographyMultiPolygon)} because of type \"{type}\""
            );
        }

        throw new JsonException($"Cannot deserialize {nameof(GeographyMultiPolygon)}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        GeographyMultiPolygon value,
        JsonSerializerOptions options
    )
    {
        MultiPolygonConverter.WriteGeographyMultiPolygon(writer, value);
    }
}
