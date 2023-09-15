// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KnowledgeMining.Functions.Skills.Distinct
{
    public class Thesaurus
    {
        public IDictionary<string, string> Synonyms { get; }

        public Thesaurus(string? blob)
        {
            ArgumentNullException.ThrowIfNull(blob, nameof(blob));

            Synonyms = new Dictionary<string, string>();

            ParseThesaurus(blob);
        }

        private void ParseThesaurus(string json)
        {
            var dataset = JsonSerializer.Deserialize<IEnumerable<IEnumerable<string>>>(json) ?? Enumerable.Empty<IEnumerable<string>>();

            foreach (IEnumerable<string> lemma in dataset)
            {
                if (!lemma.Any()) continue;

                var canonicalForm = lemma.First();

                foreach (string form in lemma)
                {
                    var normalizedForm = Normalize(form);

                    if (Synonyms.TryGetValue(normalizedForm, out string? existingCanonicalForm))
                    {
                        throw new InvalidDataException(
                            $"Thesaurus parsing error: the form '{form}' of the lemma '{canonicalForm}' looks the same, once normalized, as one of the forms of '{existingCanonicalForm}'. Please disambiguate or merge lemmas.");
                    }

                    Synonyms.Add(normalizedForm, canonicalForm);
                }
            }
        }

        public IEnumerable<string> Dedupe(IEnumerable<string>? words)
        {
            if (words is null)
            {
                return Enumerable.Empty<string>();
            }

            var normalizedToWord = new Dictionary<string, string>();

            foreach (string word in words)
            {
                string normalized = Normalize(word);
                string canonical = Synonyms.TryGetValue(normalized, out string? canonicalFromThesaurus) ?
                    canonicalFromThesaurus :
                    normalized;

                if (!normalizedToWord.ContainsKey(canonical))
                {
                    normalizedToWord.Add(canonical, canonicalFromThesaurus ?? word); // Arbitrarily consider the first occurrence as canonical
                }
            }

            return normalizedToWord.Values.Distinct();
        }

        private string Normalize(string word)
            => new(word
                .Normalize()
                .ToLowerInvariant()
                .Where(c => !(char.IsPunctuation(c) || char.IsSeparator(c)))
                .ToArray());
    }
}