# Game Design Document (GDD): "Approve, Please" (Working Title)

## 1. Project Overview

* **Genre:** 2D Simulation / Puzzle (Papers, Please style)
* **Core Loop:** Receive Document Set -> Inspect for Discrepancies -> Stamp APPROVE or REJECT -> Day Ends -> Result (Pass/Fail) -> Advance to next day.
* **Development Scope (MVP):** 3-Day Game Jam optimized. Focus purely on 2D UI dragging, text parsing, and stamping feedback.

## 2. Core UI & Player View

* **Perspective:** Top-down view of a wooden office desk.
* **UI Layout:**
* **Center:** The working area where documents spawn.
* **Right Side:** Two draggable Stamps (Green `[APPROVE]`, Red `[REJECT]`).
* **Top Left:** Clock (e.g., 09:00 to 18:00) and Date.
* **Top Right:** Daily Quota counter (e.g., "Processed: 0/15") and Penalty Strikes (3 Warnings).
* **Bottom Left (Rulebook):** A small togglable UI showing Company Guidelines (Memos, Blacklisted Vendors, Access Levels).



## 3. Game Mechanics: Interaction & Stamping

* **Drag & Drop:** All documents are 2D UI Panels with Colliders/Raycast Targets. The player can drag them around freely. Clicking a document brings it to the top sorting layer.
* **The Stamp System (Crucial for Game Feel):**
* Player drags the physical Stamp tool over a document.
* On mouse-release, a raycast checks if it hit the **Action Proposal** document.
* If yes: Play loud `[STAMP_SFX]`, instantiate a permanent Stamp mark Sprite on the document.
* The entire set of documents instantly sweeps off the screen, and the next set slides in.
* Backend logic instantly evaluates if the decision was Correct or Incorrect.



## 4. Document System (The "3-Combo" Set)

Every case spawns exactly 3 connected documents for cross-referencing:

1. **HR Profile Card (The Truth):** Photo, Name, Dept, Employee ID, Security Level (1 to 5).
2. **Action Proposal (The Request):** Date, Request Title, Requested Amount/Access, Vendor/Target, Signature.
3. **Attached Evidence (The Variable):** Changes based on the request (Receipt, Access Log, QA Report, VIP Reference).

---

## 5. The 5 Villains (Generation Logic & Discrepancies)

**Dev Note:** For document generation, create a randomized data structure. 50% of spawned cases are "Innocent" (all data matches). 50% are "Villains" (contains specific programmed discrepancies).

### Villain 1: The Embezzler (횡령범)

* **Goal:** Steal money directly.
* **How to spot (Discrepancy):**
* The `Requested Amount` on the **Action Proposal** is drastically different from the `Total Amount` on the **Attached Evidence (Receipt)**.
* *Example:* Proposal asks for $15,000, but the actual receipt is for $1,500 (extra zero added).


* **Action:** REJECT.

### Villain 2: The Kickback Taker (리베이트 헌터)

* **Goal:** Funnel company money to shady external vendors for bribes.
* **How to spot (Discrepancy):**
* The `Vendor Name` on the **Action Proposal** matches a company listed on the player's **Rulebook (Blacklist Memo)**.
* *Alternative:* The Unit Price on the receipt is absurdly high for a basic item (e.g., $5,000 for a standard office chair).


* **Action:** REJECT.

### Villain 3: The Golden Parachute / Nepotism (무임승차 낙하산)

* **Goal:** Abuse VIP connections to bypass rules despite low qualifications.
* **How to spot (Discrepancy):**
* The **HR Profile Card** shows low rank/security (e.g., Level 1 Intern).
* The **Action Proposal** requires Level 5 Authorization (e.g., "Executive Expense Account").
* The **Attached Evidence** is a `VIP Reference Letter`, but the Signature is misspelled (e.g., "Chairperson J. Smilh" instead of "J. Smith"), or the Date on the letter is expired (e.g., Dated 2022 in a 2026 game).


* **Action:** REJECT.

### Villain 4: The Tech Runner (기술 먹튀범)

* **Goal:** Steal internal core data/assets before resigning.
* **How to spot (Discrepancy):**
* **Action Proposal** claims "Standard Overtime for Marketing Project".
* The **Attached Evidence (Server Access Log)** shows entry into `[Core Server Room]` (which mismatches their Marketing dept) OR shows an abnormal Data Transfer size (e.g., Download: 500 Terabytes).


* **Action:** REJECT.

### Villain 5: The Insider Saboteur (공매도 결탁자/내부 트롤러)

* **Goal:** Ruin the company's product launch intentionally.
* **How to spot (Discrepancy):**
* The **Action Proposal** requests "Final Approval for Game Release".
* The **Attached Evidence (QA Report)** has clear visual tampering (e.g., The word `FAIL` is crossed out with a red marker, and `PASS` is scribbled next to it).
* *Alternative:* An Anonymous Leak Printout is attached. The IP address at the bottom of the leak perfectly matches the `Workstation IP` listed on the employee's **HR Profile Card**.


* **Action:** REJECT.

---

## 6. Win & Loss Conditions (The Core Penalty System)

The player's role as the HR/Approval Officer is strictly evaluated at the end of each day.

* **Correct Decision:**
* Approved an Innocent employee.
* Rejected a Villain (with discrepancies).
* *Result:* +1 to Daily Quota.


* **Incorrect Decision (Strike Penalty):**
* **False Positive:** Rejected an Innocent employee (Fired an innocent person).
* **False Negative:** Approved a Villain (Let the company get ruined).
* *Result:* Player receives a `STRIKE` (A loud buzzer plays, and a red warning slip prints out on the desk instantly).



### 💀 Game Over (Loss)

* **Condition:** Accumulate 3 Strikes within a single day.
* **Narrative:** The player is immediately fired.
* **Screen:** "YOU ARE FIRED. The company lost too much money (or you sued the wrong people). Clean out your desk." -> Restart Game.

### 🎉 Game Clear (Win)

* **Condition:** Survive for X Days (e.g., 5 Days) meeting the daily quota without being fired.
* **Narrative:** The company successfully launches its product and stabilizes, thanks to your ruthless auditing.
* **Screen:** "CONGRATULATIONS. You are granted a 3-Month Paid Vacation. See you when you get back."
* **Post-Clear Mechanic:** After the screen fades out, it loops back to Day 1 with faster time and harder, subtle discrepancies (Infinite Loop/Re-game).

## 7. Audio & Asset Guidelines for Dev Team

* **Art:** Minimalist UI. White/Yellow paper textures, simple Arial/Courier fonts to look like official documents. Use generic silhouette avatars for ID photos.
* **SFX (Mandatory for Game Feel):**
* `Paper_Shuffle.wav` (When dragging documents).
* `Stamp_Heavy.wav` (A very satisfying, heavy thud when stamping).
* `Buzzer_Error.wav` (When a strike is given).
* —Optional— `Clock_Tick.wav` (Subtle background noise to increase tension).

