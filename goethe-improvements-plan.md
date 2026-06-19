# Goethe Playtest — Improvement Plan

Based on **11 June 2026, Goethe-Institut** feedback. Status key: **Done** | **In progress** | **Planned** | **Deferred**

---

## Quick wins (implemented)

| Feedback | Players | Action | Status |
|----------|---------|--------|--------|
| UI invisible / bugs (canvas) | P4 | Canvas scale fix + `GameplayCanvasGuard` | **Done** |
| Bigger font | P2, P5 | Narration 28pt; HUD 46pt bold; menu buttons 44pt | **Done** |
| Lower dialogue box | P4 | Narration panel anchors lowered | **Done** |
| "Hits left" wording | P4 | HUD → **Lives** + **Artifacts X / 2** | **Done** |
| Artifacts more visible | P3, P4 | Gold pedestals + guaranteed spawn per segment | **Done** |
| Character bigger | P4 | Ch03 scale increased | **Done** |
| Slower base speed | P4 | `baseForwardSpeed` 8; speed shoes +12 | **Done** |
| Too short | P4 | `RunLengthController` + more spawns per segment | **Done** |
| More music | P2, P3 | Dual music layer + victory stinger blend | **Done** |
| Pickup/sneak sound link | P5 | Generated pickup WAV palette in `Assets/Audios/Pickups/` | **Done** |
| Menu UI | All, P4 | Orange visible buttons, subtitle, larger settings text | **Done** |

---

## Deferred (documented, larger scope)

| Feedback | Players | Decision | Status |
|----------|---------|----------|--------|
| Cut scenes / curtains | P1 | Side curtain obstacles only; full transitions later | **Deferred** |
| Side seating environment | P2 | Environment art pass on runner lanes | **Deferred** |
| Live AI dialogue in-game | — | Out of scope; scripted narration kept | **Rejected** |

---

## One-click setup in Unity

**Tools → Setup All Goethe Improvements** — runs menu, audio, pickups, HUD, curtains, level length, and stability wiring.

---

*Update this file as fixes ship. Link from `refinements-changes.md` and final reflection.*
