using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace WireFlow
{
    /// <summary>
    /// The JSON of WireFlow documents, as of .wf format 2. An element's Type ("box", "text", "rect", "group", "part",
    /// "image") chooses its class when it is read, so the JSON names no .NET types. Enums are written by name and an
    /// automatic size (NaN) as null. Every value is written, so that reading never depends on the defaults in the code.
    /// </summary>
    public static class FlowJson
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting = Formatting.Indented,
            ContractResolver = new KindFirstContractResolver(),
            Converters = { new StringEnumConverter(), new NaNAsNullConverter(), new FlowElementConverter() }
        };
    }


    /// <summary>
    /// Writes Type and Name before the other properties, so that each object shows first what it is.
    /// </summary>
    public class KindFirstContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .OrderBy(p => p.PropertyName == nameof(FlowBase.Type) ? 0 : p.PropertyName == nameof(FlowBase.Name) ? 1 : 2)
                .ToList();
        }
    }


    /// <summary>
    /// Reads an element declared as FlowElementBase (the items of layers and groups) into the class its Type names.
    /// Writing needs no converter: the element's own class is written, with its Type.
    /// </summary>
    public class FlowElementConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(FlowElementBase);

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            JObject json = JObject.Load(reader);
            FlowElementBase element = Create(json.Value<string>(nameof(FlowBase.Type)));
            using (JsonReader elementReader = json.CreateReader())
                serializer.Populate(elementReader, element);
            return element;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The element of a kind: "box", "text", "rect" and any other kind are FlowElement, which shows the kinds it does
        /// not know as unknown objects.
        /// </summary>
        public static FlowElementBase Create(string type)
        {
            switch (type)
            {
                case "group":
                    return new FlowGroup();
                case "part":
                    return new FlowPart();
                case "image":
                    return new FlowImage();
                default:
                    return new FlowElement();
            }
        }
    }


    /// <summary>
    /// Writes NaN, which means an automatic size, as null, and reads null as NaN; JSON has no NaN.
    /// </summary>
    public class NaNAsNullConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(double);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return double.NaN;
                case JsonToken.String:
                    return double.Parse((string)reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
                default:
                    return Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            double number = (double)value;
            if (double.IsNaN(number))
                writer.WriteNull();
            else
                writer.WriteValue(number);
        }
    }
}
