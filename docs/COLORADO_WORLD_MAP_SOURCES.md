# Colorado World Map Sources

This note records the source strategy for the first Colorado overworld slice. The in-game map is scaled and simplified for readability, but city, landmark, and road placement should preserve real relative geography where practical.

## Source Strategy

- Colorado bounds use an approximate full-state rectangle: longitude `-109.06` to `-102.04`, latitude `36.99` to `41.00`.
- City and town markers use real-world municipal locations, curated down to a readable major set rather than every municipality.
- Landmark POIs prioritize recognizable Colorado places, parks, passes, reservoirs, transport nodes, and major infrastructure.
- Road geometry is generated into `data/world_map/colorado_roads.generated.json` from official Colorado GIS highway layers, then simplified for readable in-game drawing.
- Terrain regions are broad gameplay bands: plains, Front Range corridor, central mountains, western plateau, San Luis Valley, and major reservoir area.

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
- Use route geometry from the Interstates, US Highways, and State Highways basemap layers. Use the detailed highway segment layer for lane/width metadata where available, falling back to conservative route-class defaults.
- Regenerate roads with `python tools/world_map/generate_colorado_roads.py` when the route selection, simplification tolerance, or source query rules change. The generated JSON is committed so runtime play does not need network access.
- Keep the dedicated `gas_station` and `farmstead` local-site test POIs near the Front Range start point for quick manual testing.
- Use real sensitive or active sites as map anchors when useful, but treat later local-site content deliberately.
- Do not treat this data as final route simulation. Roads currently influence travel cost near the party; they do not constrain movement or pathfind.
