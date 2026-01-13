using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    [Header("Objets à spawner")]
    public GameObject[] objetsASpawner;
    
    [Header("Paramètres de spawn")]
    public int nombreObjets = 10;
    public float rayonSpawn = 5f;
    public bool spawnAuDemarrage = true;
    public bool spawnContinu = false;
    public float delaiEntreSpawns = 2f;
    
    [Header("Personnalisation avancée")]
    public bool positionAleatoire = true;
    public bool rotationAleatoire = true;
    public bool echelleAleatoire = false;
    public Vector3 plageEchelle = new Vector3(0.5f, 0.5f, 0.5f);
    public bool alignerAvecSurface = false;
    public float distanceSurface = 0.1f;
    public LayerMask couchesSurface;
    
    private float prochainSpawn;
    
    void Start()
    {
        if (spawnAuDemarrage)
        {
            SpawnObjets();
        }
        
        if (spawnContinu)
        {
            prochainSpawn = Time.time + delaiEntreSpawns;
        }
    }

    void Update()
    {
        if (spawnContinu && Time.time >= prochainSpawn)
        {
            SpawnObjets();
            prochainSpawn = Time.time + delaiEntreSpawns;
        }
    }
    
    public void SpawnObjets()
    {
        if (objetsASpawner == null || objetsASpawner.Length == 0)
        {
            Debug.LogWarning("Aucun objet à spawner défini!");
            return;
        }
        
        for (int i = 0; i < nombreObjets; i++)
        {
            SpawnObjetUnique();
        }
    }
    
    private void SpawnObjetUnique()
    {
        GameObject objetPrefab = objetsASpawner[Random.Range(0, objetsASpawner.Length)];
        Vector3 positionSpawn = CalculerPositionSpawn();
        Quaternion rotationSpawn = CalculerRotationSpawn();
        Vector3 echelleSpawn = CalculerEchelleSpawn();
        
        GameObject objetSpawne = Instantiate(objetPrefab, positionSpawn, rotationSpawn);
        objetSpawne.transform.localScale = echelleSpawn;
        
        if (alignerAvecSurface)
        {
            AlignerAvecSurface(objetSpawne);
        }
    }
    
    private Vector3 CalculerPositionSpawn()
    {
        if (positionAleatoire)
        {
            Vector2 pointAleatoire = Random.insideUnitCircle * rayonSpawn;
            return transform.position + new Vector3(pointAleatoire.x, 0, pointAleatoire.y);
        }
        else
        {
            float angle = (float)objetsASpawner.Length / nombreObjets * 2 * Mathf.PI;
            float x = Mathf.Cos(angle) * rayonSpawn;
            float z = Mathf.Sin(angle) * rayonSpawn;
            return transform.position + new Vector3(x, 0, z);
        }
    }
    
    private Quaternion CalculerRotationSpawn()
    {
        if (rotationAleatoire)
        {
            return Random.rotation;
        }
        else
        {
            return Quaternion.identity;
        }
    }
    
    private Vector3 CalculerEchelleSpawn()
    {
        if (echelleAleatoire)
        {
            float echelleX = Random.Range(1f - plageEchelle.x, 1f + plageEchelle.x);
            float echelleY = Random.Range(1f - plageEchelle.y, 1f + plageEchelle.y);
            float echelleZ = Random.Range(1f - plageEchelle.z, 1f + plageEchelle.z);
            return new Vector3(echelleX, echelleY, echelleZ);
        }
        else
        {
            return Vector3.one;
        }
    }
    
    private void AlignerAvecSurface(GameObject objet)
    {
        RaycastHit hit;
        Vector3 directionBas = Vector3.down;
        
        if (Physics.Raycast(objet.transform.position + Vector3.up * 10f, directionBas, out hit, 20f, couchesSurface))
        {
            objet.transform.position = hit.point + hit.normal * distanceSurface;
            objet.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rayonSpawn);
        
        if (objetsASpawner != null && objetsASpawner.Length > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < Mathf.Min(nombreObjets, 20); i++)
            {
                Vector2 pointAleatoire = Random.insideUnitCircle * rayonSpawn;
                Vector3 positionTest = transform.position + new Vector3(pointAleatoire.x, 0, pointAleatoire.y);
                Gizmos.DrawWireSphere(positionTest, 0.2f);
            }
        }
    }
}
