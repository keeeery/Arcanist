# Arcanist (공개용 사본)

포트폴리오 첨부용으로 공개한 저장소입니다.
원래 두 저장소에 나뉘어 있던 커밋 이력을 한 줄로 잇고, **외부 에셋만 모든 커밋에서 제외**했습니다.
커밋 메시지·작성자·날짜는 원본과 같고, 커밋 해시는 다릅니다.

- `Initial Commit` ~ `카드 사용 연출` (2026-06-02 ~ 06-24): 개인 저장소에서 작업한 이력
- `마이그레이션` 이후: 학원 저장소(KooHoo-Dev/3rd-2D-Project-keeeery)로 옮겨 이어 간 이력.
  `마이그레이션` 커밋에는 옮길 때 바뀐 부분만 담겨 있고, 학원 템플릿 저장소의 첫 커밋은 생략했습니다.

## 제외한 것

라이선스상 재배포할 수 없거나 출처가 제 것이 아닌 파일입니다.

| 경로 | 내용 |
|---|---|
| `Assets/99. External Assets/` | 외부 아트·UI·셰이더 에셋 (Modern UI Pack, AllIn1SpriteShader, 4000 Fantasy Icons, Gothicvania Magic Pack 5, VFX Free Pack, Kings and Pigs, Pirate Bomb 등) |
| `Assets/10. Resources/Background/SSW_SET5_v1.0/` | 외부 배경 세트 |
| `Assets/11. Audio/BGM/` 중 2곡 | 스테이지·보스 BGM (외부 음원) |
| `Assets/Plugins/Demigiant/` | DOTween |
| `Assets/12. Tools/LDtk/` 안의 PNG | 외부 타일셋으로 그려진 맵 이미지 (`.ldtk` 맵 파일은 남김) |

## 열어 볼 때

- Unity `6000.3.15f1`
- 위 에셋이 빠져 있어 **그대로는 실행되지 않습니다.** DOTween·Modern UI Pack·AllIn1SpriteShader를 쓰는 스크립트는 컴파일 오류가 나고, 씬·프리팹의 스프라이트·이펙트 참조는 비어 있습니다.
- 코드는 `Assets/02. Scripts/`에 있습니다.

## 포함된 폰트

`Assets/TextMesh Pro/`의 Source Han Serif K는 SIL Open Font License 1.1, LiberationSans는 같은 라이선스(`LiberationSans - OFL.txt`)로 배포되는 폰트입니다.
