# Requirements and acceptance criteria

Verified against the instructor's `YMH219_Ek_Uygulama_Odevleri_Ogrenci_Foyu.pdf` on 2026-11-16: page 1 for common submission rules, page 2 for Flappy Bird, and page 5 for README content, assessment, and final checks. The classroom announcement supplied by the owner confirms profile-link submission and the deadline. See [PLAN.md](PLAN.md) for the delivery plan.

## Required gameplay

| ID | Requirement | Acceptance criterion | Status / intended stage |
| --- | --- | --- | --- |
| R01 | Bird input | Keyboard moves the bird upward, with a documented held-key policy; our implementation uses KeyDown/KeyUp | Merged in PR #3; GameForm, FlapInput, Bird, input/engine tests |
| R02 | Gravity | Bird vertical velocity changes over elapsed time; tests verify the update contract | Merged in PR #3; BirdTests, FixedStepLoopTests |
| R03 | Obstacles | At least two obstacles move right to left; our design uses at least two pipe pairs with passable gaps | Merged in PR #4; geometry/rendering tests |
| R04 | Recycling | Off-screen obstacles reappear to the right with valid gaps and spacing | Merged in PR #4; boundary/full-cycle/long-run tests retained |
| R05 | Collision | Pipe or ground contact ends the current run with documented boundary rules | Merged in PR #5; CollisionDetectorTests/GameRulesTests retained |
| R06 | Game Over | Gameplay updates stop and the final score is displayed | Merged in PR #5; frozen-state and renderer/window tests retained |
| R07 | Score | Each successfully passed obstacle scores once | Merged in PR #5; exact threshold, eligibility, ordering tests retained |
| R08 | Restart | New run resets entities, score, game state, and input state consistently | Merged in PR #5; repeated Core reset/STA Form restart tests retained |
| R09 | OOP | Bird, obstacles, and game state have separate responsibilities; UI delegates gameplay | Core owns rules/state/score; App owns input/timer/rendering/native restart button |

The PDF requires at least two obstacles rather than specifying pipe pairs. Our plan uses at least two active `PipePair` objects, each with upper/lower pipes, to satisfy the minimum clearly. Page 2 permits KeyDown/KeyUp or PreviewKeyDown; KeyDown/KeyUp is our chosen input approach.

## Optional enhancements

| Enhancement | Acceptance condition if selected | Status |
| --- | --- | --- |
| Persistent high score | Survives restart/application relaunch; storage failures handled | Merged in PR #6; FileHighScoreStoreTests, HighScoreServiceTests, GameFormRulesTests; [storage contract](HIGH_SCORE.md) |
| Progressive speed | Predictable bounded difficulty progression with tests | Deferred; no progression added |
| Audio or difficulty levels | Clear controls and stable gameplay behavior | Deferred; no audio or selector added |

High score is the only selected optional enhancement. Stage 05 improves controls, score/best contrast, pause explanation and native restart focus/scaling without changing mandatory gameplay/settings. Human difficulty and physical input/display review remain pending. Stage 06 provides screenshots, clean-checkout and public-access evidence; identity/owner/second-computer sign-off is still pending.

## Quality and process

- Windows Forms application, C#, clear class responsibilities, and readable code.
- App/Core/Tests separation, meaningful tests, and Windows CI.
- Distinct stage branches, honest commit history, reviewed PRs, no automatic merging.
- English Markdown documentation aligned with implemented behavior and actual verification.
- Student can explain encapsulation, events, timer updates, dependencies, and reset behavior.

## Required repository and README deliverables

- One separate **public** repository for this assignment on the student's own GitHub account (PDF page 1). The owner selected `alaamadii/FlappyBird`; the instructor recommends `YMH219-FlappyBird`. With owner approval it became public on 2026-11-17; anonymous repository page/API access and profile listing were verified.
- Buildable source, `.gitignore`, and README; exclude `bin`, `obj`, `.vs`, and other generated files.
- README must include project name/purpose, actual technologies and NuGet packages, setup/run steps, basic features, at least **two embedded real screenshots**, known limitations/development notes, and the student's full name and student number (pages 2 and 5).
- Meaningful commits must show the real development process; a single final-day upload may lose Git/GitHub credit.
- Test opening/running the project on another computer (page 5 submission checklist).
- Push the final work by **2026-11-30 23:59**. Later commits may be treated as late. Use Asia/Riyadh for this working schedule; confirm the institution's timezone if different.
- The student submits only their GitHub profile URL in Classroom; the instructor inspects the five separate repositories through that profile. This repository and its plan cover Flappy Bird only.

The instructor lists Visual Studio/C#/.NET/Windows Forms. VS Code is the owner's development choice; it does not change the required Windows Forms deliverable. English documentation, xUnit, CI, custom GDI+ drawing, and PR stages are our project practices, not extra instructor requirements.

### Owner direction on public identity — 2026-11-18

The owner explicitly chose to omit the student number from the public README. The placeholder/request for that value is removed, and the assistant will not collect or publish it for this README. The full name remains pending. The original PDF requirement above is preserved as source attribution; omission is an owner-directed exception, not a claim that the instructor's student-number requirement has been fulfilled or waived.

## Evaluation weights verified against PDF page 5

| Area | Points |
| --- | ---: |
| Working application and required behavior | 40 |
| OOP, code organization, error handling | 20 |
| GitHub and commits | 15 |
| User interface | 10 |
| README and documentation | 10 |
| Explaining the code | 5 |
| Total | 100 |

These weights were checked against the instructor's page-5 assessment table.
