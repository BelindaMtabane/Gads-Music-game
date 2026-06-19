# Critical Engagement With Feedback — Rhythm Raiders

**Event:** Goethe-Institut playtest · **11 June 2026, 18:00**  
**Word count:** ~560  

---

## What did I expect?

Before the Goethe-Institut session, I expected feedback on **whether the chase fantasy works** and whether players understood the **two-artifact win condition**. Because we use AI tools in development, I also expected questions about **dialogue** — but I assumed criticism would focus on the **guard difficulty** and **jump feel**, which we had already tuned in earlier internal playtests.

I expected younger players might struggle with controls; I did **not** expect the **youngest tester (age 14)** to praise AI-assisted dialogue as the feature that **helped him understand the game**. That shifted how I value narration relative to pure mechanics.

---

## What surprised me?

The strongest surprise was **unanimous UI criticism** combined with **positive dialogue clarity**. All five players said the UI needs work; four said dialogue/AI narration is **simple and straightforward**. We had been treating UI as secondary to “making the run work,” but external players experience **menus, fonts, and dialogue boxes first**.

**Artifact visibility** surprised two adult players (P3, P4). We invested in guard AI and pink-material fixes; players still could not **see the goal objects** clearly on the lane.

P4 gave the densest list: **bugs**, **too short**, **lower dialogue box**, **bigger character**, **speed without shoes**, and **change “hits left” wording**. That matched technical debt we knew about (canvas scale bugs) but had not fully fixed before the event — so public feedback felt harsher than internal testing.

P5’s comment that **pickup sounds should connect to sneaking sounds** was unexpected — a **audio design cohesion** note we had not considered. P2 and P3 both asked for **more music**, which aligns with our brand but exposes thin audio layering in the current build.

Positive surprise: **“Love the idea of music and gaming — great vibe”** (P5) and P1 liking the **sound concept**. The core pitch lands; execution and polish lag.

---

## What did I ignore or choose not to implement?

**Side seats in the environment (P2):** Deferred. Valid set-dressing for a venue fantasy, but requires modelling time with low impact on core loop before submission.

**Full curtain cut-scenes (P1):** Deferred to a future art pass. We lowered and enlarged the dialogue box instead for faster UX gain.

**Live AI dialogue during play:** Not requested directly, but implied by course context. Rejected for this milestone — latency, scope, and playtest stability. We keep **scripted** lines written with AI-assisted drafting offline.

**Making the level much longer immediately:** Partially accepted. P4’s “too short” is noted; we plan more spawn distance but will not rebuild the entire level in Part 3 week.

**Every UI redesign suggestion without prioritization:** We implemented **font size, panel position, HUD labels, visibility scales** first; full menu redesign is staged, not ignored.

---

## Evaluation of feasibility

| Feedback | Feasibility | Decision |
|----------|-------------|----------|
| Bigger font / lower dialogue box | High | Implemented |
| Artifacts & character more visible | High | Scaled in scene |
| Rename “Hits Left” | High | → “Lives” + artifact X/2 |
| More music | Medium | Planned in AudioManager |
| Pickup/sneak sound cohesion | Medium | Planned SFX pass |
| Base speed without shoes | Medium | Tune `forwardSpeed` |
| Bugs (canvas) | High | Canvas scale fixed again |
| Side seats / curtains | Low this sprint | Deferred |

---

## Final judgement

Goethe feedback **confirmed the idea** and **challenged the presentation**. We integrate: **readability, visibility, honest HUD language, and stability**. We defer: **environment dressing and cinematic curtains**. We reject: **scope explosions** that risk another bug pass.

This matches professional practice: playtests tell you what players **feel**, not what to build blindly. Our `goethe-improvements-plan.md` tracks what shipped versus what is next.

---

*Full record: `feedback-summary.md` · Actions: `goethe-improvements-plan.md`*
