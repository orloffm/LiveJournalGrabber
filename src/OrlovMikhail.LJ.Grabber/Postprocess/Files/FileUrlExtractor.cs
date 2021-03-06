﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OrlovMikhail.LJ.Grabber.PostProcess.Files
{
    public class FileUrlExtractor : IFileUrlExtractor
    {
        private const string Pattern = @"(?<=\s*(?i)(?:src)\s*=\s*[""']?)([^'"">\s]+)(?=[""']?)";
        private readonly Regex _regex;

        public FileUrlExtractor()
        {
            _regex = new Regex(Pattern);
        }

        public string[] GetImagesURLs(string html)
        {
            var ret = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            MatchCollection mc = _regex.Matches(html);
            foreach (Match m in mc)
            {
                string url = m.Groups[0]
                    .Value;
                bool isAnImage = Tools.Tools.IsAnImage(url);
                if (isAnImage)
                {
                    ret.Add(url);
                }
            }

            return ret.ToArray();
        }

        public string ReplaceFileUrls(string html, Func<string, string> matcher)
        {
            var replacees = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            MatchEvaluator me = _m =>
            {
                string url = _m.Groups[0]
                    .Value;
                string replacee;
                if (!replacees.TryGetValue(url, out replacee))
                {
                    replacee = matcher(url);
                    replacees[url] = replacee;
                }

                if (string.IsNullOrWhiteSpace(replacee))
                {
                    return url;
                }

                return replacee;
            };

            string result = _regex.Replace(html, me);

            return result;
        }
    }
}