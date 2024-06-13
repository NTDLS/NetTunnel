using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine;
using System.Text;
using System.Text.RegularExpressions;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service
{
    public static class HttpUtility
    {
        public static readonly List<string> HttpVerbStrings = new()
        {
            "connect","delete","get","head","options","patch","post","put","trace"
        };

        public enum HTTPHeaderResult
        {
            WaitOnData,
            Present,
            NotPresent
        }

        public static HTTPHeaderResult Process(ref StringBuilder httpHeaderBuilder, INtEndpointConfiguration endpointConfig, PumpBuffer buffer)
        {
            try
            {
                if (httpHeaderBuilder.Length > 0) //We are reconstructing a fragmented HTTP request header.
                {
                    var stringContent = Encoding.UTF8.GetString(buffer.Bytes, 0, buffer.Length);
                    httpHeaderBuilder.Append(stringContent);
                }
                else if (StartsWithHTTPVerb(buffer.Bytes))
                {
                    var stringContent = Encoding.UTF8.GetString(buffer.Bytes, 0, buffer.Length);
                    httpHeaderBuilder.Append(stringContent);
                }

                string headerDelimiter = string.Empty;

                if (httpHeaderBuilder.Length > 0)
                {
                    var headerType = IsHttpHeader(httpHeaderBuilder.ToString(), out string? requestVerb);

                    //TODO: We should throw an exception if this is an inbound endpoint and the headerType == NtHttpHeaderType.Response
                    //TODO: We should throw an exception if this is an outbound endpoint and the headerType == NtHttpHeaderType.Request

                    if (headerType != NtHttpHeaderType.None && requestVerb != null)
                    {
                        var endOfHeaderIndex = GetHttpHeaderEnd(httpHeaderBuilder.ToString(), out headerDelimiter);
                        if (endOfHeaderIndex < 0)
                        {
                            return HTTPHeaderResult.WaitOnData; //We have a HTTP header but its a fragment. Wait on the remaining header.
                        }
                        else
                        {
                            if (buffer.Length > headerDelimiter.Length * 2) // "\r\n" or "\r\n\r\n"
                            {
                                //If we received more bytes than just the delimiter then we
                                //  need to determine how many non-header bytes need to be sent to the peer.

                                int endOfHeaderInBufferIndex = FindDelimiterIndexInByteArray(buffer.Bytes, buffer.Length, $"{headerDelimiter}{headerDelimiter}");
                                if (endOfHeaderInBufferIndex < 0)
                                {
                                    throw new Exception("Could not locate HTTP header in receive buffer.");
                                }

                                int bufferEndOfHeaderOffset = endOfHeaderInBufferIndex + (headerDelimiter.Length * 2);

                                if (bufferEndOfHeaderOffset > buffer.Length)
                                {
                                    //We received extra non-header bytes. We need to remove the header bytes from the buffer
                                    //  and then send them after we modify and send the header.
                                    int newBufferLength = buffer.Length - bufferEndOfHeaderOffset;
                                    Array.Copy(buffer.Bytes, bufferEndOfHeaderOffset, buffer.Bytes, 0, newBufferLength);
                                }

                                buffer.Length -= bufferEndOfHeaderOffset;
                            }
                            else
                            {
                                buffer.Length = 0;
                            }
                        }

                        //Apply the header rules, replacing the original http header builder.
                        httpHeaderBuilder = new StringBuilder(
                            ApplyHttpHeaderRules(endpointConfig, httpHeaderBuilder.ToString(), headerType, requestVerb, headerDelimiter));

                        return HTTPHeaderResult.Present;
                    }
                }
            }
            catch (Exception ex)
            {
                httpHeaderBuilder.Clear();
                Singletons.Core.Logging.Write(ex, "An error occurred while parsing the HTTP request header.");
            }

            return HTTPHeaderResult.NotPresent;
        }
        public static int FindDelimiterIndexInByteArray(byte[] buffer, int bufferLength, string delimiter)
        {
            for (int bufIdx = 0; bufIdx <= bufferLength - delimiter.Length; bufIdx++)
            {
                bool found = true;
                for (int delIdx = 0; delIdx < delimiter.Length; delIdx++)
                {
                    if (buffer[bufIdx + delIdx] != delimiter[delIdx])
                    {
                        found = false;
                    }
                }
                if (found)
                {
                    return bufIdx;
                }
            }
            return -1;
        }

        public static string ApplyHttpHeaderRules(INtEndpointConfiguration endpointConfig,
            string httpHeader, NtHttpHeaderType headerType, string httpRequestVerb, string headerDelimiter)
        {
            try
            {
                var availableRules = endpointConfig.HttpHeaderRules.Where(o =>
                                      (o.HeaderType == headerType || o.HeaderType == NtHttpHeaderType.Any)
                                      && (o.Verb.ToString().Equals(httpRequestVerb, StringComparison.CurrentCultureIgnoreCase) || o.Verb == NtHttpVerb.Any)
                                      && o.Enabled == true
                                      ).ToList();

                foreach (var rule in availableRules)
                {
                    if (rule.Action == NtHttpHeaderAction.Upsert)
                    {
                        httpHeader = UpsertHttpHostHeaderValue(httpHeader, rule.Name, rule.Value, headerDelimiter);
                    }
                    else if (rule.Action == NtHttpHeaderAction.Update)
                    {
                        httpHeader = UpdateHttpHostHeaderValue(httpHeader, rule.Name, rule.Value, headerDelimiter);
                    }
                    else if (rule.Action == NtHttpHeaderAction.Insert)
                    {
                        httpHeader = InsertHttpHostHeaderValue(httpHeader, rule.Name, rule.Value, headerDelimiter);
                    }
                    else if (rule.Action == NtHttpHeaderAction.Delete)
                    {
                        httpHeader = DeleteHttpHostHeaderValue(httpHeader, rule.Name, headerDelimiter);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                Singletons.Core.Logging.Write(ex, "Failed to process HTTP Header rules.");
            }

            return httpHeader;
        }

        public static bool StartsWithHTTPVerb(byte[] bytes)
        {
            var possibleHttpHeader = Encoding.UTF8.GetString(bytes.Take(10).ToArray()).ToLower();

            foreach (var verb in HttpVerbStrings)
            {
                if (possibleHttpHeader.StartsWith(verb))
                {
                    if (possibleHttpHeader.Length > verb.Length)
                    {
                        return char.IsWhiteSpace(possibleHttpHeader[verb.Length]);
                    }
                }
            }

            return false;
        }

        public static string? GetNextHeaderToken(string str, ref int position)
        {
            int spacePos = str.IndexOfAny(new char[] { ' ', '\n' }, position);
            if (spacePos >= position)
            {
                string token = str.Substring(position, spacePos - position).Trim();
                position = spacePos + 1;
                return token;
            }

            return null;
        }

        public static bool IsHttpVerb(string? str)
        {
            str = str?.ToUpper();

            var verbs = Enum.GetValues(typeof(NtHttpVerb));

            foreach (var verb in verbs)
            {
                if (str == verb?.ToString()?.ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        public static NtHttpHeaderType IsHttpHeader(byte[]? data, int length, out string? verb)
        {
            if (data == null)
            {
                return IsHttpHeader(string.Empty, out verb);
            }

            int maxScanLength = length > 2048 ? 2048 : length;
            string httpHeader = Encoding.UTF8.GetString(data, 0, maxScanLength);
            return IsHttpHeader(httpHeader, out verb);
        }

        public static NtHttpHeaderType IsHttpHeader(string httpHeader, out string? verb)
        {
            int firstLineBreak = httpHeader.IndexOf('\n') + 1;
            if (firstLineBreak > 1)
            {
                httpHeader = httpHeader.Substring(0, firstLineBreak).ToUpper();

                int headerTokPos = 0;
                var token = GetNextHeaderToken(httpHeader, ref headerTokPos);

                if (httpHeader == null)
                {
                    //Not much to do...
                }
                else if (httpHeader.StartsWith("Http/"))
                {
                    //Is response header.
                    verb = string.Empty;
                    return NtHttpHeaderType.Response;
                }
                else if (IsHttpVerb(token))
                {
                    //Is (potential) response header.

                    verb = token;

                    while (1 == 1)
                    {
                        token = GetNextHeaderToken(httpHeader, ref headerTokPos);
                        if (token == null)
                        {
                            verb = string.Empty;
                            return NtHttpHeaderType.None;
                        }

                        if (token.ToUpper().StartsWith("HTTP/"))
                        {
                            //Is request header
                            return NtHttpHeaderType.Request;
                        }
                    }
                }

            }

            verb = string.Empty;
            return NtHttpHeaderType.None;
        }

        /// <summary>
        /// Returns the index of the end of the Http header
        /// </summary>
        /// <param name="header"></param>
        /// <param name="delimiter">Returns the delimiter type (CrLf or Lf)</param>
        /// <returns></returns>
        public static int GetHttpHeaderEnd(string header, out string delimiter)
        {
            int crlf = header.IndexOf("\r\n\r\n");
            int lf = header.IndexOf("\n\n");
            int endOfHeader = 0;

            if (crlf > 0 && (crlf < lf || lf < 0))
            {
                delimiter = "\r\n";
                endOfHeader = crlf + 4;
            }
            else if (lf > 0 && (lf < crlf || crlf < 0))
            {
                delimiter = "\n";
                endOfHeader = lf + 2;
            }
            else
            {
                delimiter = "";
                return -1;
            }

            return endOfHeader;
        }

        /// <summary>
        /// Adds a new Http header value if it does not already exist.
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="lineBreak"></param>
        /// <returns></returns>
        public static string InsertHttpHostHeaderValue(string headerText, string name, string value, string lineBreak)
        {
            var fieldFinder = new Regex(@"(?i:" + name + @")\:.*");
            var existingMatch = fieldFinder.Match(headerText);

            string newFieldValue = string.Format("{0}: {1}", name, value);

            if (existingMatch != null)
            {
                ///Do nothing.               
            }
            else
            {
                headerText += newFieldValue + lineBreak;
            }

            return headerText;
        }

        /// <summary>
        /// Adds a new Http header value or updates it if it already exists.
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="lineBreak"></param>
        /// <returns></returns>
        public static string UpsertHttpHostHeaderValue(string headerText, string name, string value, string lineBreak)
        {
            var fieldFinder = new Regex(@"(?i:" + name + @")\:.*");
            var existingMatch = fieldFinder.Match(headerText);

            string newFieldValue = string.Format("{0}: {1}", name, value);

            if (existingMatch != null)
            {
                headerText = fieldFinder.Replace(headerText, newFieldValue);
            }
            else
            {
                headerText += newFieldValue + lineBreak;
            }

            return headerText;
        }

        /// <summary>
        /// Adds a new Http header value or updates it if it already exists.
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="lineBreak"></param>
        /// <returns></returns>
        public static string UpdateHttpHostHeaderValue(string headerText, string name, string value, string lineBreak)
        {
            var fieldFinder = new Regex(@"(?i:" + name + @")\:.*");
            var existingMatch = fieldFinder.Match(headerText);

            string newFieldValue = string.Format("{0}: {1}", name, value);

            if (existingMatch != null)
            {
                headerText = fieldFinder.Replace(headerText, newFieldValue);
            }
            else
            {
                //Do nothing.
            }

            return headerText;
        }

        /// <summary>
        /// Removes a Http header value if it exists, otherwise does nothing.
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="name"></param>
        /// <param name="lineBreak"></param>
        /// <returns></returns>
        public static string DeleteHttpHostHeaderValue(string headerText, string name, string lineBreak)
        {
            var fieldFinder = new Regex(@"(?i:" + name + @")\:.*");
            var existingMatch = fieldFinder.Match(headerText);

            if (existingMatch != null)
            {
                headerText = fieldFinder.Replace(headerText, string.Empty);
            }

            return headerText;
        }
    }
}
