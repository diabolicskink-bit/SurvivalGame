# Colorado World Map Sources

This note records the source strategy for the first Colorado overworld slice. The in-game map is scaled and simplified for readability, but city, landmark, and road placement should preserve real relative geography where practical.

## Source Strategy

- Colorado bounds use an approximate full-state rectangle: longitude `-109.06` to `-102.04`, latitude `36.99` to `41.00`.
- City and town markers use real-world municipal locations, curated down to a readable major set rather than every municipality.
- Landmark POIs prioritize recognizable Colorado places, parks, passes, reservoirs, transport nodes, and major infrastructure.
- Road geometry is generated into `data/world_map/colorado_roads.generated.json` from official Colorado GIS highway layers, then simplified into coarse map-scale route chunks for readable in-game drawing.
- Tactical atlas and terrain-cost data are generated from deterministic curated Colorado-shaped masks into `data/world_map/colorado_atlas.png` and `data/world_map/colorado_terrain.generated.json`.
- The V2 atlas generator is intentionally more illustrated than cartographic: it paints mountain spines, alpine high country, foothills, forest bands, canyon country, western plateau/desert scrub, plains texture, named river corridors, and irregular reservoir shapes so the background visually communicates the sampled travel terrain.
- Terrain regions remain as fallback gameplay bands, while the generated terrain grid is the primary world-map travel terrain source when present.

## Primary Sources

- CDOT Colorado Highways: https://data.colorado.gov/Transportation/Colorado-Highways/2h6w-z9ry/about
- CDOT Major Roads: https://data.colorado.gov/Transportation/Colorado-Major-Roads/e7ye-tasg/about
- Colorado State Basemap ArcGIS Interstates layer: https://gis.colorado.gov/public/rest/services/OIT/Colorado_State_Basemap/MapServer/36
- Colorado State Basemap ArcGIS US Highways layer: https://gis.colorado.gov/public/rest/services/OIT/Colorado_State_Basemap/MapServer/37
- Colorado State Basemap ArcGIS State Highways layer: https://gis.colorado.gov/public/rest/services/OIT/Colorado_State_Basemap/MapServer/38
- Colorado State Basemap ArcGIS Highways for Large Scale layer: https://gis.colorado.gov/public/rest/services/OIT/Colorado_State_Basemap/MapServer/39
- CDOT GIS program: https://www.codot.gov/programs/gis
- CDOT Cities in Colorado: https://data.colorado.gov/Transportation/Cities-in-Colorado/7nuk-vzhq/about
- DOLA Municipal Boundaries: https://data.colorado.gov/Geo-Data/Colorado-Municipal-Boundaries/u943-ics6/about
- Colorado scenic drives and byways: https://www.colorado.com/activities/scenic-drives
- Colorado national parks guide: https://www.colorado.com/articles/quick-guide-colorado-national-parks/
- NPS Colorado list: https://www.nps.gov/state/co/list.htm

## Selection Rules

- Keep the first city/town set around 45-50 markers so the map remains readable without zoom-level filtering.
- Keep non-city landmarks around 40-50 markers and favor recognizable names over purely resource-optimal gameplay sites for this first pass.
- Keep the generated road layer to curated major routes for this pass: interstates, selected US highways, and selected state highways. Do not import dense local streets yet.
- Use route geometry from the Interstates, US Highways, and State Highways basemap layers. Use the detailed highway segment layer for lane/width metadata where available, falling back to conservative route-class defaults. Visual map lanes are generated from typical route lane counts: one, two, or three lanes each way.
- Regenerate roads with `python tools/world_map/generate_colorado_roads.py` when the route selection, simplification tolerance, or source query rules change. Keep the tolerance coarse enough for overworld readability rather than preserving every highway switchback. The generated JSON is committed so runtime play does not need network access.
- Regenerate the tactical atlas and terrain-cost grid with `python tools/world_map/generate_colorado_atlas.py` when terrain classification, atlas size, grid size, or tactical-atlas styling changes.
- The generated terrain grid currently uses Colorado-specific travel terrain types rather than only broad bands: shortgrass prairie, high plains, Front Range corridor, foothills, mountain forest, alpine peaks, mountain valleys, western plateau, canyonlands, desert scrub, river corridors, and reservoirs.
- Keep the dedicated `gas_station` and `farmstead` local-site test POIs near the Front Range start point for quick manual testing.
- Use real sensitive or active sites as map anchors when useful, but treat later local-site content deliberately.
- Do not treat this data as final route simulation. Roads currently influence travel cost near the party; they do not constrain movement or pathfind.
