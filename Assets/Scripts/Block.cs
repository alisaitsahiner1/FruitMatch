using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    public int x;
    public int y;
    public BoardManager boardManager;

    private Vector3 targetPosition;
    private float moveSpeed = 10f;

    private Vector3 originalScale;
    private float squashAmount = 0.15f;
    private float squashSpeed = 10f;
    private bool isSquashing = false;
    private bool hasReachedTarget = true; // Başta hedefe ulaşıldı gibi davran
  

    private void Start()
    {
        originalScale = transform.localScale;
        transform.position = boardManager.GetWorldPosition(x, y); // İlk pozisyonu doğru yerleştir
        targetPosition = transform.position; // Hedef pozisyonu da aynı yap
        hasReachedTarget = true; // Başlangıçta hareket yok
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (!hasReachedTarget && Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            hasReachedTarget = true;
            if (!isSquashing)
            {
                StartCoroutine(DoSquashEffect());
            }
        }
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
        targetPosition = boardManager.GetWorldPosition(x, y);
        hasReachedTarget = false; // Yeni hedefe gidiyor
    }

    public void ForceUpdatePosition()
    {
        transform.position = boardManager.GetWorldPosition(x, y);
        targetPosition = transform.position;
        hasReachedTarget = true; // Zorla konumla, squash yapmasın
    }

    private IEnumerator DoSquashEffect()
    {
        isSquashing = true;

        Vector3 squashed = new Vector3(originalScale.x + squashAmount, originalScale.y - squashAmount, originalScale.z);

        // Squash
        float t = 0f;
        while (t < 1f)
        {
            transform.localScale = Vector3.Lerp(originalScale, squashed, t);
            t += Time.deltaTime * squashSpeed;
            yield return null;
        }

        // Stretch back
        t = 0f;
        while (t < 1f)
        {
            transform.localScale = Vector3.Lerp(squashed, originalScale, t);
            t += Time.deltaTime * squashSpeed;
            yield return null;
        }

        transform.localScale = originalScale;
        isSquashing = false;
    }
}
