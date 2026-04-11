# Raw Asset Sources

`assets-raw/` is the editable source-of-truth lane for pre-game UI resources.

Expected layout:
- `upstream/`: symlinked or copied source paths from external `ppy/osu` and `ppy/osu-resources` checkouts
- `local/`: rewrite-owned branding replacements and local overrides
- `upstream/theme-staging/source-index.json`: generated index of included upstream source roots and contract entry counts

Shipped runtime assets are generated into `assets/`, not read from this directory directly.
