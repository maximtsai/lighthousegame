# Game Design Document: Lighthouse

*Note: All dialogue text in this document is currently placeholder and subject to change.*

## 1. Game Overview
- **Title:** Lighthouse
- **Genre:** Psychological Horror / Point-and-Click Narrative
- **Platform:** TBA
- **Target Audience:** TBA

## 2. Gameplay Mechanics
### Core Loop
The player experiences a sequence of days (Day 1 to 7). Each day consists of a checklist of routine tasks (Washing up, eating, checking weather, maintaining the lighthouse, checking fish traps). Performing tasks or reacting to events affects a hidden or visible **Sanity** score.

### Sanity System
- Specific actions or choices yield Sanity points (e.g., +1, +2) or deduct Sanity points (e.g., -2, -5, -10).
- Sanity levels appear to affect the protagonist's perception of reality, leading to different endings.

### Controls
- Point-and-Click interaction with environments (Scenes) and Objects.
- UI elements include a Task list (that glows when updated) and a Toolbar for inventory items.

## 3. Story and Setting
### Premise
You are a lighthouse keeper isolated on an island. Your coworker, Camborne, has died. As the days progress, isolation and routine give way to psychological breakdown, culminating in the discovery of a grotesque mermaid.

### Characters
- **The Protagonist:** The player character. A lighthouse keeper struggling with isolation, trauma, and deteriorating sanity.
- **Camborne:** The protagonist's coworker. Currently deceased and requires burying. His presence (and rot) haunts the protagonist.
- **The Mermaid:** A grotesque figure that washes ashore. The protagonist becomes obsessed with saving and caring for her.

### Setting/Environments (Scenes)
- **Bedroom:** Your bed, coworker's bed, table (journal/sketchbooks), restroom door, stairs.
- **Restroom/Sink:** Sink, mirror, washcloth, shaving tool, toothbrush.
- **Kitchen:** Stairs, doors to outside, coworker's body (initially), stove.
- **Stove:** Contextual scene for cooking meals (fish and corn chowder).
- **Outside:** Docks, Home exterior, Lighthouse exterior, Beach (debris).
- **Lighthouse (1st Floor)**
- **Lighthouse (Middle Floor)**
- **Lighthouse (Top Floor):** Central Mirror, Lantern, Mop, Wick, Fuel area, Mercury basin, Gearbox.
- **Dock:** Fish traps.

## 4. Day-by-Day Walkthrough

### Day 1
**Tasks:**
- **Wash Up:** Go to sink, hover tools. Action: Wash (+2 sanity).
- **Eat Breakfast:** Go to stove, cook fish and corn chowder (+2 sanity). "Fish and corn chowder for the 17th time in a row."
- **Observe Weather:** Go outside, use Journal. Pick specific weather (e.g., sunny + altocumulus for +2 sanity).
- **Maintain the Lighthouse:** Go to top floor. Mop, snip wick, refill fuel, refill mercury, wind gearbox.
- **Check Fish Traps:** Go to dock, collect fish.
- **Dinner:** Cook fish again ("18th meal in a row").
- **Bury it:** Interact with dead Camborne's body. Must bury him in the dirt mound outside before sleeping (-5 sanity for smelling).
- **Go To Bed:** Head to bedroom, journal explicitly (draw dead Camborne to process trauma).

### Day 2
**Tasks:**
- **Wash Up & Breakfast:** Similar to Day 1. Stove event: Dead meat moves. See it? Yes (+1 sanity), No (-2 sanity).
- **Observe Weather:** Pick weather (+2 sanity). Event: Something large brushes water. See it? Yes (+1 sanity), No (-2 sanity).
- **Maintain the Lighthouse:** Same as Day 1 until wick trimming. Light flickers. Has stain always been there? Yes (+1 sanity), No (-2 sanity).
- **Check Fish Traps:** Same as Day 1.
- **Inspect Grave (New):** Rain undone the grave. Must fix it. Camborne's face poking out -> bury -> cut hand -> bleed.
- **Bandage Hand (New):** Go to sink. Infection question? Yes (-2 sanity), No (+1 sanity).
- **Eat Dinner:** Fish makes you sick.
- **Go To Bed:** "Your bed." Coworker's bed: "It sleeps outside now."

### Day 3
**Tasks:**
- **Wash Up:** Festering wound. Scratch (-2 sanity), Rinse (+1 sanity).
- **Eat Breakfast:** "Just corn chowder is enough for you."
- **Observe Weather:** Weather looks fine. Journal prompts repeatedly ask "Are you sure?" -> Raging thunderstorm blackout.
- **Maintain the Lighthouse:** Trimming wick -> drop scissors down stairs. "Clumsy oaf."
- **Retrieve Scissors (New):** Go to 1st floor.
- **Finish Maintenance:** Go back up. Are they dropped? Yes (+1 sanity), I don't know (-2 sanity).
- **Check Fish Traps:** Visuals of dead bodies (-10 sanity).
- **Avert Eyes, Return to Quarters, Go To Bed.**

### Day 4
**Tasks:**
- **Wash up, Breakfast, Weather:** Prompts to "Scratch" wound added to all three. Yes (-2 sanity), No (+0 sanity).
- **Survey the Beach (New):** Canned supplies and corpses washed up.
- **Bury them (New):** Bury 4 bodies. "Scratch?" Yes (-2), No (+0). 5th body is the Grotesque Mermaid. "She can be saved."
- **Take her to Camborne's bed:** Appears after performing CPR on mermaid.
- **Wash hands (New)**
- **Clean her (Minigame):** Remove barnacles/seaweed, wipe down.
- **Disinfect wound:** Use alcohol.
- **Wrap wound:** Use cloth/blanket/coat.
- **Feed her:** Use corn chowder.
- **Go To Bed:** Draw her healthy in journal.

### Day 5
- **Event:** Mermaid jumpscare.
- **Tasks:**
  - Wash hands.
  - Mash worms on mermaid (Minigame).
  - Feed her. "She must be in pain."
  - Observe Weather. Option to pick "How is she?".
  - Check on her: Smash wriggling worms. "You're safe now."
  - Maintain Lighthouse: Mermaid jerks during task. "I am sorry. I will be back."
  - Go to bed. "It's my fault" dialogue if interacting with mermaid.

### Day 6
- **Morning Event:** Slithering sounds. Mermaid bloated with sea worms.
- **Tasks:**
  - Minigame.
  - Take her outside to the beach.
  - Sit with her: 25 seconds beach date -> sleep -> high tide.
  - Save her: Mermaid floating, worms eating her. Mash worms sequence. Body is mangled but face is pristine. She stops responding.
  - Decision: "She's gone" (Take to grave/sit in chair) OR "She needs rest" (Take to bed).

### Day 7 (Endings)
**Bliss Ending:**
- Mermaid is awake and nuzzles player on her bed.
- Tasks: Take her downstairs, sit at table, make breakfast (hand feed her).
- Ending Event: Delivery person arrives, takes one look at you, screams and runs away.

**Sanity Ending:**
- Tasks remain routine until bed time.
- Task: Clean Camborne's bed (black stains from previously killing worms).
- Find letter under blanket: "I've already died long ago. -Camborne"
- Go to bed. Lights out.
- Concepts/Ideas: Lighthouse head spinning, Botanical on graveyard, Sitting all alone with the lighthouse's light and nothing else.

## 5. Art and Sound
### Art Style
- Detailed UI and Point-and-Click scenes.
### Sound Effects
- Wash sound effects
- Liquid fillings (fuel/mercury)
- Winding graphics/sounds (gearbox)
- Scissor snips
- Thunderstorm/Rain
- Mopping
- Meat moving/Slithering
- CPR/Smashing worms
### Music
- TBA

## 6. Development Roadmap
### Phase 1: Prototype
- Core loop (Scenes, basic tasks UI)
- Day 1 implementation
### Phase 2: Alpha
- Complete 7-day progression events
- Sanity scoring implementation
### Phase 3: Beta
- Polish art, sound, and dialogue balancing
