using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Pengaturan Kecepatan")]
    [Tooltip("Kecepatan maju pesawat")]
    public float kecepatanMaju = 20f;

    [Tooltip("Kecepatan rotasi pitch (naik/turun)")]
    public float kecepatanPitch = 50f;

    [Tooltip("Kecepatan rotasi yaw (belok kiri/kanan)")]
    public float kecepatanYaw = 50f;

    [Tooltip("Kecepatan rotasi roll (miring kiri/kanan)")]
    public float kecepatanRoll = 100f;

    [Header("Pengaturan Physics")]
    [Tooltip("Kekuatan gravitasi yang mempengaruhi pesawat")]
    public float gravitasi = 9.8f;

    [Tooltip("Kekuatan lift (angkat) pesawat")]
    public float lift = 15f;

    private Rigidbody rb;
    private float inputPitch;
    private float inputYaw;
    private float inputRoll;

    void Start()
    {
        // Ambil komponen Rigidbody
        rb = GetComponent<Rigidbody>();

        // Pastikan Rigidbody ada
        if (rb == null)
        {
            Debug.LogError("Rigidbody tidak ditemukan! Tambahkan Rigidbody ke pesawat.");
        }
        else
        {
            // Nonaktifkan gravity default Unity
            rb.useGravity = false;
        }
    }

    void Update()
    {
        // Ambil input dari keyboard
        AmbilInput();
    }

    void FixedUpdate()
    {
        // Terapkan gerakan dan rotasi
        if (rb != null)
        {
            GerakkanPesawat();
            RotasiPesawat();
            TerapkanPhysics();
        }
    }

    void AmbilInput()
    {
        // W/S atau Arrow Up/Down untuk Pitch (naik/turun)
        inputPitch = -Input.GetAxis("Vertical");

        // A/D atau Arrow Left/Right untuk Yaw (belok)
        inputYaw = Input.GetAxis("Horizontal");

        // Q/E untuk Roll (miring)
        inputRoll = 0f;
        if (Input.GetKey(KeyCode.Q))
            inputRoll = -1f;
        if (Input.GetKey(KeyCode.E))
            inputRoll = 1f;
    }

    void GerakkanPesawat()
    {
        // Gerakkan pesawat ke depan terus menerus
        Vector3 gerakan = transform.forward * kecepatanMaju;
        rb.linearVelocity = gerakan;
    }

    void RotasiPesawat()
    {
        // Hitung rotasi berdasarkan input
        float pitch = inputPitch * kecepatanPitch * Time.fixedDeltaTime;
        float yaw = inputYaw * kecepatanYaw * Time.fixedDeltaTime;
        float roll = inputRoll * kecepatanRoll * Time.fixedDeltaTime;

        // Terapkan rotasi
        transform.Rotate(pitch, yaw, roll, Space.Self);
    }

    void TerapkanPhysics()
    {
        // Terapkan gravitasi custom
        Vector3 gravitasiForce = Vector3.down * gravitasi;
        rb.AddForce(gravitasiForce, ForceMode.Acceleration);

        // Terapkan lift berdasarkan kecepatan dan orientasi pesawat
        float liftForce = rb.linearVelocity.magnitude * lift;
        Vector3 liftDirection = transform.up;
        rb.AddForce(liftDirection * liftForce, ForceMode.Force);
    }

    // Fungsi untuk menampilkan info di Scene view
    void OnDrawGizmos()
    {
        // Tampilkan arah depan pesawat
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);

        // Tampilkan arah atas pesawat
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 3f);
    }
}