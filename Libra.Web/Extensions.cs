using Libra.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Libra.Web
{
    public static class Extensions
    {
        public static string Translate(this HtmlHelper helper, string key)
        {
            return DependencyResolver.Current.GetService<ITranslationProvider>()
                .GetString(key);
        }
        public static bool IsInRole(this HtmlHelper helper, IEnumerable<Role> roles)
        {
            return roles.Any(r => User.Current.IsInRole(r));
        }
        public static bool IsInRole(this HtmlHelper helper, Role role)
        {
            return User.Current.IsInRole(role);
        }
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }

        public static ActionResult ToJsonResult(this OperationResult value)
        {
            return new ContentResult
            {
                Content = ToJson(value)
            };
        }

        public static TModel ParseJson<TModel>(this string model)
        {
            return JsonConvert.DeserializeObject<TModel>(
                model, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }

        public static T Cast<T>(this string value, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            when (ex is InvalidCastException
                || ex is FormatException
                || ex is OverflowException
                || ex is ArgumentException)
            {
                return defaultValue;
            }
        }

        public static string Classifier<TEnum>(this HtmlHelper helper, bool nullable = false)
        {
            List<int> listOfUserProducts = User.Current.ProductGroup?.ToList() ?? new List<int>(); //.Split(',').Select(Int32.Parse).ToList();
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"Type '{typeof(TEnum).Name}' must be an enum");
            }
            var list = Enum.GetValues(typeof(TEnum))
                .OfType<TEnum>()
                .Select(e => new ClassifierItem
                {
                    Key = Convert.ToInt32(e),
                    Value = DependencyResolver.Current.GetService<ITranslationProvider>().GetString($"{typeof(TEnum).Name}_{e.ToString()}")
                })/*.Where(x => listOfUserProducts.Contains((int)x.Key))*/
                .ToList();

            if (nullable)
            {
                list.Insert(0, new ClassifierItem
                {
                    Key = null, 
                    Value = "-"
                });
            }
            return list.ToJson();
        }

        public static string Classifier(this HtmlHelper helper, Dictionary<int?, string> list, bool nullable = false)
        {
            var itemList = new List<ClassifierItem>();

            itemList.AddRange(list.Select(e => new ClassifierItem
                    {
                        Key = Convert.ToInt32(e.Key),
                        Value = e.Value//DependencyResolver.Current.GetService<ITranslationProvider>().GetString(e.Value)
                    })
                    .ToList());

            if (nullable)
            {
                itemList.Insert(0, new ClassifierItem
                {
                    Key = null,
                    Value = "-"
                });
            }
            return itemList.ToJson();
        }
    }

    public class ClassifierItem
    {
        public int? Key { get; set; }
        public string Value { get; set; }
    }
}