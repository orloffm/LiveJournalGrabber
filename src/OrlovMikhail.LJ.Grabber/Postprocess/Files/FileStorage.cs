﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace OrlovMikhail.LJ.Grabber
{
    /// <summary>Stores files in "files" subfolder.</summary>
    public class FileStorage : StorageBase, IFileStorage
    {
        private const string SubdirToUse = "Files";
        private const string MappingsFilename = "mappings.txt";

        private readonly IFileSystem _fs;
        private readonly string _workingDir;

        private readonly Dictionary<string, string> _files;

        static readonly ILog log = LogManager.GetLogger(typeof(FileStorage));

        public FileStorage(IFileSystem fs, string basePath)
        {
            _fs = fs;
            _workingDir = fs.Path.Combine(basePath, SubdirToUse);

            _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ReloadMappings();
        }

        #region mappings
        private void ReloadMappings()
        {
            _files.Clear();

            string path = MakeFullPath(MappingsFilename);
            if (_fs.File.Exists(path))
            {
                string[] lines = _fs.File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    string[] kvp = line.Split('\t');

                    // Manually edited lines cause failure.
                    if (kvp.Length != 2)
                        throw new InvalidOperationException("Incorrect line content: '" + line + "'");

                    string originalUrl = kvp[0];
                    string actualFilename = kvp[1];

                    string fullActualFilePath = MakeFullPath(actualFilename);
                    if (_fs.File.Exists(fullActualFilePath))
                    {
                        // File exists, add it.
                        _files[originalUrl] = actualFilename;
                    }
                }
            }
        }

        private void SaveMappings()
        {
            StringBuilder w = new StringBuilder();
            foreach (var kvp in _files)
                w.AppendFormat("{0}\t{1}\r\n", kvp.Key, kvp.Value);

            string content = w.ToString();
            string path = MakeFullPath(MappingsFilename);
            FileInfoBase fi = _fs.FileInfo.FromFileName(path);
            _fs.Directory.CreateDirectory(fi.DirectoryName);
            _fs.File.WriteAllText(path, content);
        }
        #endregion

        private string MakeFullPath(string filename)
        {
            string fullActualFilePath = _fs.Path.Combine(_workingDir, filename);
            return fullActualFilePath;
        }

        public override FileInfoBase EnsureStored(Uri url, byte[] data)
        {
            string path = System.Net.WebUtility.UrlDecode(url.AbsolutePath);

            bool isImage = Tools.IsAnImage(path);
            if (!isImage)
            {
                log.Error("File storage is meant only for images, won't store " + url.ToString() + ".");
                return null;
            }

            // Try load image from bytes.
            Image m;
            try
            {
                m = ImageHelper.LoadImageFromBytes(data);
            }
            catch
            {
                m = null;
            }

            if (m == null)
            {
                log.Error("The data from " + url.ToString() + " is not an image.");
                return null;
            }

            // How it should be saved?
            string bestName = _fs.Path.GetFileName(path);
            string newExtension;

            // Let's avoid gifs/bmps and resize huge images.
            byte[] dataToSave = ImageHelper.EnsureNotHugePNGOrJPEG(m, data, out newExtension);
            if (newExtension != null)
                bestName = _fs.Path.ChangeExtension(bestName, newExtension);

            string actualFilename;
            if (!_files.TryGetValue(url.AbsoluteUri, out actualFilename))
            {
                // File does not exist, create a name for it.
                actualFilename = InventNewFileNameFor(bestName);
            }

            string fullPath = MakeFullPath(actualFilename);
            FileInfoBase fi = _fs.FileInfo.FromFileName(fullPath);
            _fs.Directory.CreateDirectory(fi.DirectoryName);
            _fs.File.WriteAllBytes(fullPath, dataToSave);

            // OK, save.
            _files[url.AbsoluteUri] = actualFilename;
            SaveMappings();

            // Return the full fileinfobase.
            return TryGet(url);
        }

        /// <summary>Creates a new non-existing filename that will be used
        /// for storing the file from a given URL.</summary>
        private string InventNewFileNameFor(string bestName)
        {
            int counter = 0;
            string ret = bestName;
            while (_files.Values.Contains(ret, StringComparer.OrdinalIgnoreCase))
            {
                counter++;
                ret = _fs.Path.GetFileNameWithoutExtension(bestName) + "_" + counter + _fs.Path.GetExtension(bestName);
            }

            return ret;
        }

        public override FileInfoBase TryGet(Uri url)
        {
            string actualFilename;
            if (_files.TryGetValue(url.AbsoluteUri, out actualFilename))
            {
                string fullPath = MakeFullPath(actualFilename);
                return _fs.FileInfo.FromFileName(fullPath);
            }

            return null;
        }
    }
}
