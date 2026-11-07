using System.Collections.Generic;
using UnityEngine;

// 움직이는 발판(체인 발판·자성 발판) 위에 올라탄 물체를 찾아 발판과 같은 거리만큼 함께 옮김
// → 발판이 위아래·좌우로 움직여도 올라탄 상자·봇이 떨지 않고 딱 붙어서 이동
// 상자 위의 봇처럼 위에 쌓인 것도 함께 옮김
public static class PlatformRiders
{
    private static readonly ContactPoint2D[] contacts = new ContactPoint2D[32];
    private static readonly Queue<Rigidbody2D> pending = new Queue<Rigidbody2D>();

    // 발판 콜라이더 위에 올라탄(쌓인 것 포함) 물체를 riders에 모음
    public static void Collect(Collider2D[] platformColliders, HashSet<Rigidbody2D> riders)
    {
        riders.Clear();
        pending.Clear();

        foreach (Collider2D platform in platformColliders)
        {
            if (platform == null || !platform.enabled || platform.isTrigger) continue;

            int count = platform.GetContacts(contacts);
            for (int i = 0; i < count; i++)
            {
                Collider2D other = contacts[i].collider == platform ? contacts[i].otherCollider : contacts[i].collider;
                TryAdd(other, contacts[i], riders);
            }
        }

        // 올라탄 물체 위에 또 올라탄 물체
        while (pending.Count > 0)
        {
            Rigidbody2D body = pending.Dequeue();
            int count = body.GetContacts(contacts);
            for (int i = 0; i < count; i++)
            {
                Collider2D other = contacts[i].collider.attachedRigidbody == body ? contacts[i].otherCollider : contacts[i].collider;
                TryAdd(other, contacts[i], riders);
            }
        }
    }

    // 모은 물체를 발판이 움직인 만큼 함께 옮김
    public static void Carry(HashSet<Rigidbody2D> riders, Vector2 delta)
    {
        if (delta == Vector2.zero) return;

        foreach (Rigidbody2D body in riders)
            if (body != null) body.position += delta;
    }

    // 접촉한 other가 아래 물체 위에 올라타 있으면 추가 (옆에 닿기만 한 것은 제외)
    private static void TryAdd(Collider2D other, ContactPoint2D contact, HashSet<Rigidbody2D> riders)
    {
        if (other == null || other.isTrigger) return;

        Rigidbody2D body = other.attachedRigidbody;
        if (body == null || body.bodyType != RigidbodyType2D.Dynamic || riders.Contains(body)) return;

        // 접촉 방향을 other 쪽을 향하도록 맞춘 뒤, 그 방향이 위쪽이면 올라탄 것
        Vector2 toOther = (Vector2)other.bounds.center - contact.point;
        Vector2 normal = Vector2.Dot(contact.normal, toOther) >= 0f ? contact.normal : -contact.normal;
        if (normal.y < 0.5f) return;

        riders.Add(body);
        pending.Enqueue(body);
    }
}
