using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;

public class PointJsonConverter : JsonConverter<Point>
{
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        int x = 0;
        int y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Point(x, y);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString() ?? string.Empty;
                reader.Read();
                switch (propertyName)
                {
                    case "X":
                        x = reader.GetInt32();
                        break;
                    case "Y":
                        y = reader.GetInt32();
                        break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}