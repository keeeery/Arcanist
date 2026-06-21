using UnityEngine;
using TMPro;
using System.Collections;

namespace Utilities
{
    // FloatingText
    // - 미디어나 게임 개발, 모바일 앱 환경 등에서 화면 위에 떠다니거나
    //   둥둥 떠오르는 듯한 텍스트 효과나 위젯 등

    // 목표
    // - FloatText의 특정 함수를 이용해서 위로 올라가는 연출과 사라지는 듯한 효과 구현

    public class FloatingText : MonoBehaviour
    {
        [field: SerializeField] public float VerticalSpeed { get; private set; } = 1.0f;
        [field: SerializeField] public float LifeTime { get; private set; } = 2.0f;

        private TMP_Text text;
        private Renderer textRenderer;
        private Color originColor;
        private float progressTime = 0.0f;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            textRenderer = GetComponent<Renderer>();
            textRenderer.sortingOrder = 1000;
        }

        public void Show(string message, Color color, Vector3 worldPosition)
        {
            text.SetText(message);
            originColor = color;
            transform.position = worldPosition;
            StartCoroutine(AnimateCoroutine());
        }

        private IEnumerator AnimateCoroutine()
        {
            progressTime = 0.0f;
            // 원본 색상을 복사하여 알파만 0인 색상 생성
            Color targetColor = originColor;
            targetColor.a = 0.0f;

            while (true)
            {
                if (progressTime > LifeTime) break;

                Color nowColor = Color.Lerp(originColor, targetColor, progressTime / LifeTime);
                text.color = nowColor;

                transform.Translate(Vector3.up * VerticalSpeed * Time.deltaTime);

                yield return null;
                progressTime += Time.deltaTime;
            }

            gameObject.SetActive(false);
        }
    }
}
