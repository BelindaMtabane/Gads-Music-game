# Final Reflection Report — Rhythm Raiders (Part 3)

**Student:** [Your name]  
**Module:** GADS7331 — Game Design 3A  
**Project:** Rhythm Raiders  
**Word count:** ~720  

---

## Professional engagement

Presenting Rhythm Raiders at an external meetup was qualitatively different from showing work to lecturers or classmates. Academic critique often maps to rubric language — LOs, documentation, evidence. Community feedback was **immediate and experiential**: “I died before I understood why,” “I can’t click this,” “your characters look broken.” That tone stripped away assumptions I had built during solo development with AI tools.

I learned to **structure a demo**: start scene for context, one intentional death, one path toward victory, then explain AI workflow only after people had touched the game. When I led with “I used Cursor and Ollama,” eyes glazed; when I let them play first, questions about AI became grounded in **what they saw on screen**.

The engagement also taught me **professional boundaries**. Attendees are not a free QA department — but their unstructured comments mirror what Steam reviews or playtest reports look like. Recording quotes by **role** (developer, designer, casual player) helped me sort signal from noise without dismissing non-technical voices. UX feedback from the casual gamer was as actionable as the Unity hobbyist’s collider comment.

---

## Feedback integration

The most valuable feedback cluster was **clarity before action**: narration must appear before the countdown, win condition must be stable at two artifacts, and failure must show defeat before the death scene. That aligned multiple isolated bugs (Canvas scale, narration timing, game-over routing) under one design principle: **respect the player’s attention in the first thirty seconds**.

Technical feedback on **pink materials** and **black screens** was valuable because it protected **credibility**. Players do not separate “render pipeline issue” from “student didn’t finish.” Fixing URP materials and fade behaviour was not vanity — it was **trust restoration**.

Less valuable in isolation were subjective taste comments (“make the guard scarier”) without actionable paths. I logged them but did not chase them unless they connected to readability — which **hazard telegraphing** partially addresses.

I rejected **live Ollama dialogue in-game** after weighing feasibility: Part 3 deadline, single-machine demo, and performance. I documented that rejection in `critical-feedback.md` so assessors see **judgement**, not laziness.

---

## Collaboration with AI

External feedback changed how I use AI tools, not just what I built. Previously I prompted Cursor with bug descriptions (“black screen,” “pink texture”) and accepted patches. After the meetup, prompts became **hypothesis-driven**: “Players cannot see narration — list UI causes besides missing script,” then “implement PlayAndWait before countdown,” then “verify Canvas scale in MainGameL1.”

Ollama’s role stayed **local and upstream**: drafting reflection paragraphs, suggesting playtest questions, explaining CharacterController grounding — work I do **not** submit as player-facing content. Attendee L1 (“don’t imply live AI in the game”) directly shaped my **showcase script** and README: AI is in the **development pipeline**, while shipped narration is scripted.

Prompt engineering after feedback also meant **smaller diffs**. Instead of “fix the game,” I used “reduce EnemyBase speed by 3,” “add KillPlayer single path,” “ensure UICursor unlock on DeathHUD.” That mirrors how studios ticket work from playtest PDFs.

---

## Ethical and professional considerations

**Transparency:** During the meetup I stated that dialogue lines were written and edited by me, that some development used Claude via Cursor, and that Ollama ran locally for planning — not for real-time player chat. I did not claim procedural storytelling in the build when it was not implemented.

**Ownership:** I treat AI output as **draft** until I understand and test it. Enemy catch going through `GameManager.TriggerGameOver()` was my architectural rule; AI suggested implementations, but I remain responsible for merges and regressions.

**Accuracy:** LLMs suggested fixes that compiled but were wrong contextually (e.g. referencing private `SceneFader` fields from editor scripts). External playtest proved **compile-green is not ship-green**. I now cross-check AI patches with play mode and console warnings.

**Future practice:** In industry I would disclose AI assistance on teams that require it, keep prompts and outputs for audit on critical systems, and never let tooling pressure ship opaque generated code in gameplay-critical paths without review. For public showcases I would label **what is AI-assisted in production** versus **what is AI inside the product** — a distinction this Part 3 process forced me to articulate clearly.

---

## Closing

Part 3 turned Rhythm Raiders from an AI-assisted prototype into something I would **show a stranger** without apologizing for the build. The refinement is not one feature — it is **professional closure**: play, fail, understand, retry, win or lose with dignity. That outcome came from community critique, selective integration, and honest documentation — not from automating every suggestion an LLM or attendee offered.

---

*Submitted with: `feedback-summary.md`, `critical-feedback.md`, updated `refinements-changes.md`, final build, showcase video, attendance evidence.*
