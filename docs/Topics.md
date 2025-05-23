# Topics

Topics represent a single item that can be pointed out by an analyzer.  This might be a potential CTD it noticed, or simply a small passing suggestion.

An example Topic might be when an [Armor piece is missing its World Model](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Analyzers/discussions/83).  Anytime the analyzer suite notices this, it would raise this topic on the offending record.

## Levels

Each topic has an associated level:

- CTD: Can cause CTDs
- Error:  Causes problems in game, but game continues running
- Warning:  Something suspicious, but no concrete known negative side effects.
- Suggestion:  Optimizations, typical patterns, and anything else that doesn't fall into an above category

## Topic Code

Each topic has a "Code".  In the original example the code is `A83`.  This code is used in many configuration options and helps identify each topic uniquely.

## Github Discussions

Each topic comes with a [Github Discussion](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Analyzers/discussions/83).  In general, the number of a topic's [Code](#TopicCode) matches the Github discussion number.  The original topic's code is `A83` because the discussion it relates to is #83.

These discussions serve as a home for documentation relating their topic, as well as a contextual discussion forum for their topic.

Each topic discussion should generally have the following information:

- Topic Number (implicit)
- Severity label
- Games the topic applies to (Skyrim, Fallout, etc)
- Context
  - The "Bethesda Path". `ARMA/[MOD2/MOD3]`
  - The "Mutagen Path". `ArmorAddon.WorldModel.[M/F].File` is the path for the example topic, as these are the classes/fields involved in the topic.
- Description outlining what the issue/suggestion is
- Effects of what the topic are if left unattended
- Any related images/video
- Related references to other discussions or wikis on the topic