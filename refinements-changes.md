# Refinements & Changes

Chronological summary of polish, fixes, and features added during development (especially AI-assisted sessions). Use this as a changelog reference before releases or demos.

---

## Gameplay & win conditions

| Change | Detail |
|--------|--------|
| Victory threshold | Win requires **2 artifacts** (`PickupBase.ArtifactsToWin`); was 10, then aligned across `ArtifactGoal`, `MusicInstrument`, `GameManager` |
| Centralized win check | `PickupBase.TryTriggerVictory()` prevents early victory on single pickup |
| Enemy catch | First contact with guard (while not sneaking) → instant `KillPlayer()` → defeat anim → Death scene |
| High-danger obstacles | `HealthDecreaseObstacle.instantKill` routes through `GameManager.TriggerGameOver()` |
| Enemy speed | Chase speeds reduced by ~3 (default 9, post-boost 4, boosted ~34–37) |
| Single jump | `canJump` + foot `GroundCheck`; jump animation only on `DidJumpThisFrame` |
| Jump landing fix | `groundCheck` was wrongly wired to Ground platform → always grounded; auto foot check added |

---

## Narration & scenes

| Change | Detail |
|--------|--------|
| Pre-game narration | Intro lines play **before** 5–4–3–2–1 countdown (not after GO) |
| `PlayAndWait` | `NarrationManager` coroutine waits for all lines before countdown |
| Canvas scale fix | MainGameL1 UI Canvas `(0,0,0)` → `(1,1,1)` so HUD/narration visible |
| Scene fader | `SceneFader` builds persistent fade canvas; `IsBlocking` for sync |
| Legacy fade disabled | Old `FadeOverlay` objects disabled to prevent permanent black screen |
| Title branding | UI shows **Rhythm Raiders** (not legacy Gads Music Game name) |
| Narrator audio | Voice clips set to **no loop** when line ends |
| Death / Victory HUD | Wait for scene fade; NEXT buttons wired with raycast fixes |

---

## UI & input

| Change | Detail |
|--------|--------|
| Cursor | `UICursor` locks in gameplay, unlocks on menus / death / victory |
| Button clicks | `UIButtonRaycastFix` + `ButtonSoundPlayer` on UI buttons |
| NEXT buttons | Fixed non-functional NEXT on game over and victory screens |
| TMP glyph warning | `NEXT ▶` may show □ — cosmetic (font lacks U+25B6) |

---

## Audio (new systems)

| Asset / script | Purpose |
|----------------|---------|
| `AudioManager` | Singleton: music, SFX, countdown, victory, game over |
| `SetupAudio` editor tool | Wires clips from `Assets/Audios/` into scenes |
| Clips added | background, button, countdown, victory, piano game over |

---

## Visuals & URP

| Change | Detail |
|--------|--------|
| Pink materials | URP Lit materials for Ch03 (player) and Ch15 (guard) |
| `FixCharacterURPMaterials` | Editor menu: remap embedded `.dae` materials, fix normals |
| `CharacterMaterialApplier` | Runtime safety net on character models |
| `URPMaterialAutoFixer` | Additional URP shader repair helper |
| Normal maps | Problematic Mixamo normal maps stripped where they caused magenta |

---

## Code quality & death flow

| Change | Detail |
|--------|--------|
| `PickupBase.KillPlayer()` | Single path for health/hits → `Death()` → `GameManager` |
| Removed bad reload | `HealthDecrease()` no longer loads `MainGameL1` on death |
| `EnemyBase` | `OnTriggerEnter` + trigger collider for CharacterController player |
| Compile fixes | `velocity.y` on float; `MeshFilter.sharedMesh` in editor tools |

---

## Files to exclude from version control

- `Assets/.mcp_auth_token`
- `.screenshots/` (local playtest captures)
- `Library/`, `Temp/`, `Logs/` (Unity generated — already in `.gitignore`)

---

## Known minor issues

1. **TMP ▶ character** — LiberationSans SDF missing play icon; use `>` or add glyph to font asset.
2. **Debug.Log volume** — Many gameplay logs still enabled (PickupBase, EnemyBase, spawners).
3. **Repository name** — Folder `Gads-Music-game` vs display name **Rhythm Raiders**.

---

## Suggested commit message (summary)

```
Improve Rhythm Raiders: narration before countdown, game-over on catch/danger,
2-artifact win, jump/ground fix, slower enemy, audio system, URP character fixes
```

---

## Part 3 — Goethe-Institut playtest (11 June 2026, 18:00)

Source: five external players — see `feedback-summary.md`.

| Feedback | Players | Response | Status |
|----------|---------|----------|--------|
| UI needs work; bigger font | All, P2, P5 | Narration font 28pt; canvas scale fix | Done |
| Lower dialogue box | P4 | Narration panel anchored lower | Done |
| Artifacts more visible | P3, P4 | Music-note artifact scale increased | Done |
| Character bigger | P4 | Ch03 scale 0.015 → 0.02 | Done |
| Change "Hits left" | P4 | HUD: **Lives** + **Artifacts X / 2** | Done |
| AI dialogue clear / helpful | P1–P5 | Keep scripted tone; improve presentation only | Kept |
| More music | P2, P3 | Extra layers in AudioManager | Planned |
| Pickup/sneak sound link | P5 | Shared SFX palette | Planned |
| Slower base speed | P4 | Tune forwardSpeed | Planned |
| Too short / bugs | P4 | Level length + stability pass | Planned |
| Side seats / curtains | P1, P2 | Environment & transition art | Deferred |

Full plan: `goethe-improvements-plan.md`

---

## Part 3 — Earlier refinement (pre-Goethe)

Refinements below were prioritized after **community meetup feedback** (`feedback-summary.md`). Each item maps to feedback IDs.

| Feedback IDs | Change implemented | Visible in build? |
|--------------|-------------------|-------------------|
| U4, U1, N1 | Narration **before** countdown; `PlayAndWait`; Canvas scale (1,1,1) | Yes — briefing visible |
| V2 | SceneFader + legacy overlay fixes; no permanent black screen | Yes |
| V1 | URP character materials; Fix Character Pink Materials | Yes |
| G1 | Enemy chase speed reduced (~3 units) | Yes — fairer first run |
| G4, G5 | `KillPlayer()` → defeat anim → GameManager; lethal obstacles instant kill | Yes |
| G2 | Win gated at **2 artifacts** via `TryTriggerVictory()` | Yes |
| G3 | Foot `GroundCheck`; single jump per landing | Yes |
| U2, U3 | `UICursor` + `UIButtonRaycastFix` on Death/Victory HUD | Yes |
| N2 | Narrator / narrative audio `loop = false` | Yes (where clip assigned) |
| N3 | Title **Rhythm Raiders** on start UI | Yes |
| L1, L3 | Docs: `prompt-used.md`, `report.md`, README AI disclosure | Repo / showcase |

### Deferred (documented in `critical-feedback.md`)

| Feedback | Decision |
|----------|----------|
| L2 — Live Ollama dialogue in-game | Rejected: latency, scope, performance |
| Rhythm-timing mechanic | Deferred: conflicts with current runner scope |
| Multiplayer | Rejected: out of Part 3 scope |

### Showcase video

Final showcase must show **before/after** or verbally distinguish **original vs refined** behaviour (narration timing, game over on catch, working end-screen UI).

