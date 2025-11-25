using UnityEngine;
using System.Collections;

public class BlockBounceNative : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float duration = 0.4f;
    public float destroyDuration = 0.15f; // Durasi menghilang (lebih cepat lebih enak)
    [Range(0, 1)] public float squashAmount = 0.3f;

    // Kurva Muncul (Spawn)
    public AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0f, 0f),       
        new Keyframe(0.3f, 1.2f),   
        new Keyframe(0.6f, 0.9f),   
        new Keyframe(1f, 1f)        
    );

    // Kurva Menghilang (Destroy)
    // Logika: Mulai 1 -> Naik dikit (Antisipasi) -> Terjun ke 0
    public AnimationCurve destroyCurve = new AnimationCurve(
        new Keyframe(0f, 1f), 
        new Keyframe(0.2f, 1.1f), 
        new Keyframe(1f, 0f)
    );

    private Vector3 initialScale;
    private Collider myCollider; // Referensi ke collider

    private void Awake()
    {
        initialScale = transform.localScale;
        
        // Cari collider di object ini atau parentnya (sesuai setup kamu)
        myCollider = GetComponent<Collider>();
        if (myCollider == null) myCollider = GetComponentInParent<Collider>();
    }

    private void OnEnable()
    {
        // Setup awal spawn
        transform.localScale = Vector3.zero;
        if (initialScale == Vector3.zero) initialScale = new Vector3(0.52f, 0.52f, 0.52f);
        
        // Pastikan collider nyala saat muncul
        if (myCollider != null) myCollider.enabled = true;

        StartCoroutine(AnimateBounce());
    }

    // ---------------------------------------------------------
    // FUNGSI BARU UNTUK MENGHAPUS
    // ---------------------------------------------------------
    public void StartDestroySequence()
    {
        // Hentikan animasi spawn jika masih berjalan
        StopAllCoroutines();
        
        // Matikan collider supaya player langsung jatuh/tembus
        // Ini memberikan feedback instan ke player bahwa blok sudah "hilang" secara fisik
        if (myCollider != null) myCollider.enabled = false;

        StartCoroutine(AnimateDestroy());
    }

    private IEnumerator AnimateDestroy()
    {
        float timer = 0f;

        // Simpan ukuran saat ini (antisipasi jika dihancurkan saat belum selesai spawn)
        Vector3 startScale = transform.localScale;

        while (timer < destroyDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / destroyDuration;

            // Ambil nilai kurva (1 -> 1.1 -> 0)
            float curveValue = destroyCurve.Evaluate(progress);

            // Terapkan scale (Tanpa squash stretch biar cepat & clean saat hapus)
            // Kita kalikan dengan initialScale supaya proporsinya tetap benar
            transform.localScale = initialScale * curveValue;

            yield return null;
        }

        // Scale sudah 0, sekarang benar-benar hapus dari dunia game
        transform.localScale = Vector3.zero;
        Destroy(gameObject);
        
        // Jika parentnya (BlockRoot) terpisah dan perlu dihapus juga:
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    private IEnumerator AnimateBounce()
    {
        // ... (Kode spawn yang lama tetap sama disini) ...
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float curveValue = bounceCurve.Evaluate(progress);
            float deviation = curveValue - 1f;
            float factorXZ = 1f - (deviation * squashAmount);
            float finalY = initialScale.y * curveValue;
            float finalXZ = initialScale.x * factorXZ;
            if (curveValue < 0.1f) finalXZ = initialScale.x * curveValue;

            transform.localScale = new Vector3(finalXZ, finalY, finalXZ);
            yield return null;
        }
        transform.localScale = initialScale;
    }
}