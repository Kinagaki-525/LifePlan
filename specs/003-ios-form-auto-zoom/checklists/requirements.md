# Specification Quality Checklist: iOS端末でフォーム操作時に画面が自動拡大される問題の解消

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-10-12
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Notes

- [NEEDS CLARIFICATION] の 2 論点は、ユーザーの回答で解消した（spec.md の Clarifications、Session 2026-10-12）。
  1. 対象端末：タッチ操作が主の端末すべて。PC は変えない（User Story 4・6、FR-003、FR-012、SC-006、SC-008）
  2. 文字の大きさを変えるのは入力欄・選択欄・本文欄の中だけ（FR-009）
- 「現状」の文字の大きさ（px）、画面幅、viewport の記述は、原因と影響範囲を共有するための調査事実として残している。要件（FR・SC）は、利用者から見た挙動（倍率が変わらない、横スクロールがない）で書いている。
- 実機で未確認の点（16px の基準、iPad の挙動）は Assumptions に記載し、SC で実機確認を必須にしている。
