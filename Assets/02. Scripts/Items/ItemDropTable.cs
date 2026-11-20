using System;
using UnityEngine;

namespace FiveMinuteHero.Items
{
    // "무엇이 몇 % 확률로 나오는가" 표. 확률은 아이템(ItemData)이 아니라 이 표가 든다.
    // 그래서 같은 회복 아이템을 적 처치 표에서는 2%, 맵 상자 표에서는 50%로 둘 수 있다. 표는 아이템 종류(하위 클래스)를 모른다.
    [CreateAssetMenu(fileName = "ItemDropTable", menuName = "FiveMinuteHero/Items/Drop Table")]
    public class ItemDropTable : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField] private ItemData _item;
            [Tooltip("한 번 굴릴 때 이 아이템이 나올 확률(0~1). 표 전체 합은 1 이하여야 하고, 1에서 모자란 만큼이 '안 나옴'이다.")]
            [SerializeField, Range(0f, 1f)] private float _chance;

            public ItemData Item => _item;
            public float Chance => _chance;
        }

        [SerializeField] private Entry[] _entries = new Entry[0];

        // 확률 합. 1을 넘으면 뒤쪽 항목이 표에 적힌 확률보다 적게 나온다. 쓰는 쪽이 시작할 때 검사한다.
        public float TotalChance
        {
            get
            {
                float total = 0f;
                foreach (Entry entry in _entries)
                {
                    total += entry.Chance;
                }
                return total;
            }
        }

        // roll은 [0, 1) 난수 하나. 확률을 앞에서부터 쌓아 가며 roll이 들어가는 칸의 아이템을 돌려주고, 어느 칸에도 없으면 null(안 나옴).
        // 예: 회복 0.02, 자석 0.005 → [0, 0.02)는 회복, [0.02, 0.025)는 자석, 나머지는 없음.
        // 아이템마다 따로 굴리지 않고 한 번만 굴리므로, 적 하나가 아이템을 둘 떨어뜨리는 일이 없고 확률도 표에 적힌 그대로다.
        // 난수를 밖에서 받는 이유: 어떤 난수를 쓸지(시드, 다른 시스템과 섞이지 않게)는 굴리는 쪽이 정한다.
        public ItemData Pick(double roll)
        {
            double cumulative = 0d;
            foreach (Entry entry in _entries)
            {
                cumulative += entry.Chance;
                if (roll < cumulative)
                {
                    return entry.Item;
                }
            }

            return null;
        }
    }
}
