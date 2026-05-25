# Texas TX17-4R Print — Form-Fidelity Review

Reviewed: 2026-05-25. Reference: `Documents/TexasCitation/UTC Texas.gif` (official G.A. Thompson
TX17-4R (NCR), 500×1077 px ≈ **3.6 in wide × 7.7 in tall**). Generator under review:
`Modules.Records.UI/Printing/CitationTexasPdfDocument.cs` (master @ 58cc669, after PR #78).

## Key realization that reframes the work

The official form is a **narrow ~3.6 in-wide** document. That means:
- It fits a **4 in thermal roll** at essentially 1:1 (4 in − margins ≈ 3.64 in usable — already our block width).
- The Letter variant just pins that same true-width block on the left.

So the "one shared block → two page sizes" architecture is **correct** and should stay. The problem is
**not width** — it is that the shared block stopped reproducing the State layout. PR #78's narrow
re-proportioning (splitting rows, dropping the margin strips, shrink-to-fit fills) traded fidelity for
"it fits." For a State form that is the wrong trade: it must match.

## A. Structural omissions (these fail approval outright)

The official form has **rotated vertical fields in both side margins**. The generator renders none of
them as vertical; it collapsed five of them into horizontal boxes ("side strip") stacked under the
offense panel, and dropped the rest entirely.

**Left margin (vertical, rotated, bottom-to-top), flanking the offense box:**
- A1. `ACCIDENT CASE` (bold) + a checkbox, with `Leading Causes of Accidents` beneath — beside the main
  SPEEDING/violation grid. **Missing entirely.**
- A2. `Conditions that Increased Seriousness of Violation` — beside the lower slippery/darkness section.
  **Missing entirely.**

**Right margin (vertical, rotated):**
- A3. Two checkboxes captioned `Arrest-Delivered to`. **Missing entirely.**
- A4. `Accepted Bond-Amt. or Type` fill line — present but rendered as a horizontal box, not vertical.
- A5. `Receipt No.` fill line — present but horizontal, not vertical.
- A6. Tall vertical fill lines `NAME`, `Occupation`, `Social Security Number` down the far-right edge —
  present as horizontal boxes, not vertical.

## B. Label-format infidelities (font/layout particulars)

- B1. **NAME line.** Official: one continuous underline with four sub-labels *beneath* it —
  `LAST` · `(PLEASE PRINT)` · `FIRST` · `INITIAL`. Generator: three separate inline-labeled fields
  `NAME (LAST)` / `(FIRST)` / `(INIT)`. Wrong structure.
- B2. **BIRTH DATE label is stacked** — `BIRTH` on line 1, `DATE` on line 2 — sitting left of its fill.
  Generator prints `BIRTH DATE` inline on one line. (User-cited example.)
- B3. **AGE…WT is a single line.** Official packs `AGE / BIRTH DATE / RACE / SEX / HT. / WT.` on **one**
  line. Generator splits it across **two** rows (AGE/BIRTH DATE/RACE, then SEX/HT/WT).
- B4. **DRIV. LIC. label is stacked** — `DRIV.` over `LIC. No.` — and `STATE` / `KIND` are sub-labels
  *under* the same continuing line, with `DID UNLAWFULLY OPERATE (PARK)` trailing at the right end of
  that same line. Generator: inline `DRIV. LIC. NO.` on its own row, with `(PARK)` on a separate
  right-aligned line.
- B5. **Checkbox-after-label.** `COMMERCIAL VEHICLE □` and `HAZARDOUS MATERIAL □` put the box **after**
  the label. Generator's `Check()` primitive always puts the box **before** the caption.
- B6. **Shrink-to-fit data fonts.** `FitFontSize` scales fill text down by length. On a State form the
  rendered entries should be a single consistent type size, not variable per field length.

## C. Font

- C1. The form face is a plain grotesque (Helvetica/Arial family). We currently force **Lato** because
  Linux App Service lacks Arial (that font gap previously crashed every render — see
  `project_citation_pdf_linux_font`). Lato's proportions differ from Arial and may itself be an approval
  problem. Fix path: **bundle a metric-compatible Arial substitute** (e.g. Liberation Sans / Arimo,
  which are metric-identical to Arial and freely licensed) and register it, so it renders on Linux *and*
  matches the State face. Do NOT go back to system "Arial".

## Disposition

A1–A6 and B1–B5 are layout-correctness defects against the State master. This is a faithful-reproduction
rewrite of `ComposeForm`, keeping the shared-block / two-page-size architecture. C1 is a parallel
font-bundling change. Recommend a new branch off master; re-verify by emitting both PDFs and overlaying
against `UTC Texas.gif`.
