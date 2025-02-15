﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace task_sync_web.Commons;

public class DateFormatConverter : IsoDateTimeConverter
{
    public DateFormatConverter(string format)
    {
        DateTimeFormat = format;
    }
}

public class BoolFormatConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(bool);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var tag = new bool();

        //tag.Name = reader.Value.ToString();

        return tag;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (bool.TryParse(Convert.ToString(value), out bool val))
        {
            if (val)
                writer.WriteValue("1");
            else
                writer.WriteValue("0");
        }
        else
        {
            throw new CustomExtention(ErrorMessages.EW1201);
        }

    }
}

public class DefalultFlagConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(bool);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var tag = new bool();

        //tag.Name = reader.Value.ToString();

        return tag;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(string.Empty);
    }
}

public class TimeSpanConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(bool);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var tag = new bool();

        //tag.Name = reader.Value.ToString();

        return tag;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        try
        {
            var timespan = TimeSpan.Parse(value.ToString());
            var taskStartTime = "";
            if (timespan < TimeSpan.Zero)
                taskStartTime =  "-" + timespan.ToString(@"hh\:mm");
           else
                taskStartTime = timespan.ToString(@"hh\:mm");
            writer.WriteValue(taskStartTime);
        }
        catch (Exception)
        {
            writer.WriteValue("undefined");
        }
       
    }
}

