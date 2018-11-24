﻿using System;

namespace OrlovMikhail.LJ.Grabber.Postprocess.Files
{
    /// <summary>Extracts all file URLs from HTML.</summary>
    public interface IFileUrlExtractor
    {
        /// <summary>Extracts URLs to files that are listed in the HTML.</summary>
        string[] GetImagesURLs(string html);

        /// <summary>Given HTML, replaces all file URLs with the strings provided.</summary>
        /// <param name="html">Source HTML.</param>
        /// <param name="matcher">Function that returns values to replace the source with.</param>
        string ReplaceFileUrls(string html, Func<string, string> matcher);
    }
}
