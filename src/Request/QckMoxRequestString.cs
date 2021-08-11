using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace QckMox.Request
{
    internal class QckMoxRequestString : IEquatable<QckMoxRequestString>
    {
        private const string REGEX_METHOD_GROUP = "method";
        private const string REGEX_RESOURCE_GROUP = "resource";
        private const string REGEX_PARAMETERS_GROUP = "parameters";

        private static readonly string _parserMethodRegex = "[A-Za-z]+";
        private static readonly string _parserResourceRegex =   @"[\w\-\.\%\\\/]+";
        private static readonly string _parserParametersRegex = @"[\w\-\.\%\=\&\@\!\$\'\(\)\+\,\;\~]+";

        private static readonly string _parserMethodGroup = $"(?'{REGEX_METHOD_GROUP}'{_parserMethodRegex})";
        private static readonly string _parserResourceGroup = $"(?'{REGEX_RESOURCE_GROUP}'{_parserResourceRegex})";
        private static readonly string _parserParametersGroup = $@"(?'{REGEX_PARAMETERS_GROUP}'{_parserParametersRegex})";

        private static readonly Regex _parserRegex = new Regex($@"^[ ]*{_parserMethodGroup}([ ]+{_parserResourceGroup})?([ ]+{_parserParametersGroup})?[ ]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public QckMoxRequestString()
        {
            Queries = new Dictionary<string, StringValues>(new Dictionary<string, StringValues>(), StringComparer.OrdinalIgnoreCase);
            Headers = new Dictionary<string, StringValues>(new Dictionary<string, StringValues>(), StringComparer.OrdinalIgnoreCase);
        }

        public QckMoxRequestString(string requestString, string queryTag = QckMoxRequestConfig.DEFAULT_QUERY_TAG, string headerTag = QckMoxRequestConfig.DEFAULT_HEADER_TAG) : this()
        {
            var parsed = Parse(requestString, queryTag: queryTag, headerTag: headerTag);
            Method = parsed.Method;
            Resource = parsed.Resource;
            Queries = parsed.Queries;
            Headers = parsed.Headers;
        }

        public string Method { get; set; }
        public string Resource { get; set; }
        public IDictionary<string, StringValues> Queries { get; private set; }
        public IDictionary<string, StringValues> Headers { get; private set; }

        public override string ToString()
        {
            return ToString(false, false);
        }

        public override bool Equals(object obj)
        {
            return AreEqual(this, obj as QckMoxRequestString);
        }

        public bool Equals(QckMoxRequestString obj)
        {
            return AreEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static implicit operator string(QckMoxRequestString value) => value.ToString();

        public static bool operator ==(QckMoxRequestString obj1, QckMoxRequestString obj2)
        {
            return AreEqual(obj1, obj2);
        }

        public static bool operator !=(QckMoxRequestString obj1, QckMoxRequestString obj2)
        {
            return AreEqual(obj1, obj2) is false;
        }

        public string ToString(bool excludeResource = false, bool excludeParameters = false)
        {
            var resourcePart = excludeResource is true ? null : Resource;
            var parameterPart = excludeParameters is true ? null : GetParameterPart(this);

            var value = JoinUsableValues(' ', Method, resourcePart, parameterPart);
            return value.Trim();
        }

        public static QckMoxRequestString Parse(string value, string queryTag = QckMoxRequestConfig.DEFAULT_QUERY_TAG, string headerTag = QckMoxRequestConfig.DEFAULT_HEADER_TAG)
        {
            try
            {
                var match = _parserRegex.Match(value);
                if(match.Success is false) { throw new ArgumentException("Unrecognised format."); }

                var parsed = new QckMoxRequestString();

                if(match.Groups.TryGetValue(REGEX_METHOD_GROUP, out var method) && method.Success is true)
                {
                    parsed.Method = method.Value;
                }

                var hasResource = match.Groups.TryGetValue(REGEX_RESOURCE_GROUP, out var resource) && resource.Success is true;
                var hasParameters = match.Groups.TryGetValue(REGEX_PARAMETERS_GROUP, out var parameters) && parameters.Success is true;
                var resourceValue = resource?.Value;
                var parametersValue = parameters?.Value;

                var resourceMatchesQueryPrefix = string.IsNullOrWhiteSpace(queryTag) is false && resourceValue?.StartsWith(queryTag, StringComparison.OrdinalIgnoreCase) is true;
                var resourceMatchesHeaderPrefix = string.IsNullOrWhiteSpace(headerTag) is false && resourceValue?.StartsWith(headerTag, StringComparison.OrdinalIgnoreCase) is true;
                var resourceMatchesPrefix = resourceMatchesQueryPrefix || resourceMatchesHeaderPrefix;
                var useResourceAsParameter = hasResource is true && hasParameters is false && resourceMatchesPrefix is true;
                if (useResourceAsParameter is true)
                {
                    parametersValue = resourceValue;
                    hasParameters = true;
                    hasResource = false;
                }

                if(hasResource is true)
                {
                    parsed.Resource = resourceValue;
                }

                if(hasParameters is true)
                {
                    var items = parametersValue.Split('&', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    var queries = ParseParameters(items, queryTag, headerTag);
                    var headers = ParseParameters(items, headerTag, queryTag);

                    parsed.Queries = queries;
                    parsed.Headers = headers;
                }

                return parsed;
            }
            catch(Exception e)
            {
                throw new FormatException("Unable to parse string.", e);
            }
        }

        public static bool TryParse(string value, out QckMoxRequestString result, string queryTag = QckMoxRequestConfig.DEFAULT_QUERY_TAG, string headerTag = QckMoxRequestConfig.DEFAULT_HEADER_TAG)
        {
            try
            {
                result = Parse(value, queryTag: queryTag, headerTag: headerTag);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        private static string GetParameterPart(QckMoxRequestString value)
        {
            if(value?.Queries?.Any() is not true && value?.Headers?.Any() is not true)
            {
                return null;
            }

            var parameters = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            var queries = value?.Queries ?? new Dictionary<string, StringValues>();
            foreach(var item in queries)
            {
                var parameter = GetParameterItem(item.Key, item.Value);
                parameters.Add(parameter);
            }

            var headers = value?.Headers ?? new Dictionary<string, StringValues>();
            foreach(var item in headers)
            {
                var parameter = GetParameterItem(item.Key, item.Value);
                parameters.Add(parameter);
            }

            return JoinUsableValues('&', parameters);
        }

        private static string JoinUsableValues(char separator, IEnumerable<string> values)
        {
            return JoinUsableValues(separator, values?.ToArray());
        }

        private static string JoinUsableValues(char separator, params string[] values)
        {
            var queue = new Queue<string>();
            values = values ?? new string[0];

            foreach(var value in values)
            {
                if (string.IsNullOrWhiteSpace(value) is true) { continue; }

                queue.Enqueue(value.Trim());
            }

            return string.Join(separator, queue);
        }

        private static string GetParameterItem(string key, StringValues value)
        {
            if (string.IsNullOrWhiteSpace(key)) { return null; }

            var parameterKey = key?.Trim();
            var parameterValue = StringValues.IsNullOrEmpty(value) is true || string.IsNullOrWhiteSpace($"{value}") is true
                ? string.Empty
                : $"={$"{value}".Trim()}";

            var parameter = $"{parameterKey}{parameterValue}";
            return string.IsNullOrWhiteSpace(parameter) is true
                ? null
                : parameter;
        }

        private static IDictionary<string, StringValues> ParseParameters(IReadOnlyCollection<string> parameters, string tag, string tagToExclude)
        {
            var parsed = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            foreach(var item in parameters)
            {
                if(string.IsNullOrWhiteSpace(tag) is false
                    && item?.StartsWith(tag) is not true)
                {
                    continue;
                }

                if(string.Equals(tag, tagToExclude, StringComparison.OrdinalIgnoreCase) is false
                    && string.IsNullOrWhiteSpace(tagToExclude) is false
                    && item?.StartsWith(tagToExclude) is true)
                {
                    continue;
                }

                var parameter = ParseParameter(item);
                if (parameter.Key is null) { continue; }
                if (parsed.ContainsKey(parameter.Key) is true)
                {
                    var origParameter = parsed[parameter.Key];
                    var sorted = new SortedSet<string>(origParameter, StringComparer.OrdinalIgnoreCase);
                    sorted.Add(parameter.Value);
                    var newParameter = new StringValues(sorted.ToArray());
                    parsed[parameter.Key] = newParameter;
                }
                else
                {
                    parsed.Add(parameter.Key, parameter.Value);
                }
            }

            return parsed;
        }

        private static KeyValuePair<string, string> ParseParameter(string parameter)
        {
            var values = parameter?.Split('=', StringSplitOptions.TrimEntries) ?? new string[0];

            if(values.Length == 1)
            {
                return new KeyValuePair<string, string>(values[0], null);
            }

            if(values.Length == 2)
            {
                return new KeyValuePair<string, string>(values[0], values[1]);
            }

            if(values.Length > 2)
            {
                return new KeyValuePair<string, string>(values[0], values[1]);
            }

            return new KeyValuePair<string, string>(null, null);
        }

        private static bool AreEqual(QckMoxRequestString left, QckMoxRequestString right)
        {
            if (left is null && right is null) { return true; }
            if (left is null || right is null) { return false; }
            if (Object.ReferenceEquals(left, right)) { return true; }

            if (string.Equals(left.Method, right.Method, StringComparison.OrdinalIgnoreCase) is false) { return false; }

            var normalisedLeft = left.Resource?.Replace('\\', '/');
            var normalisedRight = right.Resource?.Replace('\\', '/');
            if (string.Equals(normalisedLeft, normalisedRight, StringComparison.OrdinalIgnoreCase) is false) { return false; }

            if (AreItemsEqual(left.Queries, right.Queries) is false) { return false; }
            if (AreItemsEqual(left.Headers, right.Headers) is false) { return false; }

            return true;
        }

        private static bool AreItemsEqual(IDictionary<string, StringValues> left, IDictionary<string, StringValues> right)
        {
            if (left is null && right is null) { return true; }
            if (left is null || right is null) { return false; }
            if (left.Count != right.Count) { return false; }

            var caseInsensitiveList = new Dictionary<string, StringValues>(left, StringComparer.OrdinalIgnoreCase);
            foreach(var item in right)
            {
                // keys are case insensitive
                if (caseInsensitiveList.ContainsKey(item.Key) is false) { return false; }

                // values are case sensitive
                if (AreItemsEqual(caseInsensitiveList[item.Key], item.Value) is false) { return false; }
            }

            return true;
        }

        private static bool AreItemsEqual(IReadOnlyCollection<string> left, IReadOnlyCollection<string> right)
        {
            if (left is null && right is null) { return true; }
            if (left is null || right is null) { return false; }
            if (left.Count != right.Count) { return false; }

            foreach(var item in right)
            {
                // values are case sensitive
                if (left.Contains(item) is false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}