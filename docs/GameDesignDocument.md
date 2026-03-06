# Game Design Document

## Match-3 Mobile Game

**Version:** 1.0  
**Platform:** Mobile (iOS/Android)  
**Engine:** MonoGame  
**Language:** C#

---

## 1. Project Overview

A Match-3 puzzle game built with C# and MonoGame for mobile platforms. Players match 3 or more identical gems to clear them and earn points.

---

## 2. Game Screens

### 2.1 Main Menu
- Single screen with "Play" button
- Pressing "Play" starts the game level

### 2.2 Game Screen
- 8x8 game grid
- Score display
- Timer: 60 seconds per level
- 60 FPS performance target

### 2.3 Game Over Screen
- Displayed when no valid moves remain or timer is out
- "Game Over" message with "Ok" button
- "Ok" returns to main menu

---

## 3. Core Mechanics

### 3.1 Game Board
- Grid: 8x8 cells
- Gem types: 5 unique gems

### 3.2 Matching Rules
- Match 3+ identical gems in a row/column to clear
- Invalid moves (no match) are rejected

### 3.3 Timer
- Level time limit: 60 seconds
- No per-move timer
- Game ends when timer reaches zero

### 3.4 Scoring System
- Points awarded for each cleared gem
- Score formula based on destroyed elements (basic, Line, Bomb)

### 3.5 Gravity & Refill
- Gems fall smoothly (no instant movement)
- New gems spawn from top to fill empty cells

### 3.6 No Initial Bonuses
- Bonuses cannot appear in initial board generation
- Bonuses only spawn during player moves

---

## 4. Bonus System

### 4.1 Line Bonus
**Creation:** Match 5 gems in a row  
**Activation:** When matched  
**Effect:** Clears entire row OR column  
**Behavior:**
- Single Line: clears one direction (horizontal or vertical)
- Cannot be upgraded further

### 4.2 Bomb Bonus
**Creation:** Match 5 gems in L or T shape  
**Activation:** When matched  
**Effect:** 
- Destroys 3x3 area around the bomb (250ms delay for visual effect)
- Explodes regardless of proximity to the matched gems

**Behavior:**
- When part of a match, triggers chain reaction
- If Bomb is matched with regular gems, it explodes

---

## 5. Bonus Combination Rules

| Combination | Result |
|-------------|--------|
| Bonus + Regular gem | Bonus activates |

---

## 6. Performance Requirements

- **Target FPS:** 60
- **Animation:** Smooth, no stuttering
- **Consistent performance:** No FPS drops during gameplay

---

## 7. Visual Effects

| Trigger | Effect |
|---------|--------|
| Line bonus activated | Clears entire row OR column with destruction animation |
| Two intersecting Lines | Clears both row AND column (cross pattern) |
| Bomb activated | 250ms delay, then explodes 3x3 area |

---

## 8. Technical Stack

- **Language:** C#
- **Framework:** MonoGame
- **Target Platforms:** iOS, Android (mobile)

---

## 9. Out of Scope

- Sound/audio
- Animations (basic visual effects only)
- High scores/persistence
- Multiple levels
- In-app purchases
- Social features
