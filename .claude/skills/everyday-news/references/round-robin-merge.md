# Round-Robin Merge Algorithm

Source: `scripts/fetch_news.py` — `merge_and_limit()`

## Purpose

Ensure news items come from multiple sources within a category, rather than one source filling all 5 slots.

## Algorithm

```
Given: sources = [(name1, items1), (name2, items2), ...]
Limit: N (default 5)

1. Convert each source's items to a mutable list
2. Round-robin: take 1 unseen item from source 1, then source 2, then source 3...
3. Skip duplicates via seen-set (keyed on title[:40].lower())
4. Stop when N items collected or no source has unseen items left

Result: sources[0][0], sources[1][0], sources[0][1], sources[1][1], ...
```

## Example

财经 with CNBC (5 items) + MarketWatch (5 items), limit=5:
→ CNBC[0], MW[0], CNBC[1], MW[1], CNBC[2]

Not: CNBC[0..4] (fills 5 from first source)

## Why not simple dedup+truncate?

Simple `flatten → dedup → [:N]` fills from the first source's items before the second source gets a chance, because sources are ordered by priority in SOURCES dict. Round-robin guarantees every source contributes at least its share.
