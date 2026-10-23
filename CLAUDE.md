# FiveMinuteHero

Unity 6000.3 LTS · URP 2D · Android(세로 고정) · Input System 전용.
모바일 뱀서류(스테이지 3 × 5단계, 한 판 5분 / 보스 단계 7분). 취업 포트폴리오용 1인 프로젝트.
기획서·결정 로그는 사용자의 Notion에 있다. 이 파일과 충돌하면 사용자에게 먼저 묻는다.

## 함께 일하는 방식 (가장 중요)
- 사용자는 신입 Unity 개발자이고, 모든 코드를 면접에서 직접 설명할 수 있어야 한다.
- **구현 전에** 설계 선택지 2~3개와 트레이드오프를 짧게 설명하고, 사용자의 결정을 기다린다. 사소한 구현(오타, 이름 변경)은 예외.
- 한 번에 한 기능만, 작게 진행한다. 요청 범위 밖의 파일은 고치지 않는다.
- 구현 후: 핵심 줄이 무엇을 왜 하는지 설명하고, 이해 확인 질문 1~2개를 던진다(답은 바로 주지 않는다).
- 기능을 마치면 아래를 출력한다.
  - 포트폴리오 기록 포인트: 상황 → 원인 → 선택한 해결 → 결과(가능하면 수치)
  - AI 수정 기록 후보: 처음 작성안에서 틀렸거나 고친 부분이 있었다면 무엇을 왜
  - 커밋 메시지와 PR 본문 초안 (형식은 아래)
- git 커밋·푸시·브랜치 작업은 하지 않는다. 사용자가 GitKraken으로 직접 한다.
- 설명과 주석은 한국어로 쓴다.

## 구조
```
Assets/
  01. Scenes/ 02. Scripts/ ...     게임 쪽 (번호 폴더)
  02. Scripts/FiveMinuteHero.Game.asmdef
  ThirdParty/                      구매·무료 에셋 (git에서 원본 제외, .meta만 커밋)
Packages/com.jonghyun.core/        공통 모듈 임베디드 패키지
  package.json
  Runtime/<Module>/Jonghyun.Core.<Module>.asmdef
```
- 현재 asmdef: `Jonghyun.Core.Perf`(참조 없음), `FiveMinuteHero.Game`(→ Core.Perf, Unity.InputSystem).
- 모듈(Core.Save, Core.Ads, Core.Analytics, Core.Assets, Core.Pool)은 **실제로 쓸 때** 하나씩 추가한다. 미리 만들지 않는다.
- 네임스페이스: 게임은 `FiveMinuteHero.<영역>`, 모듈은 `Jonghyun.Core.<Module>`.

## 의존 규칙 (asmdef로 강제)
- 의존은 Game → Core 한 방향뿐. Core는 Game을 절대 참조하지 않는다.
- **Core에는 게임 용어가 없다.** "부활", "골드", "적", 광고 placement ID, 분석 이벤트 이름은 Game의 상수나 SO에 둔다. Core 코드는 다른 게임에서 수정 없이 쓸 수 있어야 한다.
- 외부 SDK(AdMob, Firebase)는 `Jonghyun.Core.Ads.AdMob` 같은 구현 asmdef에서만 참조한다. Game 코드에 SDK `using`이 있으면 안 된다.
- `GameEvents`(이벤트 버스)는 Game 내부 통신용이다. Core는 모른다.
- 새 asmdef를 만들거나 참조를 추가할 때는 이유를 설명하고 사용자 확인을 받는다.

## 설계 원칙
- 저결합: 시스템 간 직접 참조 대신 인터페이스와 C# 이벤트.
- 서비스 주입은 **방식 B(수동 주입)**: Bootstrap이 구현체를 만들어 `Init(...)`으로 넘긴다. ServiceLocator·싱글턴 남발·DI 프레임워크는 쓰지 않는다.
- 데이터는 ScriptableObject로 분리한다. 15단계 난이도 = 스테이지 SO 3개 + 난이도 배율 표(5줄). 단계마다 SO를 복제하지 않는다.
- 스프라이트·수치는 SO가 참조하게 해서, 에셋 교체 시 코드가 바뀌지 않게 한다.

## 성능 작업 규칙
- `perf-baseline` 태그 전(1주차)에는 **평범하게** 구현한다. 오브젝트 풀링, 공간 분할, 매니저 일괄 Update 같은 최적화를 미리 넣지 않는다. 대신 일부러 비효율적으로 짜지도 않는다(GetComponent 캐싱 같은 기본기는 지킨다).
- 최적화는 항상 Profiler로 원인을 확인한 뒤 진행하고, 측정 조건(노트20, Release 빌드, 고정 시드, 적 100/300/500/1000, 60초 3회 중앙값)을 바꾸지 않는다.
- 최적화 PR에는 Before/After 수치를 남긴다.

## 코드 규칙
- private 필드 `_camelCase`, 인스펙터 노출은 `[SerializeField] private`.
- `FindObjectOfType`·`GameObject.Find`를 런타임 로직에 쓰지 않는다. 참조는 인스펙터나 Init 주입으로.
- 매직 넘버 대신 SO 또는 상수.
- 코드를 바꾼 뒤에는 Unity 콘솔에서 컴파일 에러가 없는지 확인한다(Unity MCP가 연결되어 있으면 사용).

## 건드리지 않는 것
- `Assets/ThirdParty/`의 원본 파일 (라이선스상 재배포 금지).
- `ProjectSettings/`, `Packages/manifest.json`: 바꿔야 하면 이유를 설명하고 먼저 묻는다.

## 커밋·PR 형식
- 브랜치: `feat/` `fix/` `perf/` `refactor/` `chore/` `docs/` + 짧은 영어 이름 (예: `feat/player-movement`)
- 커밋: `feat: 가상 조이스틱 이동` 처럼 머리말 + 한국어 요약
- PR 본문: `## 변경 내용` / `## 왜` / `## 확인` 세 칸. 최적화 PR은 `## 확인`에 Before/After 표.
