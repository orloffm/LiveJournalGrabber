﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OrlovMikhail.LJ.Grabber.Entities;
using OrlovMikhail.LJ.Grabber.Entities.Other;

namespace OrlovMikhail.LJ.Grabber.Helpers
{
    public static class TestingShared
    {
        public static void CreateTwoComments(out Comment nonFullVersion, out Comment fullVersion)
        {
            nonFullVersion = new Comment();
            fullVersion = new Comment();

            foreach (Comment x in new[] {nonFullVersion, fullVersion})
            {
                x.Id = 1;
                x.Date = new DateTime(2010, 1, 1, 5, 5, 5);
                x.ParentUrl = new LiveJournalTarget("galkovsky", 1, 567).ToString();
                x.Url = new LiveJournalTarget("galkovsky", 1, 568).ToString();
            }

            nonFullVersion.IsFull = false;
            nonFullVersion.Depth = 2;

            fullVersion.IsFull = true;
            fullVersion.Depth = 1;
            fullVersion.Subject = "Subject";
            fullVersion.Text = "Text";
        }

        /// <summary>Generates a page that has a simple tree of comments.</summary>
        public static EntryPage GenerateEntryPage(bool makeAllFull = false, int shiftNumbers = 0)
        {
            EntryPage p = new EntryPage();
            p.Entry = new Entry
            {
                Text = "Text", Subject = "Subject", Id = 1, Poster = new UserLite {Username = "galkovsky"}
                , Date = new DateTime(2015, 1, 1)
            };

            p.CommentPages = new CommentPages
            {
                Current = 1, Total = 2, NextUrl = new LiveJournalTarget("galkovsky", 1).ToString()
                , LastUrl = new LiveJournalTarget("galkovsky", 1).ToString()
            };

            Comment a = new Comment
            {
                IsFull = makeAllFull, Poster = new UserLite {Username = "pupkin"}
                , Text = makeAllFull ? "1" : string.Empty
            };
            SetIdAndUrls(a, 11 + shiftNumbers, null);
            {
                Comment aB = new Comment
                {
                    IsFull = true, Poster = new UserLite {Username = "galkovsky"}, Text = "2"
                };
                a.Replies.Comments.Add(aB);
                SetIdAndUrls(aB, 12 + shiftNumbers, a);
                {
                    Comment aBC = new Comment
                    {
                        IsFull = makeAllFull, Poster = new UserLite {Username = "pupkin"}
                        , Text = makeAllFull ? "3" : string.Empty
                    };
                    aB.Replies.Comments.Add(aBC);
                    SetIdAndUrls(aBC, 13 + shiftNumbers, aB);
                }
                Comment aD = new Comment
                {
                    IsFull = makeAllFull, Poster = new UserLite {Username = "pupkin"}
                    , Text = makeAllFull ? "4" : string.Empty
                };
                a.Replies.Comments.Add(aD);
                SetIdAndUrls(aD, 14 + shiftNumbers, a);
            }

            p.Replies.Comments.Add(a);

            return p;
        }

        /// <summary>Gets the content of an embedded resource.</summary>
        public static string GetFileContent(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .First(z => z.EndsWith(filename));

            byte[] buffer;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }

            return Encoding.UTF8.GetString(buffer);
        }

        public static void SetIdAndUrls(Comment c, int id, Comment parent)
        {
            c.Id = id;
            c.Url = new LiveJournalTarget("galkovsky", 1, id).ToString();
            if (parent != null)
            {
                c.ParentUrl = new LiveJournalTarget("galkovsky", 1, parent.Id).ToString();
            }
        }
    }
}