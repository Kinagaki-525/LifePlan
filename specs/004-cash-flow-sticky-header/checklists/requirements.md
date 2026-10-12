# Specification Quality Checklist: スマートフォン版キャッシュフロー表の年度ヘッダーに年齢を表示し、縦スクロール中も固定する

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

- 仕様化の前に、3 つの論点をユーザーの回答で確定した（spec.md の Clarifications、Session 2026-10-12）。
  1. 年齢の表示はスマートフォン幅（575px 以下）のみ。PC 幅は西暦だけのまま（FR-001、FR-007、User Story 4）
  2. 年齢が空の年度は「夫 -」と表示する（FR-004、User Story 1 シナリオ 3、SC-002）
  3. ページの縦スクロールに追従して固定する。表の枠の中での縦スクロールは採らない（FR-009）
- 「現状」の画面幅（575px）と、横スクロールの仕組みのためにヘッダーが固定されないという記述は、原因と影響範囲を共有するための調査事実として残している。要件（FR・SC）は、利用者から見た挙動（表示内容、固定・解除、列のずれがないこと）で書いており、固定の方式は plan で比較して決める。
- 正本 `docs/simulator/index.md` への反映案は [docs-update-proposal.md](../docs-update-proposal.md)（未反映・承認待ち）。
- ユーザーの指示で、実機以外でできる範囲の確認（SC-011〜SC-017：自動テスト、ブラウザの自動操作による数値の確認、修正前後の比較、JavaScript 無効時、アクセシビリティツリー）と、横スクロールを壊さない要件（FR-020）を追加した。SC は確認方法の粒度で書いているが、特定のツール名は plan で決める。
- iPhone Safari の挙動（慣性スクロール、アドレスバーの伸縮）は実機でしか確認できないため、SC-010 で実機確認を受け入れの条件にしている。
