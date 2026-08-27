using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshipTurret : MonoBehaviour
{

    [Header("Referencias")]
    public Transform PlayerCamara;
    public GameObject SpawnPoint;
    public GameObject BulletContainer;
    public GameObject PackageContainer;

    [Header("Configuración de Sensibilidad")]
    public float sensibilidad = 100f;

    [Header("Límites de la Torreta (Grados Locales)")]
    public float minX = -150f;  // Izquierda (Yaw)
    public float maxX = -30f;   // Derecha (Yaw)

    // Límites verticales corregidos para la lógica de Unity
    public float minY = -20f;   // Hacia abajo (Pitch)
    public float maxY = 70f;    // Hacia arriba (Pitch)
    // Acumuladores de rotación local
    private float currentRotationY = 0f; 
    private float currentRotationX = 0f; 

    [Header("Shooting Vars")]
    public float fireRate = 0.30f;
    private float currentTime = 0.0f;

    [Header("Bullet Prefabs")]
    public bullet bulletPrefab;
    public Package packagePrefab;

    [Header("ObjectPooling Vars")]
    public int InitialPoolSize = 50;
    private Queue<bullet> SleepingBulletPool = new Queue<bullet>();

    [Header("Debug")]
    public bool canMouse = true;

    // === Automata de Estados de torreta === //
    public enum EstadoTorreta 
    {
    Armed,
    Auxiliary
    }

    private bool canSwitch = true;

    public EstadoTorreta estadoActual;


    // Start is called before the first frame update
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        initializeObjectPooling();

        estadoActual = EstadoTorreta.Auxiliary;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(canMouse){
            aim(Time.deltaTime);
        }
        

        shoot(Time.deltaTime);

        if(canSwitch){
            if(Input.GetButton("Switch")){
                print("¡SWITCHED!");
                if(estadoActual == EstadoTorreta.Armed){
                    estadoActual = EstadoTorreta.Auxiliary;
                }
                else{
                    estadoActual = EstadoTorreta.Armed;
                }
            }
        }
        
        

    }

    void initializeObjectPooling(){
        for(int i = 0; i< InitialPoolSize; i++){
            bullet nuevaBala = Instantiate(bulletPrefab,Vector3.zero, Quaternion.identity);
            nuevaBala.transform.SetParent(BulletContainer.transform);
            nuevaBala.deactivate();
            nuevaBala.onBulletDeactivated += RegresarBalaAlPool;
            //La registramos en el Object Pooling :)
            SleepingBulletPool.Enqueue(nuevaBala);
        }

    }


    //Esta funcion es llamada por AirCraft (Padre)
    public void InicializarAngulos(float yawInicial, float pitchInicial){
    print("Initialized Turret Position");
    
    // 1. Seteamos los acumuladores
    currentRotationY = yawInicial; // Recibe -90f
    
    // Si la cámara mira al revés al iniciar, multiplicamos por -1 para invertir el signo del ángulo del triángulo
    currentRotationX = pitchInicial; 

    // 2. Aplicamos un Clamp inmediato 
    currentRotationY = Mathf.Clamp(currentRotationY, minX, maxX);
    currentRotationX = Mathf.Clamp(currentRotationX, minY, maxY);

    // 3. Aplicamos las rotaciones iniciales directamente a los componentes
    transform.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
    if (PlayerCamara != null)
    {
        PlayerCamara.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
    }
}

    //Llamada en Update() cada Frame
    void aim(float delta){

        //1. Leemos los inputs del mouse
        float mouseRawX = Input.GetAxis("Mouse X") * sensibilidad * delta;
        float mouseRawY = Input.GetAxis("Mouse Y") * sensibilidad * delta;
        //print("Current Y rotation: " + mouseRawY);
        //print("Current X rotation: " + mouseRawX);


        //2. Los acumulamos y limitamos
        currentRotationX -= mouseRawY;
        currentRotationX = Mathf.Clamp(currentRotationX,minY,maxY);

        currentRotationY += mouseRawX;
        currentRotationY = Mathf.Clamp(currentRotationY,minX,maxX);

        //3. Aplicar la rotacion en coordenadas locales
        transform.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
        //print("Local rotation: " + transform.localRotation);

        // La cámara rota sobre el eje X del pivot (arriba/abajo)
        if (PlayerCamara != null)
        {
            PlayerCamara.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
            //print("Camera Rotation: " + PlayerCamara.localRotation);
        }


    }

    private int num = 0;

    void shoot(float delta){

        if(estadoActual == EstadoTorreta.Armed){

                if(currentTime > 0){
                currentTime -= delta;
            }

            if(Input.GetButton("Fire1") && currentTime <= 0){
                num += 1;

                bullet newBullet;
                if(SleepingBulletPool.Count > 0){
                    newBullet = SleepingBulletPool.Dequeue();
                    newBullet.activate(SpawnPoint.transform.position, PlayerCamara.transform.rotation);

                } else{
                    //AutoEscalamiento
                    newBullet = Instantiate(bulletPrefab,Vector3.zero, Quaternion.identity);
                    newBullet.transform.SetParent(BulletContainer.transform);

                    //print("Spawn point en: " + SpawnPoint.transform.position);
                    newBullet.activate(SpawnPoint.transform.position, PlayerCamara.transform.rotation);

                }

                //print("Object Pool: " + SleepingBulletPool.Count);
                currentTime = fireRate;
            }


        }
        else if(estadoActual == EstadoTorreta.Auxiliary){
            if(Input.GetButton("Fire1")){
                Package newPackage = Instantiate(packagePrefab,SpawnPoint.transform.position, PlayerCamara.transform.rotation);
                newPackage.transform.SetParent(PackageContainer.transform);
                estadoActual = EstadoTorreta.Armed;
                // Despues de lanzar un paquete ya no puedes lanzar mas hasta que Game Master te deje
                canSwitch = false;
            }

        }

        
    }

    //Estas balas ya llegan despues de hacer deactivate();
    void RegresarBalaAlPool(bullet balaDormida){
        SleepingBulletPool.Enqueue(balaDormida);
        //print("Object Pool: " + SleepingBulletPool.Count);
    }

    //Llamado por Game master
    public void reset(){
        print("Can Switch");
        canSwitch = true;
    }
}
