# Sound integration and fishing requirements

Scene references use clips in `Assets/Audio/SFX Review`. Existing preferred worm, peel, clean-bandage and tray clips retain their original references.

| Action | Clip |
| --- | --- |
| Pick flower | Crisp Flower Pick |
| Keep / discard flower | Bag Rustle / Discarded Flower Rustle |
| Hover flower | Flower Hover Tick (quiet, rate limited) |
| Reveal spider / flies | Spider Chatter / Quick Fly Buzz |
| Worm movement on Days 5-7 | Eight clips. Four excerpts from [d0 Maggots by dav0r](https://freesound.org/people/dav0r/sounds/223964/). Wet Worm Wriggle 71.92-72.31s, Soft Worm Slither 46.69-47.32s from [squishes.wav by Eneasz](https://freesound.org/people/Eneasz/sounds/216882/). Sticky Worm Twist 96.83-97.25s, Worm Ripple 108.85-109.37s from [slime - squishing sounds by rubberduck9999](https://freesound.org/people/rubberduck9999/sounds/680447/). All three sources are CC0. |
| Remove dirty bandage | Sticky Bandage Removal plus Quick Bandage Removal, shuffled on Days 5-7. Quick variant removes the first 0.215 seconds of the approved clip, retaining the rip and tail. Original: (approved natural removal preview; [SoundDino: The sound of a torn band-aid from the body](https://sounddino.com/en/effects/bandage-plaster/)). Source trimmed to 0.15-0.65 seconds and volume multiplied by 0.85; no pitch, speed, or texture processing. |
| Alcohol treatment | Original pouring.mp3 |
| Place fish-skin graft | Fish Skin Graft |
| Stitch | Quick Stitch / Tight Stitch, shuffled variants |
| Pluck wilted / place fresh flowers | Wilted Flower Pluck / Fresh Flower Placement |
| Pick gloves / bottle / bones / bandage roll / fish | Glove Snap / Bottle Clink / Fish Bone Clack / Bandage Roll Pickup / Fish Flop |
| Pick fish skin / flowers | Fish Skin Graft / Fresh Flower Placement |
| Brick falls / slides into wall | Original Brick Fall Out.mp3 / Brick Sliding into Place |

Pest audio follows the shuffled flower result, not the original background patch. Alternatives Soft Flower Pick and Close Fly Buzz remain available in the review folder. Preferred copies there are comparison copies; the scenes use the original clips.

The required mermaid tool is shown at full brightness; other tool art is dimmed. Automatic Button tinting and stale renderer fades are cleared. Passive glow images are disabled; the existing hover outline remains. No runtime hierarchy builder or editor hookup helper ships with the project.

Days 5–7 each contain one graft fish and the original food fish. Fishing tracks each kind separately in day-specific saved flags. Keeping one continues fishing; kept kinds are excluded from later bags and reopening. Discarding does not award a catch. Both are needed before `gathered_fish` and the fishing task complete. New days reset the flags. Dinner requires food fish again on those days; Day 2 keeps its existing exception.

Source and attribution records remain in `Documentation/SFX Review Sources`.

Validation: Unity 6000.2.5f1 batch compilation and final scene loading passed. Checks covered Days 1-7, both catch orders, discard/reopen behavior, dinner requirements, all 24 tool states, phase audio references, shuffled flower pest audio, and brick timing tied to clip duration. Visual appearance and audio balance still need a playthrough.
