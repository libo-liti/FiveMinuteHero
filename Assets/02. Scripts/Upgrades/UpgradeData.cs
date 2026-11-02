using UnityEngine;

namespace FiveMinuteHero.Upgrades
{
    // 레벨업 카드 하나의 데이터. 어떤 스탯을 어떻게 바꿀지는 하위 클래스가 Apply로 정한다.
    // 새 강화는 하위 클래스 에셋을 추가하기만 하면 되고, 후보 뽑기 코드(LevelUpController)는 바뀌지 않는다.
    // 추상 클래스는 에셋으로 만들 수 없어서 [CreateAssetMenu]는 하위 클래스에 붙인다.
    // 현재 레벨은 여기에 저장하지 않는다(SO 원본 수정 금지). LevelUpController가 런타임 사전으로 들고 있다.
    public abstract class UpgradeData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [Tooltip("{0} 자리에 이번 레벨의 수치가 들어간다. 예: 이동 속도 +{0}%")]
        [SerializeField] private string _descriptionFormat;
        [SerializeField] private Sprite _icon;
        // [0] = Lv1에서 더할 값, [1] = Lv2에서 더할 값(누적이 아니라 증분). 쿨다운 감소처럼 음수가 있어 [Min]을 붙이지 않는다.
        [SerializeField] private float[] _valuesPerLevel = new float[0];

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int MaxLevel => _valuesPerLevel.Length;

        // 기본은 항상 적용 가능. 무기 강화는 "그 무기를 가졌는지"로 덮어쓴다.
        public virtual bool CanApply(UpgradeContext context)
        {
            return true;
        }

        // 카드에 보일 문장. level은 이번에 새로 도달할 레벨.
        public abstract string GetDescription(int level);

        // level은 이번에 새로 도달하는 레벨. 그 레벨의 값 하나만 더한다(증분).
        public abstract void Apply(UpgradeContext context, int level);

        // level은 1부터 시작하므로 배열 인덱스는 level - 1. 범위 밖이면 알리고 0을 돌려 아무것도 더하지 않게 한다.
        protected float GetValue(int level)
        {
            if (level < 1 || level > MaxLevel)
            {
                Debug.LogError($"UpgradeData: 레벨 {level}은 범위(1~{MaxLevel}) 밖입니다.", this);
                return 0f;
            }

            return _valuesPerLevel[level - 1];
        }

        // 하위 클래스가 공통으로 쓰는 설명 만들기. 표시할 숫자만 넘기면 형식 문자열의 {0}에 넣는다.
        protected string FormatDescription(float displayValue)
        {
            // "0"은 소수점 없이 정수로 보여 준다(표시용일 뿐 계산에는 영향 없음).
            return string.Format(_descriptionFormat, displayValue.ToString("0"));
        }
    }
}
