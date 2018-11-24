﻿using System;
using System.Linq;
using NUnit.Framework;
using OrlovMikhail.LJ.Grabber.Helpers;
using OrlovMikhail.LJ.Grabber.Postprocess.Files;

namespace OrlovMikhail.LJ.Grabber.Entities.Helpers
{
    [TestFixture]
    public sealed class RepliesHelperTesting
    {
        private RepliesHelper _rh;

        [SetUp]
        public void BeforeTests()
        {
            _rh = new RepliesHelper(new EntryBaseHelper(new FileUrlExtractor()));
        }

        #region comments

        [Test]
        public void MergesAllFullComments()
        {
            EntryPage target = TestingShared.GenerateEntryPage();
            EntryPage source = TestingShared.GenerateEntryPage(makeAllFull: true);

            Comment[] commentsBefore = _rh.EnumerateAll(target.Replies).ToArray();

            _rh.MergeFrom(target, source);

            Comment[] commentsAfter = _rh.EnumerateAll(target.Replies).ToArray();

            // No objects replaced.
            for(int i = 0; i < commentsBefore.Length; i++)
            {
                Assert.AreSame(commentsBefore[i], commentsAfter[i]);
                Assert.IsTrue(commentsAfter[i].IsFull);
                Assert.IsTrue(!String.IsNullOrWhiteSpace(commentsAfter[i].Text));
            }
        }

        [Test]
        public void InsertCommentsFromMiddlePages()
        {
            // Three original pages.
            EntryPage p1 = TestingShared.GenerateEntryPage(true, 0);
            EntryPage p2 = TestingShared.GenerateEntryPage(true, 50);
            EntryPage p3 = TestingShared.GenerateEntryPage(true, 100);

            _rh.MergeFrom(p1, p3);
            _rh.MergeFrom(p1, p2);

            Comment[] comments = _rh.EnumerateAll(p1.Replies).ToArray();
            Assert.AreEqual(12, comments.Length, "Comments should've been added.");
            CollectionAssert.AllItemsAreUnique(comments.Select(z => z.Id));
            CollectionAssert.IsOrdered(comments.Select(z => z.Id));
        }
        #endregion

        #region new version update
        [Test]
        public void UpdatesTheCommentTree()
        {
            Comment a, b;
            TestingShared.CreateTwoComments(out a, out b);

            Comment c = new Comment() { Id = 3 };
            a.Replies.Comments.Add(c);
            Comment e = new Comment() { Id = 5 };
            a.Replies.Comments.Add(e);

            Comment d = new Comment() { Id = 4 };
            b.Replies.Comments.Add(d);

            _rh.MergeFrom(a, b);

            Assert.AreSame(c, a.Replies.Comments[0], "Should've kept the old comment in the comment tree.");
            Assert.AreSame(e, a.Replies.Comments[2], "Should've kept the old comment in the comment tree.");
            Assert.AreEqual(d, a.Replies.Comments[1], "Should've updated the comment tree with a new comment.");
        }

        [Test]
        public void UpdatesWithNewVersion()
        {
            Comment a, b;
            TestingShared.CreateTwoComments(out a, out b);

            object oldCollection = a.Replies.Comments;
            int oldDepth = a.Depth;
            string oldParentUrl = a.ParentUrl;

            _rh.UpdateDirectDataWith(a, b);

            Assert.AreSame(oldCollection, a.Replies.Comments);
            Assert.AreEqual(oldDepth, a.Depth, "Should keep the depth.");
            Assert.AreEqual(b.Text, a.Text);
            Assert.AreEqual(b.Subject, a.Subject);
            Assert.AreEqual(b.PosterUserpic, a.PosterUserpic);
            Assert.AreEqual(oldParentUrl, a.ParentUrl);
            Assert.AreEqual(b.DateValue, a.DateValue);
        }

        [Test]
        public void ThrowsIfNewVersionIsOfDifferentId()
        {
            Comment a, b;
            TestingShared.CreateTwoComments(out a, out b);
            b.Id++;

            Assert.That(() => _rh.UpdateDirectDataWith(a, b), Throws.ArgumentException);
        }
        #endregion

        #region comment enumeration
        [Test]
        public void EnumerateFoldedComments_Works()
        {
            Replies cs = new Replies();
            Comment a = new Comment() { Id = 11, IsFull = false, Text = String.Empty };
            Comment aB = new Comment() { Id = 12, IsFull = true, Text = "2" };
            Comment aBc = new Comment() { Id = 13, IsFull = false, Text = String.Empty };
            Comment aD = new Comment() { Id = 14, IsFull = false, Text = String.Empty };

            cs.Comments.Add(a);
            a.Replies.Comments.Add(aB);
            a.Replies.Comments.Add(aD);
            aB.Replies.Comments.Add(aBc);

            long[] texts = _rh.EnumerateRequiringFullUp(cs).Select(z => z.Id).ToArray();
            Assert.AreEqual(3, texts.Length, "Should've found 3 comments.");
            CollectionAssert.AreEqual(new long[] { 11, 13, 14 }, texts, "Collections match.");

            Assert.AreEqual(1, _rh.EnumerateFull(cs).Count(), "Should've found 1 comment.");

            Assert.AreEqual(4, _rh.EnumerateAll(cs).Count(), "Should've found 4 comments.");

        }
        #endregion
    }
}
