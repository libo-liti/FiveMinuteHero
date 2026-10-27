using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 플레이어가 가고 싶은 방향을 주는 쪽의 계약. 키보드·조이스틱과 벤치마크용 자동 봇을 바꿔 끼우려고 분리했다.
    public interface IMoveInput
    {
        // 길이 0~1을 기대한다. 매 프레임 읽으므로 값을 바꾸는 부작용 없이 돌려준다.
        Vector2 Direction { get; }
    }
}
