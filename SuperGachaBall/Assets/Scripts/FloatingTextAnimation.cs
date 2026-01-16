using UnityEngine;
using TMPro;

public class FloatingTextAnimation : MonoBehaviour
{
    public float moveSpeed = 100f;
    public float lifeTime = 1.0f;
    public float fadeSpeed = 2.0f;

    private TextMeshProUGUI textMesh;
    private float timer = 0f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Move up
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Fade out
        timer += Time.deltaTime;
        if (timer > lifeTime * 0.5f) // Fade halfway through lifetime
        {
            if (textMesh != null)
            {
                float alpha = Mathf.Lerp(textMesh.color.a, 0f, Time.deltaTime * fadeSpeed);
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
            }
        }

        // Destroy
        if (timer > lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
