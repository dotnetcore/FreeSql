using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class JObjectExtensions
{
	/// <summary>
	/// 将JObject转化成字典
	/// </summary>
	/// <param name="json"></param>
	/// <returns></returns>
	public static Dictionary<string, object> ToDictionary(this JToken json)
	{
		var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
		ProcessJObjectProperties(propertyValuePairs);
		ProcessJArrayProperties(propertyValuePairs);
		return propertyValuePairs;
	}

	private static void ProcessJObjectProperties(Dictionary<string, object> propertyValuePairs)
	{
		var objectPropertyNames = (from property in propertyValuePairs
								   let propertyName = property.Key
								   let value = property.Value
								   where value is JObject
								   select propertyName).ToList();

		objectPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToDictionary((JObject)propertyValuePairs[propertyName]));
	}

	private static void ProcessJArrayProperties(Dictionary<string, object> propertyValuePairs)
	{
		var arrayPropertyNames = (from property in propertyValuePairs
								  let propertyName = property.Key
								  let value = property.Value
								  where value is JArray
								  select propertyName).ToList();

		arrayPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToArray((JArray)propertyValuePairs[propertyName]));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="array"></param>
	/// <returns></returns>
	public static object[] ToArray(this JArray array)
	{
		return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
	}

	private static object ProcessArrayEntry(object value)
	{
		if (value is JObject)
		{
			return ToDictionary((JObject)value);
		}
		if (value is JArray)
		{
			return ToArray((JArray)value);
		}
		return value;
	}

}