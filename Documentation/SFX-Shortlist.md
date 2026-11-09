Current review: 26 clips consolidated directly in Assets/Audio/SFX Review. Spider Chatter is selected; current gameplay worm squish is preferred. Other remaining clips await selection. Source records: Documentation/SFX Review Sources.

# Flower and mermaid SFX shortlist

Research date: November 1, 2026. Based on `Art Assets Tracker - SFX.pdf` and the current Unity scenes/scripts.

Original sourcing pass (archived): 147 clips were downloaded from 16 sources. Six Freesound items are public compressed previews, clearly labeled. No clips are assigned to gameplay; none have been subjectively auditioned or purchased. See the folder's START HERE.txt and decisions sheet.

This remains a sourcing and implementation shortlist. Source descriptions and published license labels were checked; sound suitability still needs listening. OpenGameArt entries were available through indexed source-page content, while direct web-tool access was blocked by robots.txt. Source pages and files were subsequently retrieved through normal HTTP downloads, with page snapshots retained in `.utmp/sfx-download`. Confirm the license shipped with each downloaded file before final import. Freesound original downloads may require a free account.

All recommended sources below advertise CC0. No paid library is necessary for the first audition pass. Keep author, source URL, original filename, license copy, and any edit notes with each selected asset. Existing project clips have not had their original licenses audited in this pass.

## Source library

| ID | Source / creator | Published license | Material to audition |
|---|---|---|---|
| S1 | [40 CC0 water / splash / slime SFX — rubberduck](https://opengameart.org/content/40-cc0-water-splash-slime-sfx) | CC0 | 16 slime sounds plus bubbles, splashes, and water loops. Good starting material for wet treatment foley. |
| S2 | [Squish Sounds Effects — EZduzziteh](https://opengameart.org/content/squish-sounds-effects) | CC0 | `squish_01`–`06`, `squishpop`, `squishsplat_impact`; made for bug squashing. |
| S3 | [Sound Effects Pack — OwlishMedia](https://opengameart.org/content/sound-effects-pack) | CC0 | Fabric rustles, water, paper, and assorted impacts. Pack-level candidate; individual files still to select. |
| S4 | [Cloth Rip Fast — Rudmer_Rotteveel](https://freesound.org/people/Rudmer_Rotteveel/sounds/591195/) | CC0 | 1.333-second cloth tear, tagged for bandages. |
| S5 | [Cloth Rip Even Long — Rudmer_Rotteveel](https://freesound.org/s/591527/) | CC0 | 3.598-second continuous cloth tear, useful as edit material. |
| S6 | [Plants or Straw hay - dried cereals — Vrymaa](https://freesound.org/people/Vrymaa/sounds/734643/) | CC0 | Close handling, rustling and stalk crunches. Dry material: best for wilted flowers; assess whether softer excerpts suit fresh flowers. |
| S7 | [fly_buzzing_around.wav — KevinHilt](https://freesound.org/people/KevinHilt/sounds/381572/) | CC0 | Real fly. Creator notes handling rumble and collision thumps; choose a clean excerpt and high-pass as needed. |
| S8 | [Scissors — weyhingj](https://freesound.org/people/weyhingj/sounds/144070/) | CC0 | Air snips; select one short snip for stitching. |
| S9 | [Sodacan fizz (RAW) — Sadiquecat](https://freesound.org/people/Sadiquecat/sounds/817048/) | CC0 | Fizz texture; omit the can-opening sound. Artistic source for alcohol sizzle. |
| S10 | [Scrapes — AntumDeluge](https://opengameart.org/content/scrapes) | CC0 | Objects dragged across a cinder block, WAV/OGG. Strong material match for brick friction. |
| S11 | [Impact Sounds — Kenney](https://kenney.nl/assets/impact-sounds) | CC0 | Foley impact pack for brick end-stop, dry clacks, and tool contacts. Specific variants need auditioning. |
| S12 | [Interface Sounds — Kenney](https://kenney.nl/assets/interface-sounds) | CC0 | Optional quiet hover tick. Pick a restrained, non-electronic-sounding click. |
| S13 | [The Shop — LEGIT Audio](https://opengameart.org/content/the-shop) | CC0 for the free OGA samples only | Interior drones/room tones to filter into an eerie background layer. The separate paid full library uses different terms. |
| S14 | [Water Waves — transitking, submitted by qubodup](https://opengameart.org/content/water-waves) | CC0 | Four short wave recordings; layer quietly into ambience. |
| S15 | [Bubble Sound Effects — BMacZero / Brian MacIntosh](https://opengameart.org/content/bubble-sound-effects) | CC0 | Three isolated bubble sounds; optional damp ambience accents. These are bubbles, not verified drip recordings. |
| S16 | [General Household Sound Effects — bretbernhoft](https://opengameart.org/content/general-household-sound-effects) | CC0 | `clangingglass01/02.wav` for bottle handling; sink recordings to search for isolated droplets. Long recordings need excerpts. |

## Tracker coverage

Durations below are proposed edit targets, not measured durations of finished effects. A source combination means a designed cue still needs to be made and auditioned.

| Requested cue | First candidate | Proposed treatment |
|---|---|---|
| Creepy ambience: drone, ocean, drips | S13 + S14 + droplet excerpt from S16 | Build a quiet 20–40 s crossfaded bed. Sparse droplets. Keep treatment clicks intelligible. Confirm isolated drips exist; otherwise record them. |
| Worm squiggling loop | S1 | Assemble gentle sticky movement into a 2–4 s loop. Use a shared low-level bed instead of a loud loop on every worm. |
| Worm pop | S2 `squishpop` and `squish_01`–`06` | Short 0.1–0.35 s attacks; retain several variants. |
| Pulling off dirty bandages | S4 + S1 | Layer a short cloth pull with a restrained wet release. Avoid loud theatrical ripping. |
| Alcohol sizzle | S9 | Trim 0.3–0.8 s of fizz, fade the tail, optionally combine with the existing pour. |
| Stitching: needle + snip | S8 + quiet S3 fabric movement | Tiny puncture/rub followed by one snip, around 0.2–0.4 s. Exact needle texture may require recorded foley. |
| Fish-skin peel | S5 + S1 | Short tear plus sticky release. Keep the current trimmed clip as an A/B baseline; replacement only if it sounds better. Attack must occur immediately on click. |
| Grafting wet slap | S1 or S2 `squishsplat_impact` | Damp 0.15–0.4 s placement; no paper-flap character. |
| Pick up gloves | S3 fabric movement | Soft 0.15–0.3 s rustle. |
| Pick up bottle | S16 glass recording | Isolate one gentle clink, no ringing glass smash. |
| Pick up fish | S1 splash/slime + S11 soft impact | Small damp flop; proposed composite, not a verified fish recording. |
| Pick up bones | S11 dry impact | Short light clack; avoid heavy metal. If variants sound synthetic, record wooden sticks. |
| Pick up bandage roll | S3 fabric movement | Soft roll/rustle, distinguish from wet removal. |
| Tray sliding | S10 + quiet S11 end contact | Candidate friction layer, shorten and soften; cinder-block texture may be too gritty for the tray and must be auditioned. Existing toolbox clip is the comparison. |
| Dramatic brick sliding | S10 + S11 impact | Audible scrape followed by a decisive thunk; no long lead-in. Fit the existing drop/black-screen timing after choosing the cue. |
| Flower pick | S6 stalk snap + gentle rustle | 0.15–0.3 s pluck. Dry material is a candidate, not an exact fresh-stem match. Record a fresh stalk if it sounds brittle. |
| Flower Keep | S3 fabric movement | Soft 0.2–0.4 s cloth/bag rustle; exclude metallic contacts and zippers. |
| Flower Discard | S6 soft foliage excerpt | Light brush/drop; distinct from Keep and no water splash. |
| Spider reveal | S6 short crisp rustles | Design a subtle 0.15–0.3 s skitter using several tiny taps. Foley interpretation, not a spider recording. |
| Flies reveal | S7 | Clean 0.3–0.7 s buzz burst; avoid collision thumps. Stop on closing the inspection. |
| Flower hover (optional) | S12 | Quiet 0.03–0.08 s tick once on pointer entry. Omit if repetitive or distracting. |

## Additional needs found in the project

| Cue / finding | Recommendation |
|---|---|
| Flower Keep and Discard both reference `Keep (Bag Handling.mp3` | Highest replacement priority. Give the two choices their own cloth and foliage cues. |
| Flower pick, spider reveal, flies reveal, and hover have no dedicated fields/hooks in `FlowerMinigame` | Add explicit event-triggered audio when implementing selected clips; do not attach sounds to sprite bobbing or every update. |
| Day 7 wilted-flower removal has a null phase sound | Use S6 for a brittle dry pluck; this differs from fresh flower placement. |
| Day 6 and Day 7 fresh-flower placement have null phase sounds | Add a soft leaf/stem placement from S6, without a root snap. |
| Large worm first smash / half survives | Use a heavier wet hit from S2, distinct from the final burst. |
| Large worm releases smaller worms | Use a short burst from S2, timed to release. Avoid stacking a full loud pop for each released worm. |
| Small worms land / mini splats | S2 `squishsplat_impact` is a candidate; quieter and smaller than the main burst. Group near-simultaneous landings to control volume. |
| Grafting uses `paper_flap.mp3` | Replace with S1 wet contact; consider a quiet rub only while moving, with one completion slap per applied piece. |
| Dirty removal and clean bandage application share `bandage.mp3` | Split wet peeling from clean soft wrapping. |
| Tray opening/closing uses `toolbox.mp3` | Replace with shorter sliding foley once the material match is confirmed. |
| Worm phases use `Assets/Resources/Audio/squish.mp3` | A/B against S2; do not replace solely because a new source exists. Add variants if repeated identical pops are tiring. |
| Barnacles use an existing rip-variant set | Lower priority: keep unless listening exposes a mismatch. S1/S4 could supply an alternate wet rip composite. |
| Flower sanity feedback already exists | Keep the established shared feedback and flower-only quieter volume unless it clashes with the new foley. No new success jingle is needed. |
| Stench indicators | Keep visual-only by default. Adding 14 independent buzz loops would compete with treatment cues. |

## Audition and implementation order

1. Download S1, S2, S3, S6 and S7 for the organic/flower pass; save their license evidence with the originals. These address the most noticeable placeholders and missing cues.
2. Audition clean excerpts, then S4/S5/S8/S9 for treatment detail and S10/S11/S16 for objects. Compare existing fish peel, barnacle, and squish clips at matched loudness.
3. Make short variants with no leading silence, gentle tail fades, and headroom. Loop only ambience/movement beds; test seams before importing.
4. Add selected WAV/OGG clips and license notes to the project, then assign real scene references. New reveal/landing/tool cues need event hooks beyond the existing phase sound field.
5. Test rapid clicking, several simultaneous landings, pause, closing inspections, changing tools, and scene exit. Preserve existing audio volume controls and avoid changing shared fishing sounds when replacing flower cues.

A direct plant-pull recording by Jofae was also found, but its description asks for use in clean/appropriate media. Although its page advertises CC0, the shortlist uses other material for this horror project to avoid that ambiguity. No paid assets or restrictive-license alternatives are needed at this stage.
