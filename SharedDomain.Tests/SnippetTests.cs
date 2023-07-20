using NUnit.Framework;
using SharedDomain.Domain;
using System;
using System.Linq;

namespace SharedDomain.Tests
{
    public class SnippetTests
    {
        public static readonly string[] PunctuationSnippets = new[] { "If no one is around you, say, \"Baby I love you\"",
            "(Clap your hands, y'all, it's alright)", "Will they stand their ground?",
            "I don't know what I'm gonna do (Don't know what to do)", "Screaming some sound!",
            "You can't feel my pain.", "(Shove) Shove it! Shove it! Shove it!", "Oh (yeah)!", "Why, ,do. you do ,!(this???)"};

        public static readonly string[] NonPunctuationSnippets = new[] {"In the end it all goes away",
            "I never meant to be so cold", "Peel me from the skin" };

        [TestCaseSource(nameof(PunctuationSnippets))]
        [Test]
        public void If_ContainsPunctuation_ShouldReturn_ValidQuestionAndAnswer(string snippetText)
        {
            //Act
            var snippet = new Snippet(snippetText, Snippet.Mode.LastTwoWords);
            Console.WriteLine($"FullText = {snippet.FullText}");
            Console.WriteLine($"Question = {snippet.Question}");
            Console.WriteLine($"Answer = {snippet.Answer}");

            //Assert
            Assert.IsNotEmpty(snippet.FullText);
            Assert.IsNotEmpty(snippet.Question);
            Assert.IsNotEmpty(snippet.Answer);
            Assert.AreEqual(snippetText, snippet.FullText);
            Assert.IsTrue(Snippet.IllegalStrings.Any(ies => snippet.Question.Contains(ies)));
            Assert.IsFalse(Snippet.IllegalStrings.Any(ies => snippet.Answer.Contains(ies)));
        }

        [TestCaseSource(nameof(NonPunctuationSnippets))]
        [Test]
        public void If_DoesNotContainPunctuation_ShouldReturn_ValidQuestionAndAnswer(string snippetText)
        {
            //Act
            var snippet = new Snippet(snippetText, Snippet.Mode.LastTwoWords);
            Console.WriteLine($"FullText = {snippet.FullText}");
            Console.WriteLine($"Question = {snippet.Question}");
            Console.WriteLine($"Answer = {snippet.Answer}");

            //Assert
            Assert.IsNotEmpty(snippet.FullText);
            Assert.IsNotEmpty(snippet.Question);
            Assert.IsNotEmpty(snippet.Answer);
            Assert.AreEqual(snippetText, snippet.FullText);
            Assert.IsFalse(Snippet.IllegalStrings.Any(ies => snippet.Question.Contains(ies)));
            Assert.IsFalse(Snippet.IllegalStrings.Any(ies => snippet.Answer.Contains(ies)));
        }
    }
}